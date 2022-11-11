using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TelepresenceRoomManager : MonoBehaviour // RealtimeComponent<TelepresenceRoomManagerModel>
{
    public event Action OnUserListChanged;
    public event Action OnConnectionStatusChanged;

    public static TelepresenceRoomManager Instance { get; private set; }

    [SerializeField] private Realtime _normcore;

    public bool IsConnected => _normcore.connected;

    public int ClientId => _normcore.clientID;


    private List<NetworkAvatarController> networkUsers = new List<NetworkAvatarController>();
    public List<NetworkAvatarController> NetworkUsers => networkUsers;


    private void Awake()
    {
        Instance = this;
        _normcore.didConnectToRoom += _normcore_didConnectToRoom;
        _normcore.didDisconnectFromRoom += _normcore_didDisconnectFromRoom;
        DestinationPresenter.OnDestinationLoaded += DestinationPresenter_OnDestinationLoaded;
        DestinationPresenter.OnDestinationUnloaded += DestinationPresenter_OnDestinationUnloaded;
    }

    

    private void OnDestroy()
    {
        _normcore.didConnectToRoom -= _normcore_didConnectToRoom;
        _normcore.didDisconnectFromRoom -= _normcore_didDisconnectFromRoom;
        DestinationPresenter.OnDestinationLoaded -= DestinationPresenter_OnDestinationLoaded;
        DestinationPresenter.OnDestinationUnloaded -= DestinationPresenter_OnDestinationUnloaded;

        //if (model != null)
        //{
        //    model.connectedUsers.modelAdded -= ConnectedUsers_modelAdded;
        //    model.connectedUsers.modelRemoved -= ConnectedUsers_modelRemoved;
        //}
    }


    private void DestinationPresenter_OnDestinationLoaded()
    {
        if (!enabled) return;
        if (DestinationPresenter.CurrentDestinationId == null) return;

        _normcore.Connect(DestinationPresenter.CurrentDestinationId.ToString());
    }

    private void DestinationPresenter_OnDestinationUnloaded()
    {
        if (!enabled) return;
        _normcore.Disconnect();
    }    

    public void Connect()
    {
        _normcore.Connect(DestinationPresenter.CurrentDestinationId.ToString());
    }

    public void Disconnect()
    {
        _normcore.Disconnect();
    }

    public void RegisterNetworkUser(NetworkAvatarController user)
    {
        if (networkUsers.Contains(user)) return;

        networkUsers.Add(user);
        OnUserListChanged?.Invoke();
    }

    public void UnregisterNetworkUser(NetworkAvatarController user)
    {
        networkUsers.Remove(user);
        OnUserListChanged?.Invoke();
    }


    private void _normcore_didConnectToRoom(Realtime realtime)
    {
        //bool isFirstOneHere = !model.connectedUsers.Any(i => i.clientId != this.ClientId);
        bool isFirstOneHere = networkUsers.Count <= 1; //networkUsers.Any(i => i.Model.clientId != this.ClientId);
        
        if (isFirstOneHere)
        {
            Debug.Log("We are the first one here. Creating network syncs...");
            createNetworkSyncs();
        }
        else Debug.Log("Normcore connected, but we are not the first one here. Assuming syncs already created. Connected User Count: " + networkUsers.Count);

        OnConnectionStatusChanged?.Invoke();
    }

    private void _normcore_didDisconnectFromRoom(Realtime realtime)
    {
        OnConnectionStatusChanged?.Invoke();
    }

    private void createNetworkSyncs()
    {
        foreach(var entry in NetworkSyncFactory.SyncMappings)
        {
            foreach(var infoItem in entry.Value)
            {
                var sceneItems = GameObject.FindObjectsOfType(infoItem.TargetType);
                foreach(var sceneItem in sceneItems)
                {
                    var targetItem = (sceneItem as MonoBehaviour).gameObject;
                    Debug.Log("Creating sync for existing scene item: " + targetItem.name);
                    var sync = NetworkSyncFactory.FindOrCreateNetworkSync(targetItem, entry.Key.PrefabPath);
                    sync.RequestSyncOwnership();
                }
            }
        }

        //foreach(var entry in NetworkSyncFactory.SyncTypes)
        //{
        //    var items = GameObject.FindObjectsOfType(entry.Key);
        //    foreach(var item in items)
        //    {
        //        var targetItem = (item as MonoBehaviour).gameObject;
        //        var sync = NetworkSyncFactory.FindOrCreateNetworkSync(targetItem, entry.Value.PrefabPath);
        //        sync.RequestSyncOwnership();
        //    }
        //}
    }
    

    //protected override void OnRealtimeModelReplaced(TelepresenceRoomManagerModel previousModel, TelepresenceRoomManagerModel currentModel)
    //{
    //    base.OnRealtimeModelReplaced(previousModel, currentModel);

    //    if(previousModel != null)
    //    {
    //        previousModel.connectedUsers.modelAdded -= ConnectedUsers_modelAdded;
    //        previousModel.connectedUsers.modelRemoved -= ConnectedUsers_modelRemoved;
    //    }

    //    if(currentModel != null)
    //    {
    //        currentModel.connectedUsers.modelAdded += ConnectedUsers_modelAdded;
    //        currentModel.connectedUsers.modelRemoved += ConnectedUsers_modelRemoved;
    //    }
    //}

    //private void ConnectedUsers_modelRemoved(Normal.Realtime.Serialization.RealtimeSet<UserInfoModel> set, UserInfoModel model, bool remote)
    //{
    //    OnUserListChanged?.Invoke();
    //}

    //private void ConnectedUsers_modelAdded(Normal.Realtime.Serialization.RealtimeSet<UserInfoModel> set, UserInfoModel model, bool remote)
    //{
    //    OnUserListChanged?.Invoke();
    //}
}
