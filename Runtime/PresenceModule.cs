using System.Collections;
using UnityEngine;

public static class PresenceModule
{
    public static void Initialize()
    {
        NetworkSyncFactory.Initialize();
        AppControllerBase.Instance.UIManager.OnMakeGrabbable += UIManager_OnMakeGrabbable;
    }

    private static void UIManager_OnMakeGrabbable(GameObject Item)
    {
        var comp = Item.AddComponent<RequestOwnershipOnGrab>();
        comp.SetTarget(Item);
    }
}
