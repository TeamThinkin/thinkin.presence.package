using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSyncMonitor : MonoBehaviour
{
    private IGrabbable grabbable;

    private void Awake()
    {
        grabbable = GetComponent<IGrabbable>();
        if(grabbable != null)
        {
            grabbable.OnGrab += Grabbable_OnGrab;
        }
    }

    private void OnDestroy()
    {
        if(grabbable != null)
        {
            grabbable.OnGrab -= Grabbable_OnGrab;
        }
    }

    private void Grabbable_OnGrab(IGrabber Grabber, IGrabbable Grabbable)
    {
        NetworkItemSync.FindOrCreate(this.gameObject);
    }
}
