using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NetworkAvatarController : RealtimeComponent<UserInfoModel>
{
    [SerializeField] private Transform HeadTransform;
    [SerializeField] private Transform RightHandTransform;
    [SerializeField] private Transform LeftHandTransform;
    [SerializeField] private TMP_Text DisplayNameLabel;
    [SerializeField] private MouthMoveBlendShape MouthMover;
    [SerializeField] private NetworkAvatarHandController LeftHand;
    [SerializeField] private NetworkAvatarHandController RightHand;

    private SkinController currentSkin;

    private void Awake()
    {
        LocalAvatarManager.Instance.OnCurrentSkinLoaded += LocalAvatarManager_OnCurrentSkinLoaded;
    }

    private void LocalAvatarManager_OnCurrentSkinLoaded()
    {
        if(isOwnedLocallyInHierarchy)
        {
            linkMouthMover();
        }
    }

    private void OnDestroy()
    {
        LocalAvatarManager.Instance.OnCurrentSkinLoaded -= LocalAvatarManager_OnCurrentSkinLoaded;

        if(currentSkin != null)
        {
            Destroy(currentSkin.gameObject);
        }

        if (model != null)
        {
            TelepresenceRoomManager.Instance.RemoveUser(model);
            model.displayNameDidChange -= CurrentModel_displayNameDidChange;

            if (model.isOwnedLocallyInHierarchy)
            {
                UserInfo.OnCurrentUserChanged -= UserInfo_OnCurrentUserChanged;
            }
        }
    }

    protected override void OnRealtimeModelReplaced(UserInfoModel previousModel, UserInfoModel currentModel)
    {
        base.OnRealtimeModelReplaced(previousModel, currentModel);
        if (previousModel != null)
        {
            previousModel.avatarUrlDidChange -= CurrentModel_avatarUrlDidChange;
        }

        if (currentModel != null)
        {
            if (currentModel.isOwnedLocallyInHierarchy)
            {
                //Local avatar
                gameObject.name = "Network Avatar (Local)";
                UserInfo.OnCurrentUserChanged += UserInfo_OnCurrentUserChanged;
                updateModelFromUserInfo();
                model.clientId = TelepresenceRoomManager.Instance.ClientId;
                linkMouthMover();

                TelepresenceRoomManager.Instance.AddUser(model);
            }
            else
            {
                //Remote avatar
                gameObject.name = "Network Avatar (Remote)";
                currentModel.avatarUrlDidChange += CurrentModel_avatarUrlDidChange;
                updateSkin(currentModel.avatarUrl);
            }          

            currentModel.displayNameDidChange += CurrentModel_displayNameDidChange;
            updateDisplayName();
        }
    }
    
    private void updateModelFromUserInfo()
    {
        model.avatarUrl = UserInfo.CurrentUser.AvatarUrl;
        model.displayName = UserInfo.CurrentUser.DisplayName;
    }

    private void UserInfo_OnCurrentUserChanged(UserInfo obj)
    {
        if (!isOwnedLocallyInHierarchy) return;
        updateModelFromUserInfo();
    }

    private void CurrentModel_avatarUrlDidChange(UserInfoModel model, string value)
    {
        if (isOwnedLocallyInHierarchy) return; //Local skins are handled by the LocalAvatarManager
        updateSkin(model.avatarUrl);
    }

    private void CurrentModel_displayNameDidChange(UserInfoModel model, string value)
    {
        updateDisplayName();
    }

    private void updateDisplayName()
    {
        DisplayNameLabel.text = model.displayName;
    }

    private void linkMouthMover() //Note: this assumes the skin model has been loaded, and as that is an async process there may be problems if this called before it completes
    {
        if (isOwnedLocallyInHierarchy)
        {
            if (MouthMover == null) Debug.LogError("MouthMover Null");
            if (LocalAvatarManager.Instance == null) Debug.LogError("Local AvatarManager Null");

            if (LocalAvatarManager.Instance.CurrentSkin != null)
            {
                MouthMover.Mesh = LocalAvatarManager.Instance.CurrentSkin.GetMouthRenderer();
            }
            else
            {
                //Looks like the avatar has not loaded yet. Once it has loaded the LocalAvatarManager_OnCurrentSkinLoaded should call this function again for another attempt
                Debug.Log("Network Avatar Controller unable to link mouth mover because skin is not available. Waiting for signal from LocalAvatarManager for OnCurrentSkinLoaded");
            }
        }
        else
        {
            MouthMover.Mesh = currentSkin.GetMouthRenderer();
        }
    }

    private async void updateSkin(string avatarUrl)
    {
        if (isOwnedLocallyInHierarchy) return; //Local skins are handled by the LocalAvatarManager

        if(currentSkin != null)
        {
            Destroy(currentSkin.gameObject);
            currentSkin = null;
        }

        currentSkin = await SkinController.CreateSkin(false, avatarUrl, HeadTransform, LeftHandTransform, RightHandTransform, LeftHand, RightHand);
        linkMouthMover();
    }
}
