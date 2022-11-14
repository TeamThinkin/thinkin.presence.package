using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrabbableSync : TransformSync
{
    public override void SetTarget(GameObject LocalTarget)
    {
        base.SetTarget(LocalTarget);
        AppControllerBase.Instance.UIManager.MakeGrabbable(LocalTarget);
    }
    //public const string PrefabPath = "Prefabs/GrabbableItemSync";
    //public static Dictionary<string, GrabbableSync> Syncs { get; private set; } = new Dictionary<string, GrabbableSync>();

    //[SerializeField] private RealtimeView NetworkItem;
    //[SerializeField] private RealtimeTransform NetworkTransform;

    //public GameObject TargetItem { get; private set; }

    //private IGrabbable grabbable;

    //public void SetTarget(GameObject LocalTarget)
    //{
    //    TargetItem = LocalTarget;
    //    model.key = TargetItem.name;
    //}

    //public void SetSpawnUrl(string Url)
    //{
    //    model.spawnUrl = Url;
    //}

    //public void RequestSyncOwnership()
    //{
    //    NetworkItem.RequestOwnership();
    //    NetworkTransform.RequestOwnership();
    //}

    //private void OnDestroy()
    //{
    //    var entry = Syncs.FirstOrDefault(i => i.Value == this);
    //    if (entry.Value == this)
    //    {
    //        Syncs.Remove(entry.Key);
    //    }

    //    if(grabbable != null)
    //    {
    //        grabbable.OnGrab -= Grabbable_OnGrab;
    //    }
    //}

    //private void Update()
    //{
    //    if (TargetItem == null) return;

    //    if (NetworkTransform.isOwnedLocallySelf)
    //    {
    //        copyTransform(TargetItem.transform, this.transform);
    //        //Debug.Log("Copying local position to remote objects: " + TargetItem.name);
    //    }
    //    else
    //    {
    //        //Debug.Log("Copy remote position to local object: " + TargetItem.name);
    //        copyTransform(this.transform, TargetItem.transform);
    //    }
    //}

    //protected override void OnRealtimeModelReplaced(GrabbableSyncModel previousModel, GrabbableSyncModel currentModel)
    //{
    //    base.OnRealtimeModelReplaced(previousModel, currentModel);
    //    if (previousModel != null)
    //    {
    //        previousModel.keyDidChange -= Model_keyDidChange;
    //        previousModel.spawnUrlDidChange -= Model_spawnUrlDidChange;
    //    }
    //    if (currentModel != null)
    //    {
    //        Model_keyDidChange(currentModel, currentModel.key);
    //        Model_spawnUrlDidChange(currentModel, currentModel.spawnUrl);

    //        currentModel.keyDidChange += Model_keyDidChange;
    //        currentModel.spawnUrlDidChange += Model_spawnUrlDidChange;

    //        if (currentModel.key != null && !Syncs.ContainsKey(currentModel.key)) Syncs.Add(currentModel.key, this);
    //    }
    //}

    //private async void Model_spawnUrlDidChange(GrabbableSyncModel model, string value)
    //{
    //    if (TargetItem == null && model.spawnUrl != null && !string.IsNullOrEmpty(model.spawnUrl.Trim()))
    //    {
    //        Debug.Log("Spawning new item: *" + model.spawnUrl + "* *" + model.key + "*");

    //        var address = new AssetUrl(model.spawnUrl);
    //        var prefab = await AssetBundleManager.LoadPrefab(address);
    //        var parentObject = GameObject.Find(model.spawnParentKey);
    //        TargetItem = Instantiate(prefab, parentObject?.transform);
    //        TargetItem.name = model.key;

    //        AppControllerBase.Instance.UIManager.MakeGrabbable(TargetItem);

    //        onNewTargetItem();
    //    }
    //}

    //private void Model_keyDidChange(GrabbableSyncModel model, string value)
    //{
    //    if (!string.IsNullOrEmpty(model.key))
    //    {
    //        Debug.Log("Grabbable Sync Key changed, looking for Target Item in scene: " + model.spawnUrl);
    //        TargetItem = GameObject.Find(model.key);
    //        onNewTargetItem();
    //    }
    //    else TargetItem = null;
    //}

    //private void onNewTargetItem()
    //{
    //    if (TargetItem == null) return;

    //    var grabbable = TargetItem.GetComponent<IGrabbable>();
    //    if(grabbable != null)
    //    {
    //        grabbable.OnGrab += Grabbable_OnGrab;
    //    }
    //    else
    //    {
    //        Debug.Log("New Target Item does not have an IGrabbable component");
    //    }
    //}

    //private void Grabbable_OnGrab(IGrabber Grabber, IGrabbable Grabbable)
    //{
    //    RequestSyncOwnership();
    //}

    //private void copyTransform(Transform sourceItem, Transform destinationItem)
    //{
    //    destinationItem.position = sourceItem.position;
    //    destinationItem.rotation = sourceItem.rotation;
    //    destinationItem.localScale = sourceItem.localScale;
    //}
}
