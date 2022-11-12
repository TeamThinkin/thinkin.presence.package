using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransformSync : NetworkSyncBase<TransformSyncModel>, INetworkSync
{
    [SerializeField] private RealtimeTransform NetworkTransform;

    override public void RequestSyncOwnership()
    {
        base.RequestSyncOwnership();
        NetworkTransform.RequestOwnership();
    }

    override protected void Update()
    {
        if (!hasTargetItem) return;
        
        if (NetworkTransform.isOwnedRemotelySelf)
            copyTransform(this.transform, TargetItem.transform);
        else
            copyTransform(TargetItem.transform, this.transform);
    }

    private void copyTransform(Transform sourceItem, Transform destinationItem)
    {
        destinationItem.position = sourceItem.position;
        destinationItem.rotation = sourceItem.rotation;
        destinationItem.localScale = sourceItem.localScale;
    }
}
