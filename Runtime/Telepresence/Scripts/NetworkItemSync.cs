using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkItemSync : RealtimeComponent<NetworkItemSyncModel>
{
    public static Dictionary<string, NetworkItemSync> Syncs { get; private set; } = new Dictionary<string, NetworkItemSync>();

    [SerializeField] private RealtimeView NetworkItem;
    [SerializeField] private RealtimeTransform NetworkTransform;

    public GameObject TargetItem { get; private set; }

    public void RequestTransformOwnership()
    {
        NetworkItem.RequestOwnership();
        NetworkTransform.RequestOwnership();
    }

    public static NetworkItemSync FindOrCreate(GameObject TargetItem, string SpawnUrl = null)
    {
        if (!TelepresenceRoomManager.Instance.IsConnected) return null;

        string key = TargetItem.name; 

        if (Syncs.ContainsKey(key))
        {
            var sync = Syncs[key];
            sync.RequestTransformOwnership();
            return sync;
        }
        else
        {
            var sync = Normal.Realtime.Realtime.Instantiate("Prefabs/NetworkItemSync", Normal.Realtime.Realtime.InstantiateOptions.defaults).GetComponent<NetworkItemSync>();
            sync.model.key = key;
            sync.model.spawnUrl = SpawnUrl;
            sync.model.spawnParentKey = TargetItem.transform.parent.gameObject.name;
            sync.RequestTransformOwnership();
            Syncs.Add(key, sync);
            return sync;
        }
    }

    private void OnDestroy()
    {
        var entry = Syncs.FirstOrDefault(i => i.Value == this);
        if (entry.Value == this)
        {
            Syncs.Remove(entry.Key);
        }
    }

    private void Update()
    {
        if (TargetItem == null) return;

        if (NetworkTransform.isOwnedRemotelySelf)
            copyTransform(this.transform, TargetItem.transform);
        else
            copyTransform(TargetItem.transform, this.transform);
    }

    protected override void OnRealtimeModelReplaced(NetworkItemSyncModel previousModel, NetworkItemSyncModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
        if(previousModel != null) 
        {
            previousModel.keyDidChange -= Model_keyDidChange;
            previousModel.spawnUrlDidChange -= Model_spawnUrlDidChange;
        }
        if(currentModel != null)
        {
            Model_keyDidChange(currentModel, currentModel.key);
            Model_spawnUrlDidChange(currentModel, currentModel.spawnUrl);

            currentModel.keyDidChange += Model_keyDidChange;
            currentModel.spawnUrlDidChange += Model_spawnUrlDidChange;

            if (currentModel.key != null && !Syncs.ContainsKey(currentModel.key)) Syncs.Add(currentModel.key, this);
        }  
    }

    private async void Model_spawnUrlDidChange(NetworkItemSyncModel model, string value)
    {
        if(TargetItem == null && !string.IsNullOrEmpty(model.spawnUrl))
        {
            Debug.Log("Spawning new item: " + model.spawnUrl);

            var address = new AssetUrl(model.spawnUrl);
            Debug.Log(address.CatalogUrl);
            Debug.Log(address.AssetPath);

            var prefab = await AssetBundleManager.LoadPrefab(address);
            var parentObject = GameObject.Find(model.spawnParentKey);
            TargetItem = Instantiate(prefab, parentObject?.transform);
            TargetItem.name = model.key;

            //MakeGrabbable(TargetItem); //TODO: commented out during the Package refactor
        }
    }

    private void Model_keyDidChange(NetworkItemSyncModel model, string value)
    {
        if (!string.IsNullOrEmpty(model.key))
            TargetItem = GameObject.Find(model.key);
        else
            TargetItem = null;
    }

    private void copyTransform(Transform sourceItem, Transform destinationItem)
    {
        destinationItem.position = sourceItem.position;
        destinationItem.rotation = sourceItem.rotation;
        destinationItem.localScale = sourceItem.localScale;
    }
}
