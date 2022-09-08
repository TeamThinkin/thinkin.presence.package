using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSyncAttribute : Attribute
{
    public Type TargetType { get; private set; }
    public string PrefabPath { get; private set; }

    public NetworkSyncAttribute(Type TargetType, string PrefabPath)
    {
        this.TargetType = TargetType;
        this.PrefabPath = PrefabPath;
    }
}
