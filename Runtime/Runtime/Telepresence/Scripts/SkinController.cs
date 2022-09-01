using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using ReadyPlayerMe;
using System;
using System.Threading;

public interface IProvideHandData
{
    AvatarHandData GetHandData();
}

public class SkinController : MonoBehaviour
{
    private struct Fingers
    {
        public Transform Index1;
        public Transform Index2;
        public Transform Index3;
        public Transform Middle1;
        public Transform Middle2;
        public Transform Middle3;
        public Transform Ring1;
        public Transform Ring2;
        public Transform Ring3;
        public Transform Pinky1;
        public Transform Pinky2;
        public Transform Pinky3;
        public Transform Thumb1;
        public Transform Thumb2;
        public Transform Thumb3;
    }

    [SerializeField] private bool _isDefaultSkin;
    public bool IsDefaultSkin => _isDefaultSkin;

    private Transform headTransform;
    private Transform leftHandTransform;
    private Transform rightHandTransform;
    private IProvideHandData leftHandDataProvider;
    private IProvideHandData rightHandDataProvider;
    private Transform neckBone;
    private Transform headBone;
    private Vector3 neckHeadOffset;
    private AvatarHandData leftHandData;
    private AvatarHandData rightHandData;
    private Fingers leftFingers;
    private Fingers rightFingers;
    private bool isLocal;
    private int hiddenLayer;

    private void Awake()
    {
        hiddenLayer = LayerMask.NameToLayer("Hidden");
    }

    public static async Task<SkinController> CreateSkin(bool IsLocal, string AvatarUrl, Transform HeadTransform, Transform LeftHandTransform, Transform RightHandTransform, IProvideHandData LeftHandDataProvider, IProvideHandData RightHandDataProvider)
    {
        var avatar = await loadAvatarFromUrl(AvatarUrl);
        var skinController = avatar.AddComponent<SkinController>();
        skinController.SetSourceData(IsLocal, HeadTransform, LeftHandTransform, RightHandTransform, LeftHandDataProvider, RightHandDataProvider);
        return skinController;
    }

    private void Update()
    {
        if (leftHandDataProvider != null)
        {
            leftHandData = leftHandDataProvider.GetHandData();
            rightHandData = rightHandDataProvider.GetHandData();

            updateFingersFromData(leftFingers, leftHandData);
            updateFingersFromData(rightFingers, rightHandData);
        }

        updateBodyPosition();
    }

    private void LateUpdate()
    {
        updateBodyPosition();
    }

    private void updateFingersFromData(Fingers fingers, AvatarHandData handData)
    {
        fingers.Index1.localRotation = handData.FingerIndex1;
        fingers.Index2.localRotation = handData.FingerIndex2;
        fingers.Index3.localRotation = handData.FingerIndex3;

        fingers.Middle1.localRotation = handData.FingerMiddle1;
        fingers.Middle2.localRotation = handData.FingerMiddle2;
        fingers.Middle3.localRotation = handData.FingerMiddle3;

        fingers.Ring1.localRotation = handData.FingerRing1;
        fingers.Ring2.localRotation = handData.FingerRing2;
        fingers.Ring3.localRotation = handData.FingerRing3;

        fingers.Pinky1.localRotation = handData.FingerPinky1;
        fingers.Pinky2.localRotation = handData.FingerPinky2;
        fingers.Pinky3.localRotation = handData.FingerPinky3;

        fingers.Thumb1.localRotation = handData.FingerThumb1;
        fingers.Thumb2.localRotation = handData.FingerThumb2;
        fingers.Thumb3.localRotation = handData.FingerThumb3;
    }

    private void updateBodyPosition()
    {
        if (neckBone != null && headBone != null)
        {
            //neckBone.position = headBone.position + neckHeadOffset;
            //neckBone.rotation = Quaternion.Slerp(neckBone.rotation, headBone.rotation, 0.5f * Time.deltaTime); //TODO: Limit non vertical axis rotation so cocking your head to the side doesnt rotate torso

            neckBone.position = headTransform.position + neckHeadOffset;
            neckBone.rotation = Quaternion.Slerp(neckBone.rotation, headTransform.rotation, 0.5f * Time.deltaTime); //TODO: Limit non vertical axis rotation so cocking your head to the side doesnt rotate torso
        }
    }

    public void SetSourceData(bool IsLocal, Transform HeadTransform, Transform LeftHandTransform, Transform RightHandTransform, IProvideHandData LeftHandDataProvider, IProvideHandData RightHandDataProvider)
    {
        this.isLocal = IsLocal;
        this.headTransform = HeadTransform;
        this.leftHandTransform = LeftHandTransform;
        this.rightHandTransform = RightHandTransform;
        this.leftHandDataProvider = LeftHandDataProvider;
        this.rightHandDataProvider = RightHandDataProvider;

        if (IsLocal) patchRendererBounds(gameObject);

        rightFingers = findSkinFingers(gameObject, "Right");
        leftFingers = findSkinFingers(gameObject, "Left");
        addBoneConstraints(gameObject);
        removeExtraComponent(gameObject);
    }
    

