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

        public NetworkSyncInfo(Type TargetType, string PrefabPath)
        {
            this.TargetType = TargetType;
            this.PrefabPath = PrefabPath;
        }
    }

    public static Dictionary<NetworkSyncInfo, List<NetworkSyncInfo>> SyncMappings => syncMappings;
    private static Dictionary<NetworkSyncInfo, List<NetworkSyncInfo>> syncMappings = new Dictionary<NetworkSyncInfo, List<NetworkSyncInfo>>()
    {
        {
            new NetworkSyncInfo(typeof(GrabbableSync), "Prefabs/Grabbable Sync"),
            new List<NetworkSyncInfo>()
        },

        {
            new NetworkSyncInfo(typeof(TransformSync), "Prefabs/Transform Sync"),
            new List<NetworkSyncInfo>(new []
            {
                new NetworkSyncInfo(typeof(Tablet), "Prefabs/Tablet (Remote)"), //NOTE: this prefab path wasnt used when implementing the Tablet sync. Might not be needed
            })
        },

        {
            new NetworkSyncInfo(typeof(DispenserSync), "Presenters/Dispenser/Dispenser Sync"),
            new List<NetworkSyncInfo>(new []
            {
                new NetworkSyncInfo(typeof(DispenserElementPresenter), "Presenters/Dispenser/Dispenser"), //NOTE: this prefab path wasnt used when implementing the Tablet sync. Might not be needed
            })
        }
    };

    public static void Initialize()
    {
        ItemSpawnObserver.OnItemSpawned += ItemSpawnObserver_OnItemSpawned;
    }

    private static void ItemSpawnObserver_OnItemSpawned(GameObject TargetItem)
    {
        var spawnableItem = TargetItem.GetComponent<ISpawnableItem>();
        if(spawnableItem != null)
        {
            FindOrCreateNetworkSync(spawnableItem);
        }
    }

    public static INetworkSync FindOrCreateNetworkSync(ISpawnableItem TargetItem)
    {
        var itemType = TargetItem.GetType();
        var mapItem = syncMappings.SingleOrDefault(i => i.Value.Any(j => j.TargetType == itemType));
        return FindOrCreateNetworkSync(TargetItem.gameObject, mapItem.Key.PrefabPath);
    }

    public static INetworkSync FindOrCreateNetworkSync(GameObject TargetItem, Type SyncType)
    {
        var entry = syncMappings.SingleOrDefault(i => i.Key.TargetType == SyncType);
        return FindOrCreateNetworkSync(TargetItem, entry.Key.PrefabPath);
    }

    public static T FindOrCreateNetworkSync<T>(GameObject TargetItem) where T : class
    {
        var entry = syncMappings.SingleOrDefault(i => i.Key.TargetType == typeof(T));
        return FindOrCreateNetworkSync(TargetItem, entry.Key.PrefabPath) as T;
    }

    public static INetworkSync FindOrCreateNetworkSync(GameObject TargetItem, string SyncPrefabPath)
    {
        if (!TelepresenceRoomManager.Instance.IsConnected) return null;

        INetworkSync sync;
        sync = TelepresenceRoomManager.Instance.Syncs.FirstOrDefault(i => i.TargetItem == TargetItem);
        if (sync != null) 
        {
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


    //public static void DiscoverTypes(Assembly SearchAssembly)
    //{
    //    var types = from t in SearchAssembly.GetTypes()
    //                where !t.IsAbstract && t.GetInterfaces().Contains(typeof(INetworkSync))
    //                select t;

    //    foreach (var type in types)
    //    {
    //        var syncAttribute = type.GetCustomAttribute<NetworkSyncAttribute>();
    //        if (syncAttribute != null)
    //        {
    //            var syncInfo = new NetworkSyncInfo()
    //            {
    //                TargetType = type,
    //                PrefabPath = syncAttribute.SyncPrefabPath,
    //            };

    //            if (SyncTypes.ContainsKey(syncAttribute.TargetType))
    //                SyncTypes[syncAttribute.TargetType] = syncInfo;
    //            else
    //                SyncTypes.Add(syncAttribute.TargetType, syncInfo);

    //        }
    //    }
    //}


}
