using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceViewer : MonoBehaviour
{
    [SerializeField] private bool InvertDirection;
    [SerializeField] private NetworkAvatarController avatarController;

    private Transform viewer;

    void Start()
    {
        viewer = AppControllerBase.Instance.MainCamera.transform;    
    }

    void Update()
    {
        if (avatarController == null || avatarController.IsLocalUser) return;

        var direction = viewer.position - transform.position;
        if (InvertDirection) direction *= -1;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