    private void removeExtraComponent(GameObject skin)
    {
        var animator = skin.GetComponent<Animator>();
        if (animator != null)
        {
            Destroy(animator);
        }

        if(isLocal)
        {
            hideObject(skin, "Renderer_Teeth");
            hideObject(skin, "Renderer_EyeLeft");
            hideObject(skin, "Renderer_EyeRight");
            hideObject(skin, "Renderer_Head");
            hideObject(skin, "Renderer_Beard");

            //hideObject(skin, "Avatar_Renderer_Teeth");
            //hideObject(skin, "Avatar_Renderer_EyeLeft");
            //hideObject(skin, "Avatar_Renderer_EyeRight");
            //hideObject(skin, "Avatar_Renderer_Head");
            //hideObject(skin, "Avatar_Renderer_Beard");
        }
    }

    private void hideObject(GameObject skin, string itemName)
    {
        var item = skin.transform.Find(itemName);
        if (item != null) item.gameObject.layer = hiddenLayer;
    }

    public SkinnedMeshRenderer GetMouthRenderer()
    {
        return gameObject.transform.Find("Renderer_Head").gameObject.GetComponent<SkinnedMeshRenderer>();
        //return gameObject.transform.Find("Avatar_Renderer_Head").gameObject.GetComponent<SkinnedMeshRenderer>();
    }

    private static async Task<GameObject> loadAvatarFromUrl(string avatarUrl)
    {
        var loader = new AvatarLoader();
        bool isLoaded = false;
        GameObject loadedSkin = null;

        EventHandler<CompletionEventArgs> handler = (sender, e) =>
        {
            isLoaded = true;
            loadedSkin = e.Avatar;
        };

        loader.OnCompleted += handler;
        loader.OnFailed += Loader_OnFailed;
        loader.LoadAvatar(avatarUrl);

        await Task.Run(() =>
        {
            while (!isLoaded) 
            {
                Thread.Sleep(10); 
            }
        });
        loader.OnCompleted -= handler;

        loadedSkin.name = "Skin";
        DontDestroyOnLoad(loadedSkin);

        return loadedSkin;
    }
    private static void Loader_OnFailed(object sender, FailureEventArgs e)
    {
        Debug.Log("!!! Loader failed: " + e.Message);
    }

    private void patchRendererBounds(GameObject skin)
    {
        var renderers = skin.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.localBounds = new Bounds(Vector3.zero, Vector3.one * 50f); //TODO: This just makes the local bounds huge so that it wont be culled, but needs a better fix
        }
    }

    private Fingers findSkinFingers(GameObject skin, string HandPrefix)
    {
        var fingers = new Fingers();
        var hand = skin.transform.Find($"Armature/Hips/Spine/{HandPrefix}Hand");

        fingers.Index1 = hand.Find(HandPrefix + "HandIndex1");
        fingers.Index2 = fingers.Index1.Find(HandPrefix + "HandIndex2");
        fingers.Index3 = fingers.Index2.Find(HandPrefix + "HandIndex3");

        fingers.Middle1 = hand.Find(HandPrefix + "HandMiddle1");
        fingers.Middle2 = fingers.Middle1.Find(HandPrefix + "HandMiddle2");
        fingers.Middle3 = fingers.Middle2.Find(HandPrefix + "HandMiddle3");

        fingers.Ring1 = hand.Find(HandPrefix + "HandRing1");
        fingers.Ring2 = fingers.Ring1.Find(HandPrefix + "HandRing2");
        fingers.Ring3 = fingers.Ring2.Find(HandPrefix + "HandRing3");

        fingers.Pinky1 = hand.Find(HandPrefix + "HandPinky1");
        fingers.Pinky2 = fingers.Pinky1.Find(HandPrefix + "HandPinky2");
        fingers.Pinky3 = fingers.Pinky2.Find(HandPrefix + "HandPinky3");

        fingers.Thumb1 = hand.Find(HandPrefix + "HandThumb1");
        fingers.Thumb2 = fingers.Thumb1.Find(HandPrefix + "HandThumb2");
        fingers.Thumb3 = fingers.Thumb2.Find(HandPrefix + "HandThumb3");

        return fingers;
    }

    private void addBoneConstraints(GameObject skin)
    {
        neckBone = skin.transform.Find("Armature/Hips/Spine/Neck");
        headBone = skin.transform.Find("Armature/Hips/Spine/Neck/Head");
        neckHeadOffset = neckBone.position - headBone.position;

        addParentConstraint(skin, "Armature/Hips/Spine/RightHand", rightHandTransform, new Vector3(180, 90, 90));
        addParentConstraint(skin, "Armature/Hips/Spine/LeftHand", leftHandTransform, new Vector3(0, 90, 90));
        addParentConstraint(skin, "Armature/Hips/Spine/Neck/Head", headTransform, Vector3.zero);
    }

    private void addParentConstraint(GameObject skin, string bonePath, Transform anchor, Vector3 rotationOffset)
    {
        var bone = skin.transform.Find(bonePath);
        var constraint = bone.gameObject.AddComponent<ParentConstraint>();
        var constraintList = new List<ConstraintSource>();
        constraintList.Add(new ConstraintSource() { sourceTransform = anchor, weight = 1 });
        constraint.SetSources(constraintList);
        constraint.rotationOffsets = new[] { rotationOffset };
        constraint.constraintActive = true;
    }
}