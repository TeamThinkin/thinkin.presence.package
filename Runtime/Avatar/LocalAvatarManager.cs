using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalAvatarManager : MonoBehaviour, IProvideHandData
{
    private const string defaultAvatarUrl = "https://models.readyplayer.me/6400d536a327e1f2e59a2b5e.glb";
    public static LocalAvatarManager Instance { get; private set; }

    [SerializeField] private Transform RightHandAnchor;
    [SerializeField] private Transform LeftHandAnchor;
    [SerializeField] private Transform HeadAnchor;
    [SerializeField] private GameObject DefaultAvatar;
    [SerializeField] private LocalAvatarHandController LeftAvatarHand;
    [SerializeField] private LocalAvatarHandController RightAvatarHand;

    private SkinController currentSkin;
    private bool isCurrentSkinDefault;
    private string currentAvatarUrl;

    public event Action OnCurrentSkinLoaded;

    public SkinController CurrentSkin => currentSkin;

    private void Awake()
    {
        Instance = this;
        UserInfo.OnCurrentUserChanged += UserInfo_OnCurrentUserChanged;
    }

    private void OnDestroy()
    {
        UserInfo.OnCurrentUserChanged -= UserInfo_OnCurrentUserChanged;
    }

    private void UserInfo_OnCurrentUserChanged(UserInfo newUser)
    {
        updateSkin();
    }

    private void destroyCurrentSkin()
    {
        if (currentSkin == null) return;
        Destroy(currentSkin.gameObject);
        currentSkin = null;
        currentAvatarUrl = null;
    }

    private async void updateSkin()
    {
        if (UserInfo.CurrentUser.AvatarUrl.IsNullOrEmpty()) UserInfo.CurrentUser.AvatarUrl = defaultAvatarUrl; //TODO: this should be replaced with a locally available model

        if (!UserInfo.CurrentUser.AvatarUrl.IsNullOrEmpty())
        {
            if (currentAvatarUrl == UserInfo.CurrentUser.AvatarUrl) return;
            destroyCurrentSkin();

            isCurrentSkinDefault = false;
            currentAvatarUrl = UserInfo.CurrentUser.AvatarUrl;
            currentSkin = await SkinController.CreateSkin(true, UserInfo.CurrentUser.AvatarUrl, HeadAnchor, LeftHandAnchor, RightHandAnchor, LeftAvatarHand, RightAvatarHand);
            DontDestroyOnLoad(currentSkin);
            OnCurrentSkinLoaded?.Invoke();
        }
        else
        {
            if (isCurrentSkinDefault) return;
            destroyCurrentSkin();
            isCurrentSkinDefault = true;
            currentAvatarUrl = null;
            currentSkin = Instantiate(DefaultAvatar).GetComponent<SkinController>();
            Debug.Log("Current skin: " + currentSkin, currentSkin.gameObject);
            DontDestroyOnLoad(currentSkin);
            currentSkin.SetSourceData(true, HeadAnchor, LeftHandAnchor, RightHandAnchor, LeftAvatarHand, RightAvatarHand);
            OnCurrentSkinLoaded?.Invoke();
        }
    }

    public AvatarHandData GetHandData()
    {
        return LeftAvatarHand.HandData;
    }

    public AvatarHandData GetRightHandData()
    {
        return RightAvatarHand.HandData;
    }
}
