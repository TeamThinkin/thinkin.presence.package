using Autohand;
using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestOwnershipOnGrab : MonoBehaviour
{
    RealtimeTransform networkTransform;

    void Start()
    {
        networkTransform = GetComponent<RealtimeTransform>();
        GetComponent<Grabbable>().OnBeforeGrabEvent += OnBeforeGrab;
    }

    private void OnBeforeGrab(Hand hand, Grabbable grabbable)
    {
        if (!enabled) return;

        Debug.Log("Requesting network ownership");
        networkTransform.RequestOwnership();
    }
}
