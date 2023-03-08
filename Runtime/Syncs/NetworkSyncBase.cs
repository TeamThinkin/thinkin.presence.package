using Normal.Realtime;
using System.Collections;
using UnityEngine;

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
        ItemSpawnObserver.OnItemDespawned += ItemSpawnObserver_OnItemDespawned;
        TelepresenceRoomManager.Instance.RegisterSync(this);
    }

    protected virtual void OnDestroy()
    {
        ItemSpawnObserver.OnItemDespawned -= ItemSpawnObserver_OnItemDespawned;
        TelepresenceRoomManager.Instance?.UnregisterSync(this);
        if (hasTargetItem && TargetItem != null)
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
            OnTargetItemDestroyed();
            return;
        }
    }

    private void ItemSpawnObserver_OnItemDespawned(GameObject Item)
    {
        if (TargetItem == null) return;
        if (Item != TargetItem) return;

        OnTargetItemDestroyed();
    }

    protected void OnTargetItemDestroyed()
    {
        hasTargetItem = false;
        Realtime.Destroy(this.gameObject);
    }

    protected override void OnRealtimeModelReplaced(T previousModel, T currentModel)
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
        }
    }

    private void Model_keyDidChange(ISyncModel model, string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (TargetItem != null && TargetItem.name == key)
        {
            //Debug.Log("We are already set with the target item, no need to do anything");
            return;
        }

        var existingItem = GameObject.Find(key);
        if (existingItem != null)
        {
            //Item is already in the scene just need to attach to it
            //Debug.Log("Item is already in the scene just need to attach to it");
            SetTarget(existingItem);
        }
        if (!string.IsNullOrEmpty(model.spawnItemPrefabPath))
        {
            //Item is NOT already in scene AND we have a prefab path. Now we need to instantiate a local instance of it and attach sync
            //Debug.Log("Spawning new local instance to go with network sync");
            var prefab = Resources.Load<GameObject>(model.spawnItemPrefabPath);
            var item = Instantiate(prefab);
            item.name = key;
            SetTarget(item);
        }
        TargetItemName = TargetItem?.name;
    }

    private async void Model_spawnUrlDidChange(ISyncModel model, string value)
    {
        if (TargetItem == null && !string.IsNullOrEmpty(model.spawnUrl))
        {
            var address = new AssetUrl(model.spawnUrl);
            var prefab = await AssetBundleManager.LoadPrefab(address);
            var parentObject = GameObject.Find(model.spawnParentKey);
            TargetItem = Instantiate(prefab, parentObject?.transform);
            TargetItem.name = model.key;
            SetTarget(TargetItem);

            AppControllerBase.Instance.UIManager.MakeGrabbable(TargetItem);
        }
    }
}
