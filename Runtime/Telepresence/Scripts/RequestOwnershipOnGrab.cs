using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestOwnershipOnGrab : MonoBehaviour
{
    private IGrabbable grabbable;
    private INetworkSync sync;

    public void SetTarget(GameObject Target, INetworkSync Sync)
    {
        grabbable = Target.GetComponent<IGrabbable>();

        if (this.grabbable != null)
        {
            grabbable.OnBeforeGrab += OnBeforeGrab;
        }
    }

    private void OnDestroy()
    {
        grabbable.OnBeforeGrab -= OnBeforeGrab;
    }

    private void OnBeforeGrab(IGrabber grabber, IGrabbable grabbable)
    {
        if (!enabled) return;

        sync.RequestSyncOwnership();
    }
}
