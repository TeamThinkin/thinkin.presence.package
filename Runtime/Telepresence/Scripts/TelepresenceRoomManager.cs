using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TelepresenceRoomManager : RealtimeComponent<TelepresenceRoomManagerModel>
{
    public event Action OnUserListChanged;

    public static TelepresenceRoomManager Instance { get; private set; }

    [SerializeField] private Realtime _normcore;

    public bool IsConnected => _normcore.connected;

    public int ClientId => _normcore.clientID;


    public IEnumerable<UserInfoModel> ConnectedUsers
    {
        get
        {
            if (model != null)
            {
                foreach (var item in model.connectedUsers)
                {
                    yield return item;
                }
            }
        }
    }

    private void Awake()
    {
        Instance = this;
        _normcore.didConnectToRoom += _normcore_didConnectToRoom;
        DestinationPresenter.OnDestinationLoaded += DestinationPresenter_OnDestinationLoaded;
        DestinationPresenter.OnDestinationUnloaded += DestinationPresenter_OnDestinationUnloaded;
    }

    private void OnDestroy()
    {
        _normcore.didConnectToRoom -= _normcore_didConnectToRoom;
        DestinationPresenter.OnDestinationLoaded -= DestinationPresenter_OnDestinationLoaded;
        DestinationPresenter.OnDestinationUnloaded -= DestinationPresenter_OnDestinationUnloaded;

        if (model != null)
        {
            model.connectedUsers.modelAdded -= ConnectedUsers_modelAdded;
            model.connectedUsers.modelRemoved -= ConnectedUsers_modelRemoved;
        }
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

    public void AddUser(UserInfoModel UserInfo)
    {
        if (model == null) return;
        var newEntry = UserInfo.Clone();

        model.connectedUsers.Add(newEntry);
        OnUserListChanged?.Invoke();
    }

    public void RemoveUser(UserInfoModel UserInfo)
    {
        if (model == null) return;
        var entry = model.connectedUsers.SingleOrDefault(i => i.clientId == UserInfo.clientId);
        if (entry != null)
        {
            model.connectedUsers.Remove(entry);
            OnUserListChanged?.Invoke();
        }
    }

    private void _normcore_didConnectToRoom(Realtime realtime)
    {
        bool isFirstOneHere = !model.connectedUsers.Any(i => i.clientId != this.ClientId);
        
        if (isFirstOneHere)
        {
            Debug.Log("We are the first one here. Creating network syncs...");
            createNetworkSyncs();
        }
        else Debug.Log("Normcore connected, but we are not the first one here. Assuming syncs already created. Connected User Count: " + model.connectedUsers.Count);
    }

    private void createNetworkSyncs()
    {
        foreach(var entry in NetworkSyncFactory.SyncTypes)
        {
            var items = GameObject.FindObjectsOfType(entry.Key);
            foreach(var item in items)
            {
                var targetItem = (item as MonoBehaviour).gameObject;
                var sync = NetworkSyncFactory.FindOrCreateNetworkSync(targetItem, entry.Value.PrefabPath);
                sync.RequestSyncOwnership();
            }
        }
    }
    

    protected override void OnRealtimeModelReplaced(TelepresenceRoomManagerModel previousModel, TelepresenceRoomManagerModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);

        if(previousModel != null)
        {
            previousModel.connectedUsers.modelAdded -= ConnectedUsers_modelAdded;
            previousModel.connectedUsers.modelRemoved -= ConnectedUsers_modelRemoved;
        }

        if(currentModel != null)
        {
            currentModel.connectedUsers.modelAdded += ConnectedUsers_modelAdded;
            currentModel.connectedUsers.modelRemoved += ConnectedUsers_modelRemoved;
        }
    }

    private void ConnectedUsers_modelRemoved(Normal.Realtime.Serialization.RealtimeSet<UserInfoModel> set, UserInfoModel model, bool remote)
    {
        OnUserListChanged?.Invoke();
    }

    private void ConnectedUsers_modelAdded(Normal.Realtime.Serialization.RealtimeSet<UserInfoModel> set, UserInfoModel model, bool remote)
    {
        OnUserListChanged?.Invoke();
    }
}
