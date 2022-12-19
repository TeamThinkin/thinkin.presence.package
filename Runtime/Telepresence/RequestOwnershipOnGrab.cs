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
            grabbable.OnBeforeGrab += OnBeforeGrab;

            sync = TelepresenceRoomManager.Instance.Syncs.FirstOrDefault(i => i.TargetItem == Target);
            if (sync == null)
            { 
                //No Sync for our object right now. Listen to see if one gets created later
                TelepresenceRoomManager.Instance.OnSyncAdded += Instance_OnSyncAdded;
            }
        }
    }

    private void Instance_OnSyncAdded(INetworkSync NewSync)
    {
        if (NewSync.TargetItem == this.gameObject)
        {
            sync = NewSync;
            TelepresenceRoomManager.Instance.OnSyncAdded -= Instance_OnSyncAdded;
        }
    }

    private void OnDestroy()
    {
        if (grabbable != null) grabbable.OnBeforeGrab -= OnBeforeGrab;
    }

    private void OnBeforeGrab(IGrabber grabber, IGrabbable grabbable)
    {
        if (!enabled) return;

        if (sync != null)
        {
            sync.RequestSyncOwnership();
        }
    }
}
