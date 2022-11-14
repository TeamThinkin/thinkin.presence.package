using Normal.Realtime;
using System.Collections;
using UnityEngine;

public interface ISyncModel
{
    string key { get; set; }
    string spawnItemPrefabPath { get; set; }
    string spawnUrl { get; set; }
    string spawnParentKey { get; set; }
    
    public delegate void PropertyChangedHandler<in TValue>(ISyncModel model, TValue value);
    event PropertyChangedHandler<string> keyDidChange;
    event PropertyChangedHandler<string> spawnItemPrefabPathDidChange;
    event PropertyChangedHandler<string> spawnUrlDidChange;
    event PropertyChangedHandler<string> spawnParentKeyDidChange;
}

public abstract class NetworkSyncBase<T> : RealtimeComponent<T>, INetworkSync where T : RealtimeModel, ISyncModel, new()
{
    public string TargetItemName;

    [SerializeField] private RealtimeView NetworkItem;

    public GameObject TargetItem { get; protected set; }
    protected bool hasTargetItem;

    public virtual void RequestSyncOwnership() 
    {
        NetworkItem.RequestOwnership();
        if (this.model.hasMetaModel) this.RequestOwnership();
    }
    
    public virtual void SetTarget(GameObject LocalTarget)
    {
        //Will be called from the local instance that created the target item.
        //Aka the target item exists here, but doesnt on the remote instance(?)
        //gameObject.name = "Network Item Sync for " + LocalTarget.name;
        TargetItem = LocalTarget;
        TargetItemName = TargetItem.name;
        hasTargetItem = true;

        var syncModel = model as ISyncModel;
        if(syncModel.key != TargetItem.name) syncModel.key = TargetItem.name;

        var spawnableItem = LocalTarget.GetComponent<ISpawnableItem>();
        if (spawnableItem != null)
        {
            syncModel.spawnItemPrefabPath = spawnableItem.PrefabPath;
        }
    }

    public void SetSpawnUrl(string Url)
    {
        (model as ISyncModel).spawnUrl = Url;
    }

    protected virtual void Start()
    {
        TelepresenceRoomManager.Instance.RegisterSync(this);
    }

    protected virtual void OnDestroy()
    {
        TelepresenceRoomManager.Instance?.UnregisterSync(this);
        if (TargetItem != null)
        {
            Destroy(TargetItem);
        }
    }

    protected virtual void Update()
    {
        if (!hasTargetItem) return;
        if (TargetItem == null)
        {
            //Our target item has been destroyed
            Realtime.Destroy(this.gameObject);
            hasTargetItem=false;
            return;
        }
    }


    protected override void OnRealtimeModelReplaced(T previousModel, T currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
        if (previousModel != null)
        {
            previousModel.keyDidChange -= Model_keyDidChange;
            //previousModel.spawnUrlDidChange -= Model_spawnUrlDidChange;
        }
        if (currentModel != null)
        {
            Model_keyDidChange(currentModel, currentModel.key);
            Model_spawnUrlDidChange(currentModel, currentModel.spawnUrl);

            currentModel.keyDidChange += Model_keyDidChange;
            currentModel.spawnUrlDidChange += Model_spawnUrlDidChange;

            //if (currentModel.key != null && !Syncs.ContainsKey(currentModel.key)) Syncs.Add(currentModel.key, this);
        }
    }

    private void Model_keyDidChange(ISyncModel model, string key)
    {
        Debug.Log("Network Item Sync Key changed to: " + key);
        if (string.IsNullOrEmpty(key)) return;
        if (TargetItem != null && TargetItem.name == key)
        {
            Debug.Log("We are already set with the target item, no need to do anything");
            return;
        }

        var existingItem = GameObject.Find(key);
        if (existingItem != null)
        {
            //Item is already in the scene just need to attach to it
            Debug.Log("Item is already in the scene just need to attach to it");
            SetTarget(existingItem);
            //TargetItem = existingItem;
            //hasTargetItem = true;
        }
        if (!string.IsNullOrEmpty(model.spawnItemPrefabPath))
        {
            //Item is NOT already in scene AND we have a prefab path. Now we need to instantiate a local instance of it and attach sync
            Debug.Log("Spawning new local instance to go with network sync");
            var prefab = Resources.Load<GameObject>(model.spawnItemPrefabPath);
            var item = Instantiate(prefab);
            item.name = key;
            SetTarget(item);
            //TargetItem = item;
            //hasTargetItem = true;
        }
        TargetItemName = TargetItem?.name;
        //if (!string.IsNullOrEmpty(model.key))
        //    TargetItem = GameObject.Find(model.key);
        //else
        //    TargetItem = null;
    }

    private async void Model_spawnUrlDidChange(ISyncModel model, string value)
    {
        if (TargetItem == null && !string.IsNullOrEmpty(model.spawnUrl))
        {
            Debug.Log("Spawning new url item: " + model.spawnUrl);

            var address = new AssetUrl(model.spawnUrl);
            Debug.Log(address.CatalogUrl);
            Debug.Log(address.AssetPath);

            var prefab = await AssetBundleManager.LoadPrefab(address);
            var parentObject = GameObject.Find(model.spawnParentKey);
            TargetItem = Instantiate(prefab, parentObject?.transform);
            TargetItem.name = model.key;

            AppControllerBase.Instance.UIManager.MakeGrabbable(TargetItem);
        }
    }
}
