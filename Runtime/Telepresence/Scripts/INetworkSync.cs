using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkSync
{
    void SetTarget(GameObject LocalTarget);
    void RequestSyncOwnership();

    Transform transform { get; }
    GameObject gameObject { get; }
}
