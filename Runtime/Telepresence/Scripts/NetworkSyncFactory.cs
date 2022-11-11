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

    private static Dictionary<NetworkSyncInfo, List<NetworkSyncInfo>> syncMappings = new Dictionary<NetworkSyncInfo, List<NetworkSyncInfo>>()
    {
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
    public static Dictionary<NetworkSyncInfo, List<NetworkSyncInfo>> SyncMappings => syncMappings;

    //public static Dictionary<Type, NetworkSyncInfo> SyncTypes { get; private set; }

    public static Dictionary<string, INetworkSync> ExistingSyncs { get; private set; } = new Dictionary<string, INetworkSync>();

    public static void Initialize()
    {
        //SyncTypes = new Dictionary<Type, NetworkSyncInfo>();
        //DiscoverTypes(Assembly.GetExecutingAssembly());

        ItemSpawnObserver.OnItemSpawned += ItemSpawnObserver_OnItemSpawned;
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


    private static void ItemSpawnObserver_OnItemSpawned(GameObject TargetItem)
    {
        //Create spawnable network sync
        //need to ensure that when we spawn a item that the new item doesnt create a new spawn sync and create an endless loop

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

    public static INetworkSync FindOrCreateNetworkSync(GameObject TargetItem, string SyncPrefabPath)
    {
        if (!TelepresenceRoomManager.Instance.IsConnected) return null;

        INetworkSync sync;

        if (ExistingSyncs.ContainsKey(TargetItem.gameObject.name)) //NOTE: this Existing syncs is not currently kept up to date when syncs are created remotely
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
