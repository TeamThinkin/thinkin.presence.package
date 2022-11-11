using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[NetworkSync(typeof(DispenserElementPresenter), "Presenters/Dispenser/Dispenser Sync")]
public class DispenserSync : RealtimeComponent<DispenserSyncModel>, INetworkSync
{
    private static Dictionary<string, DispenserSync> existingSyncs = new Dictionary<string, DispenserSync>();

    private DispenserElementPresenter presenter;
    private RealtimeView parentView;

    public DispenserSyncModel Model => model;

    public bool DebugIsOwnedLocallySelf;


    public void SetTarget(GameObject LocalTarget)
    {
        model.key = LocalTarget.name;
    }

    public void RequestSyncOwnership()
    {
        parentView.RequestOwnership();
        base.RequestOwnership();
    }

    private void Awake()
    {
        parentView = GetComponent<RealtimeView>();
    }

    private void OnDestroy()
    {
        if (model != null)
        {
            model.keyDidChange -= Model_keyDidChange;
            model.counterDidChange -= Model_counterDidChange;
        }

        if(presenter != null)
        {
            presenter.OnUserInput -= Presenter_OnUserInput;
            presenter.OnItemDispensed -= Presenter_OnItemDispensed;
        }
    }

    private void Update()
    {
        if (model == null || presenter == null) return;

        if(isOwnedLocallySelf)
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
            previousModel.keyDidChange -= Model_keyDidChange;
            previousModel.counterDidChange -= Model_counterDidChange;
        }

        if(currentModel != null)
        {
            currentModel.keyDidChange += Model_keyDidChange;
            currentModel.counterDidChange += Model_counterDidChange;
            Model_keyDidChange(currentModel, currentModel.key);
        }
    }

    private void Model_counterDidChange(DispenserSyncModel model, int value)
    {
        presenter.ItemCounter = value;
    }

    private void Model_keyDidChange(DispenserSyncModel model, string value)
    {
        presenter = null;
        if (!string.IsNullOrEmpty(model.key))
        {
            if (existingSyncs.ContainsKey(model.key))
            {
                Realtime.Destroy(this.gameObject);
            }

            var item = GameObject.Find(model.key);
            if (item != null)
            {
                presenter = item.GetComponent<DispenserElementPresenter>();
                presenter.OnUserInput += Presenter_OnUserInput;
                presenter.OnItemDispensed += Presenter_OnItemDispensed;
            }
        }
    }


    private void Presenter_OnItemDispensed(GameObject Item, DispenserElementPresenter.ItemInfo Info)
    {
        Debug.Log("Dispenser Sync sees a new item has been dispensed: " + Item.name + ": " + Info.AssetSourceUrl);
        RequestSyncOwnership();
        model.counter = presenter.ItemCounter;
        var itemSync = NetworkSyncFactory.FindOrCreateNetworkSync(Item, GrabbableSync.PrefabPath) as GrabbableSync;
        Debug.Log("Setting grabbable asset url to: " + Info.AssetSourceUrl);
        itemSync.SetSpawnUrl(Info.AssetSourceUrl);
    }

    private void Presenter_OnUserInput()
    {
        RequestSyncOwnership();
    }
}
