using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RequestOwnershipOnGrab : MonoBehaviour
{
    private IGrabbable grabbable;
    private INetworkSync sync;

    public void SetTarget(GameObject Target)
    {
        grabbable = Target.GetComponent<IGrabbable>();

        if (this.grabbable != null)
        {
            sync = TelepresenceRoomManager.Instance.Syncs.FirstOrDefault(i => i.TargetItem == Target);
            if(sync != null) grabbable.OnBeforeGrab += OnBeforeGrab;
        }
    }

    private void OnDestroy()
    {
        if(grabbable != null) grabbable.OnBeforeGrab -= OnBeforeGrab;
    }

    private void OnBeforeGrab(IGrabber grabber, IGrabbable grabbable)
    {
        if (!enabled) return;

        sync.RequestSyncOwnership();
    }
}
