using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkSync
{
    GameObject TargetItem { get; }
    Transform transform { get; }
    GameObject gameObject { get; }

    void SetTarget(GameObject LocalTarget);
    void RequestSyncOwnership();

}