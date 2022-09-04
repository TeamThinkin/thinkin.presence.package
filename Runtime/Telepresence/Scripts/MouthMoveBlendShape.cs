using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthMoveBlendShape : MonoBehaviour
{
    public SkinnedMeshRenderer Mesh;

    private RealtimeAvatarVoice _voice;

    private void Awake()
    {
        _voice = GetComponent<RealtimeAvatarVoice>();
    }

    private void Update()
    {
        if (Mesh == null || _voice == null) return;
        Mesh.SetBlendShapeWeight(0, Mathf.Clamp(_voice.voiceVolume * 200, 0, 100));
    }
}
