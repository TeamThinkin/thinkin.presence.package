using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabSyncMonitor : MonoBehaviour
{
    private Grabbable grabbable;
    private Rigidbody body;
    private NetworkItemSync sync;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
        if(grabbable != null)
        {
            grabbable.onGrab.AddListener(onGrab);
            grabbable.onRelease.AddListener(onRelease);

            grabbable.OnBeginRest += Grabbable_OnBeginRest;

            body = GetComponent<Rigidbody>();
        }
    }

    private void OnDestroy()
    {
        if(grabbable != null)
        {
            grabbable.onGrab.RemoveListener(onGrab);
            grabbable.onRelease.RemoveListener(onRelease);

            grabbable.OnBeginRest -= Grabbable_OnBeginRest;
        }
    }

    private void onGrab(Hand Hand, Grabbable Grabbable)
    {
        sync = NetworkItemSync.FindOrCreate(this.gameObject);
    }

    private void onRelease(Hand Hand, Grabbable Grabbable)
    {
    }

    private void Grabbable_OnBeginRest(Grabbable obj)
    {
        Debug.Log("Grab synce monitor sees the grabbable has come to rest");
        //if (sync != null) sync.Destroy();
        //sync?.Destroy();
        //sync = null;
    }
}
