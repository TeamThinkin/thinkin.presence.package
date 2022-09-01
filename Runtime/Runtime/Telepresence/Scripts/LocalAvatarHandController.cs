using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandSideEnum
{
    Left,
    Right
}

public class LocalAvatarHandController : MonoBehaviour, IProvideHandData
{
    [SerializeField] private HandSideEnum HandSide;

    public Transform FingerIndex1;
    public Transform FingerIndex2;
    public Transform FingerIndex3;

    public Transform FingerMiddle1;
    public Transform FingerMiddle2;
    public Transform FingerMiddle3;

    public Transform FingerRing1;
    public Transform FingerRing2;
    public Transform FingerRing3;

    public Transform FingerPinky1;
    public Transform FingerPinky2;
    public Transform FingerPinky3;

    public Transform FingerThumb1;
    public Transform FingerThumb2;
    public Transform FingerThumb3;


    private AvatarHandData handData;

    public AvatarHandData HandData => handData;

    public static LocalAvatarHandController Left { get; private set; }
    public static LocalAvatarHandController Right { get; private set; }

    private void Awake()
    {
        switch(HandSide)
        {
            case HandSideEnum.Left:
                Left = this;
                break;
            case HandSideEnum.Right:
                Right = this;
                break;
        }
    }

    private void Update()
    {
        updateHandData();
    }

    private void updateHandData()
    {
        handData.FingerIndex1 = FingerIndex1.localRotation;
        handData.FingerIndex2 = FingerIndex2.localRotation;
        handData.FingerIndex3 = FingerIndex3.localRotation;

        handData.FingerMiddle1 = FingerMiddle1.localRotation;
        handData.FingerMiddle2 = FingerMiddle2.localRotation;
        handData.FingerMiddle3 = FingerMiddle3.localRotation;

        handData.FingerRing1 = FingerRing1.localRotation;
        handData.FingerRing2 = FingerRing2.localRotation;
        handData.FingerRing3 = FingerRing3.localRotation;

        handData.FingerPinky1 = FingerPinky1.localRotation;
        handData.FingerPinky2 = FingerPinky2.localRotation;
        handData.FingerPinky3 = FingerPinky3.localRotation;

        handData.FingerThumb1 = FingerThumb1.localRotation;
        handData.FingerThumb2 = FingerThumb2.localRotation;
        handData.FingerThumb3 = FingerThumb3.localRotation;
    }

    public AvatarHandData GetHandData()
    {
        return handData;
    }
}
