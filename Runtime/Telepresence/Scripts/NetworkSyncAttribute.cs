using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: this is probably not needed anymore
public class NetworkSyncAttribute : Attribute
{
    public Type TargetType { get; private set; }
    public string SyncPrefabPath { get; private set; }

    public NetworkSyncAttribute(Type TargetType, string PrefabPath)
    {
        this.TargetType = TargetType;
        this.SyncPrefabPath = PrefabPath;
    }
}