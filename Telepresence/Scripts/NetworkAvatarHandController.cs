using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAvatarHandController : RealtimeComponent<HandInfoModel>, IProvideHandData
{
    [SerializeField] private LineRenderer RayVisualizer;
    [SerializeField] private NetworkAvatarController ParentRemoteAvatarController;
    [SerializeField] private HandSideEnum HandSide;

    private LocalAvatarHandController localHand;
    private AvatarHandData handData = new AvatarHandData();

    private void Update()
    {
        if(isOwnedLocallyInHierarchy)
        {
            localHand.HandData.CopyToHandInfoModel(model);
        }
        else
        {
            handData.CopyFromHandInfoModel(model);
            //updateRayVisualizer();
        }
    }

    //private void updateRayVisualizer()
    //{
    //    if (RayVisualizer == null) return;

    //    RayVisualizer.SetPosition(1, Vector3.forward * model.rayLength);
    //    RayVisualizer.gameObject.SetActive(handData.IsPointing);
    //}

    protected override void OnRealtimeModelReplaced(HandInfoModel previousModel, HandInfoModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);

        if(model.isOwnedLocallyInHierarchy)
        {
            switch(HandSide)
            {
                case HandSideEnum.Left:
                    localHand = LocalAvatarHandController.Left;
                    break;
                case HandSideEnum.Right:
                    localHand = LocalAvatarHandController.Right;
                    break;
            }
            if(RayVisualizer != null) RayVisualizer.gameObject.SetActive(false); //The local avatar handles the visualization
        }
    }

    public AvatarHandData GetHandData()
    {
        return handData;
    }
}
