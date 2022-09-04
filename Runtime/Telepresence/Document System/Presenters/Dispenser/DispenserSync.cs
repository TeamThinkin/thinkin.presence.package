using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserSync : RealtimeComponent<DispenserSyncModel>
{
    private static Dictionary<string, DispenserSync> existingSyncs = new Dictionary<string, DispenserSync>();

    private DispenserElementPresenter source;
    private RealtimeView parentView;

    public DispenserSyncModel Model => model;

    public static DispenserSync FindOrCreate(DispenserElementPresenter Source)
    {
        if (existingSyncs.ContainsKey(Source.name))
            return existingSyncs[Source.name];

        var options = Realtime.InstantiateOptions.defaults;
        options.ownedByClient = true;
        var sync = Realtime.Instantiate("Presenters/Dispenser/Dispenser Sync", options).GetComponent<DispenserSync>();
        sync.model.key = Source.name;
        return sync;
    }

    new public void RequestOwnership()
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
        }
    }

    protected override void OnRealtimeModelReplaced(DispenserSyncModel previousModel, DispenserSyncModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);

        if(previousModel != null)
        {
            previousModel.keyDidChange -= Model_keyDidChange;
        }

        if(currentModel != null)
        {
            currentModel.keyDidChange += Model_keyDidChange;
            Model_keyDidChange(currentModel, currentModel.key);
        }
    }

    private void Model_keyDidChange(DispenserSyncModel model, string value)
    {
        source = null;
        if (!string.IsNullOrEmpty(model.key))
        {
            if (existingSyncs.ContainsKey(model.key))
            {
                Realtime.Destroy(this.gameObject);
            }

            var item = GameObject.Find(model.key);
            if (item != null)
            {
                source = item.GetComponent<DispenserElementPresenter>();
                //source.AttachNetworkSync(this);
            }
        }
    }
}
