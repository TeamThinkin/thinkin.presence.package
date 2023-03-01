using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TelepresenceRoomManager : MonoBehaviour
{
    public event Action OnUserListChanged;
    public event Action OnConnectionStatusChanged;
    public event Action<INetworkSync> OnSyncAdded;

    public static TelepresenceRoomManager Instance { get; private set; }

    [SerializeField] private Realtime _normcore;

    public bool IsConnected => _normcore.connected;

    public int ClientId => _normcore.clientID;


    private List<NetworkAvatarController> networkUsers = new List<NetworkAvatarController>();
    public List<NetworkAvatarController> NetworkUsers => networkUsers;

    public List<INetworkSync> Syncs { get; private set; } = new List<INetworkSync>();

    private bool isAutoReconnectEnabled = false;


    public void Connect()
    {
        if (!DestinationPresenter.CurrentDestinationId.HasValue) return; //No reason to connect if we are in a local scene with no destination

        isAutoReconnectEnabled = true;
        _normcore.Connect(DestinationPresenter.CurrentDestinationId.ToString());
    }

    public void Disconnect(bool stayDisconnected = false)
    {
        isAutoReconnectEnabled = !stayDisconnected;
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

    public void RegisterSync(INetworkSync NewSync)
    {
        if (!Syncs.Contains(NewSync))
        {
            Syncs.Add(NewSync);
            OnSyncAdded?.Invoke(NewSync);
        }
    }

    public void UnregisterSync(INetworkSync OldSync)
    {
        if (Syncs.Contains(OldSync)) Syncs.Remove(OldSync);
    }


    private void Awake()
    {
        Instance = this;

        PlayerIdleMonitor.OnIdleEnd += PlayerIdleMonitor_OnIdleEnd;
        PlayerIdleMonitor.OnAbandonStart += PlayerIdleMonitor_OnAbandonStart;
        _normcore.didConnectToRoom += _normcore_didConnectToRoom;
        _normcore.didDisconnectFromRoom += _normcore_didDisconnectFromRoom;
        DestinationPresenter.OnDestinationLoaded += DestinationPresenter_OnDestinationLoaded;
        DestinationPresenter.OnDestinationUnloaded += DestinationPresenter_OnDestinationUnloaded;

        InvokeRepeating("checkConnection", 1, 10);
    }

    private void OnDestroy()
    {
        PlayerIdleMonitor.OnIdleEnd -= PlayerIdleMonitor_OnIdleEnd;
        PlayerIdleMonitor.OnAbandonStart -= PlayerIdleMonitor_OnAbandonStart;
        _normcore.didConnectToRoom -= _normcore_didConnectToRoom;
        _normcore.didDisconnectFromRoom -= _normcore_didDisconnectFromRoom;
        DestinationPresenter.OnDestinationLoaded -= DestinationPresenter_OnDestinationLoaded;
        DestinationPresenter.OnDestinationUnloaded -= DestinationPresenter_OnDestinationUnloaded;
    }

    private void PlayerIdleMonitor_OnAbandonStart()
    {
        Debug.Log("Player abandoned app. Disconnecting...");
        Disconnect();
    }

    private void checkConnection()
    {
        if (!isAutoReconnectEnabled) return;

        if (!PlayerIdleMonitor.IsAbandoned && !_normcore.connected && !_normcore.connecting && DestinationPresenter.CurrentDestinationId.HasValue && !DestinationPresenter.Instance.IsLoading)
        {
            Debug.Log("Looks like we are disconnected. Reconnecting...");
            Connect();
        }
    }

    private void PlayerIdleMonitor_OnIdleEnd()
    {
        if (!_normcore.connected && !_normcore.connecting && isAutoReconnectEnabled)
        {
            Debug.Log("Player no longer idle. Reconnecting...");
            Connect();
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

    private void _normcore_didConnectToRoom(Realtime realtime)
    {
        bool isFirstOneHere = !networkUsers.Any(i => !i.IsLocalUser);
        
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
                    var sync = NetworkSyncFactory.FindOrCreateNetworkSync(targetItem, entry.Key.PrefabPath);
                    sync.RequestSyncOwnership();
                }
            }
        }
    }
}
