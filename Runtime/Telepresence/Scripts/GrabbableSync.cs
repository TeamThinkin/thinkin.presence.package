using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabbableSync : RealtimeComponent<GrabbableSyncModel>, INetworkSync
{
    public const string PrefabPath = "Prefabs/GrabbableItemSync";
    public static Dictionary<string, GrabbableSync> Syncs { get; private set; } = new Dictionary<string, GrabbableSync>();

    [SerializeField] private RealtimeView NetworkItem;
    [SerializeField] private RealtimeTransform NetworkTransform;

    public GameObject TargetItem { get; private set; }

    public void SetTarget(GameObject LocalTarget)
    {
        TargetItem = LocalTarget;
        model.key = TargetItem.name;
    }

    public void SetSpawnUrl(string Url)
    {
        model.spawnUrl = Url;
    }

    public void RequestSyncOwnership()
    {
        Debug.Log("Requesting ownership of GrabbableSync");
        NetworkItem.RequestOwnership();
        NetworkTransform.RequestOwnership();
    }

    //public static GrabbableSync FindOrCreate(GameObject TargetItem, string SpawnUrl = null)
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
    //        var sync = Normal.Realtime.Realtime.Instantiate("Prefabs/NetworkItemSync", Normal.Realtime.Realtime.InstantiateOptions.defaults).GetComponent<GrabbableSync>();
    //        sync.model.key = key;
    //        sync.model.spawnUrl = SpawnUrl;
    //        sync.model.spawnParentKey = TargetItem.transform.parent.gameObject.name;
    //        sync.RequestTransformOwnership();
    //        Syncs.Add(key, sync);
    //        return sync;
    //    }
    //}

    //TODO: commented out during the Package refactor
    //public static void MakeGrabbable(GameObject item) //TODO: This method doesnt belong in the class, need to find a way to create the same item in DispenserItem as when its spawned in this class
    //{
    //    var body = item.AddComponent<Rigidbody>();
    //    body.useGravity = false;
    //    body.drag = 0.2f;
    //    body.angularDrag = 0.2f;
    //    //body.isKinematic = true;
    //    //checkPhysicsMaterials(item);

    //    item.AddComponent<Grabbable>();
    //    item.AddComponent<DistanceGrabbable>();
    //    item.AddComponent<GrabSyncMonitor>();
    //}

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

        if (NetworkTransform.isOwnedLocallySelf)
        {
            copyTransform(TargetItem.transform, this.transform);
            Debug.Log("Copying local position to remote objects: " + TargetItem.name);
        }
        else
        {
            Debug.Log("Copy remote position to local object: " + TargetItem.name);
            copyTransform(this.transform, TargetItem.transform);
        }
    }

    protected override void OnRealtimeModelReplaced(GrabbableSyncModel previousModel, GrabbableSyncModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
        if (previousModel != null)
        {
            previousModel.keyDidChange -= Model_keyDidChange;
            previousModel.spawnUrlDidChange -= Model_spawnUrlDidChange;
        }
        if (currentModel != null)
        {
            Model_keyDidChange(currentModel, currentModel.key);
            Model_spawnUrlDidChange(currentModel, currentModel.spawnUrl);

            currentModel.keyDidChange += Model_keyDidChange;
            currentModel.spawnUrlDidChange += Model_spawnUrlDidChange;

            if (currentModel.key != null && !Syncs.ContainsKey(currentModel.key)) Syncs.Add(currentModel.key, this);
        }
    }

    private async void Model_spawnUrlDidChange(GrabbableSyncModel model, string value)
    {
        if (TargetItem == null && model.spawnUrl != null && !string.IsNullOrEmpty(model.spawnUrl.Trim()))
        {
            Debug.Log("Spawning new item: *" + model.spawnUrl + "* *" + model.key + "*");

            var address = new AssetUrl(model.spawnUrl);
            Debug.Log(address.CatalogUrl);
            Debug.Log(address.AssetPath);

            var prefab = await AssetBundleManager.LoadPrefab(address);
            var parentObject = GameObject.Find(model.spawnParentKey);
            TargetItem = Instantiate(prefab, parentObject?.transform);
            TargetItem.name = model.key;

            AppControllerBase.Instance.UIManager.MakeGrabbalbe(TargetItem);
        }
    }

    private void Model_keyDidChange(GrabbableSyncModel model, string value)
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
