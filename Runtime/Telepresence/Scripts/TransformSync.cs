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
        base.Update();

        if (!hasTargetItem) return;
        
        if (NetworkTransform.isOwnedLocallySelf)
            copyTransform(TargetItem.transform, this.transform);
        else
            copyTransform(this.transform, TargetItem.transform);
    }

    private void copyTransform(Transform sourceItem, Transform destinationItem)
    {
        destinationItem.position = sourceItem.position;
        destinationItem.rotation = sourceItem.rotation;
        destinationItem.localScale = sourceItem.localScale;
    }
}
