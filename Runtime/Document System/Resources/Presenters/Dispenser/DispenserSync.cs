using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserSync : NetworkSyncBase<DispenserSyncModel>
{
    private static Dictionary<string, DispenserSync> existingSyncs = new Dictionary<string, DispenserSync>();

    private DispenserElementPresenter presenter;

    public DispenserSyncModel Model => model;

    public bool DebugIsOwnedLocallySelf;

    override protected void OnDestroy()
    {
        base.OnDestroy();

        if (model != null)
        {
            model.counterDidChange -= Model_counterDidChange;
        }

        if(presenter != null)
        {
            presenter.OnUserInput -= Presenter_OnUserInput;
            presenter.OnItemDispensed -= Presenter_OnItemDispensed;
        }
    }

    override protected void Update()
    {
        base.Update();
        if (model == null || presenter == null) return;

        if (isOwnedLocallySelf)
        {
            DebugIsOwnedLocallySelf = true;
            Model.scrollValue = presenter.Scroll;
            //Model.counter = presenter.itemCounter;
        }
        else
        {
            DebugIsOwnedLocallySelf = false;
            presenter.Scroll = Model.scrollValue;
            //presenter.itemCounter = sync.Model.counter;
        }
    }

    protected override void OnRealtimeModelReplaced(DispenserSyncModel previousModel, DispenserSyncModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);

        if(previousModel != null)
        {
            previousModel.counterDidChange -= Model_counterDidChange;
        }

        if(currentModel != null)
        {
            currentModel.counterDidChange += Model_counterDidChange;
        }
    }

    private void Model_counterDidChange(ISyncModel model, int value)
    {
        presenter.ItemCounter = value;
    }

    public override void SetTarget(GameObject LocalTarget)
    {
        base.SetTarget(LocalTarget);

        if(TargetItem != null)
        {
            presenter = TargetItem.GetComponent<DispenserElementPresenter>();
            presenter.OnUserInput += Presenter_OnUserInput;
            presenter.OnItemDispensed += Presenter_OnItemDispensed;
        }
    }

    private void Presenter_OnItemDispensed(GameObject Item, DispenserElementPresenter.ItemInfo Info)
    {
        Debug.Log("Dispenser Sync sees a new item has been dispensed: " + Item.name + ": " + Info.AssetSourceUrl);
        RequestSyncOwnership();
        model.counter = presenter.ItemCounter;
        //var itemSync = NetworkSyncFactory.FindOrCreateNetworkSync(Item, GrabbableSync.PrefabPath) as GrabbableSync;
        //Debug.Log("Setting grabbable asset url to: " + Info.AssetSourceUrl);
        //itemSync.SetSpawnUrl(Info.AssetSourceUrl);

        var itemSync = NetworkSyncFactory.FindOrCreateNetworkSync(Item, typeof(TransformSync)) as TransformSync;
        itemSync.SetSpawnUrl(Info.AssetSourceUrl);
    }

    private void Presenter_OnUserInput()
    {
        RequestSyncOwnership();
    }
}
