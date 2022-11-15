using System.Collections;
using UnityEngine;

public interface ISyncModel
{
    string key { get; set; }
    string spawnItemPrefabPath { get; set; }
    string spawnUrl { get; set; }
    string spawnParentKey { get; set; }

    public delegate void PropertyChangedHandler<in TValue>(ISyncModel model, TValue value);
    event PropertyChangedHandler<string> keyDidChange;
    event PropertyChangedHandler<string> spawnItemPrefabPathDidChange;
    event PropertyChangedHandler<string> spawnUrlDidChange;
    event PropertyChangedHandler<string> spawnParentKeyDidChange;
}