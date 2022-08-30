//using Normal.Realtime;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

////TODO: this functionality should probably be folder into RoomManager
//public class SyncTelepresenceRoom : MonoBehaviour
//{
//    [SerializeField] private Normal.Realtime.Realtime Normcore;

//    private void Start()
//    {
//        DestinationPresenter.OnDestinationLoaded += DestinationPresenter_OnDestinationLoaded;
//        DestinationPresenter.OnDestinationUnloaded += DestinationPresenter_OnDestinationUnloaded;
//        Normcore.didConnectToRoom += Normcore_didConnectToRoom;
//    }

//    private void Normcore_didConnectToRoom(Normal.Realtime.Realtime realtime)
//    {
        
//    }

//    private void OnDestroy()
//    {
//        DestinationPresenter.OnDestinationLoaded -= DestinationPresenter_OnDestinationLoaded;
//        DestinationPresenter.OnDestinationUnloaded -= DestinationPresenter_OnDestinationUnloaded;
//        Normcore.didConnectToRoom -= Normcore_didConnectToRoom;
//    }

//    private void DestinationPresenter_OnDestinationLoaded()
//    {
//        if (!enabled) return;
//        if (DestinationPresenter.CurrentDestinationId == null) return;

//        Normcore.Connect(DestinationPresenter.CurrentDestinationId.ToString());
//    }

//    private void DestinationPresenter_OnDestinationUnloaded()
//    {
//        if (!enabled) return;
//        Normcore.Disconnect();
//    }
//}
