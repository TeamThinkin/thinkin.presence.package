using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkItemSync : RealtimeComponent<NetworkItemSyncModel>, INetworkSync
{
    //public static Dictionary<string, NetworkItemSync> Syncs { get; private set; } = new Dictionary<string, NetworkItemSync>();
    public string TargetItemName;

    [SerializeField] private RealtimeView NetworkItem;
    [SerializeField] private RealtimeTransform NetworkTransform;

    public GameObject TargetItem { get; private set; }

    private bool hasTargetItem;

    public void SetTarget(GameObject LocalTarget)
    {
        //Will be called from the local instance that created the target item.
        //Aka the target item exists here, but doesnt on the remote instance(?)
        //gameObject.name = "Network Item Sync for " + LocalTarget.name;
        TargetItem = LocalTarget;
        TargetItemName = TargetItem.name;
        hasTargetItem = true;

        model.key = TargetItem.name;

        var spawnableItem = LocalTarget.GetComponent<ISpawnableItem>();
        if(spawnableItem != null)
        {
            model.spawnItemPrefabPath = spawnableItem.PrefabPath;
        }
        
    }

    public void RequestSyncOwnership()
    {
        RequestTransformOwnership();
    }

    public void RequestTransformOwnership()
    {
        NetworkItem.RequestOwnership();
        NetworkTransform.RequestOwnership();
    }

    //public static NetworkItemSync FindOrCreate(GameObject TargetItem, string SpawnUrl = null)
    //{
    //    if (!TelepresenceRoomManager.Instance.IsConnected) return null;

    //    string key = TargetItem.name; 

    //    if (Syncs.ContainsKey(key))
    //    {
    //        var sync = Syncs[key];
    //        sync.RequestTransformOwnership();
    //        return sync;
    //    }
    //    else
    //    {
    //        var sync = Normal.Realtime.Realtime.Instantiate("Prefabs/NetworkItemSync", Normal.Realtime.Realtime.InstantiateOptions.defaults).GetComponent<NetworkItemSync>();
    //        sync.model.key = key;
    //        sync.model.spawnUrl = SpawnUrl;
    //        sync.model.spawnParentKey = TargetItem.transform.parent.gameObject.name;
    //        sync.RequestTransformOwnership();
    //        Syncs.Add(key, sync);
    //        return sync;
    //    }
    //}

    private void OnDestroy()
    {
        //var entry = Syncs.FirstOrDefault(i => i.Value == this);
        //if (entry.Value == this)
        //{
        //    Syncs.Remove(entry.Key);
        //}

        if(TargetItem != null)
        {
            Destroy(TargetItem);
        }
    }

    private void Update()
    {
        if (!hasTargetItem) return;
        if (TargetItem == null)
        {
            //Our target item has been destroyed
            Realtime.Destroy(this.gameObject);
            return;
        }

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
            //previousModel.spawnUrlDidChange -= Model_spawnUrlDidChange;
        }
        if(currentModel != null)
        {
            Model_keyDidChange(currentModel, currentModel.key);
            //Model_spawnUrlDidChange(currentModel, currentModel.spawnUrl);

            currentModel.keyDidChange += Model_keyDidChange;
            //currentModel.spawnUrlDidChange += Model_spawnUrlDidChange;

            //if (currentModel.key != null && !Syncs.ContainsKey(currentModel.key)) Syncs.Add(currentModel.key, this);
        }  
    }

    private void Model_keyDidChange(NetworkItemSyncModel model, string key)
    {
        Debug.Log("Network Item Sync Key changed to: " + key);
        if (string.IsNullOrEmpty(key)) return;
        if (TargetItem != null && TargetItem.name == key)
        {
            Debug.Log("We are already set with the target item, no need to do anything");
            return;
        }

        var existingItem = GameObject.Find(key);
        if(existingItem != null)
        {
            //Item is already in the scene just need to attach to it
            Debug.Log("Item is already in the scene just need to attach to it");
            TargetItem = existingItem;
            hasTargetItem = true;
        }
        if(!string.IsNullOrEmpty(model.spawnItemPrefabPath))
        {
            //Item is NOT already in scene AND we have a prefab path. Now we need to instantiate a local instance of it and attach sync
            Debug.Log("Spawning new local instance to go with network sync");
            Debug.Log("Prefab path: " + model.spawnItemPrefabPath);
            var prefab = Resources.Load<GameObject>(model.spawnItemPrefabPath);
            Debug.Log(prefab == null);
            var item = Instantiate(prefab);
            item.name = key;
            TargetItem = item;
            hasTargetItem = true;
        }
        TargetItemName = TargetItem?.name;
        //if (!string.IsNullOrEmpty(model.key))
        //    TargetItem = GameObject.Find(model.key);
        //else
        //    TargetItem = null;
    }

    //private async void Model_spawnUrlDidChange(NetworkItemSyncModel model, string value)
    //{
    //    //if(TargetItem == null && !string.IsNullOrEmpty(model.spawnUrl))
    //    //{
    //    //    Debug.Log("Spawning new item: " + model.spawnUrl);

    //    //    var address = new AssetUrl(model.spawnUrl);
    //    //    Debug.Log(address.CatalogUrl);
    //    //    Debug.Log(address.AssetPath);

    //    //    var prefab = await AssetBundleManager.LoadPrefab(address);
    //    //    var parentObject = GameObject.Find(model.spawnParentKey);
    //    //    TargetItem = Instantiate(prefab, parentObject?.transform);
    //    //    TargetItem.name = model.key;

    //    //    //MakeGrabbable(TargetItem); //TODO: commented out during the Package refactor
    //    //}
    //}


    private void copyTransform(Transform sourceItem, Transform destinationItem)
    {
        destinationItem.position = sourceItem.position;
        destinationItem.rotation = sourceItem.rotation;
        destinationItem.localScale = sourceItem.localScale;
    }

}
