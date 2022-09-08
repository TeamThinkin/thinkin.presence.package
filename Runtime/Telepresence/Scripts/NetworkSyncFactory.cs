using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class NetworkSyncFactory
{
    public struct NetworkSyncInfo
    {
        public Type TargetType;
        public string PrefabPath;
    }

    public static Dictionary<Type, NetworkSyncInfo> SyncTypes { get; private set; }

    public static Dictionary<string, INetworkSync> ExistingSyncs { get; private set; } = new Dictionary<string, INetworkSync>();

    static NetworkSyncFactory()
    {
        SyncTypes = new Dictionary<Type, NetworkSyncInfo>();
        DiscoverTypes(Assembly.GetExecutingAssembly());
    }

    public static void DiscoverTypes(Assembly SearchAssembly)
    {
        var types = from t in SearchAssembly.GetTypes()
                    where !t.IsAbstract && t.GetInterfaces().Contains(typeof(INetworkSync))
                    select t;

        foreach (var type in types)
        {
            var syncAttribute = type.GetCustomAttribute<NetworkSyncAttribute>();
            if (syncAttribute != null)
            {
                var syncInfo = new NetworkSyncInfo()
                {
                    TargetType = type,
                    PrefabPath = syncAttribute.PrefabPath,
                };

                if (SyncTypes.ContainsKey(syncAttribute.TargetType))
                    SyncTypes[syncAttribute.TargetType] = syncInfo;
                else
                    SyncTypes.Add(syncAttribute.TargetType, syncInfo);
                
            }
        }
    }

    public static INetworkSync FindOrCreateNetworkSync(GameObject TargetItem, string SyncPrefabPath)
    {
        if (!TelepresenceRoomManager.Instance.IsConnected) return null;

        INetworkSync sync;

        if (ExistingSyncs.ContainsKey(TargetItem.gameObject.name))
        {
            sync = ExistingSyncs[TargetItem.gameObject.name];
            sync.RequestSyncOwnership();
            return sync;
        }

        var options = Realtime.InstantiateOptions.defaults;
        options.ownedByClient = true;
        options.destroyWhenOwnerLeaves = false;

        sync = Realtime.Instantiate(SyncPrefabPath, options).GetComponent<INetworkSync>();
        sync.SetTarget(TargetItem);
        sync.RequestSyncOwnership();
        return sync;
    }
}
