using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserListItem : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text DisplayNameLabel;
    [SerializeField] Sprite IsMutedIcon;
    [SerializeField] Sprite IsUnmutedIcon;
    [SerializeField] ButtonInteractable MuteButton;

    private NetworkAvatarController networkUser;

    public void SetUser(NetworkAvatarController NetworkUser)
    {
        this.networkUser = NetworkUser;
        DisplayNameLabel.text = NetworkUser.Model.displayName + (networkUser.IsLocalUser ? " (You)" : "");
        MuteButton.SpriteRenderer.sprite = networkUser.IsMuted ? IsMutedIcon : IsUnmutedIcon; //TODO: probably need to observe the actual value on the network user instead of assumming its onyl changing here in this control
    }

    public void ToggleMute()
    {
        networkUser.IsMuted = !networkUser.IsMuted;
        MuteButton.SpriteRenderer.sprite = networkUser.IsMuted ? IsMutedIcon : IsUnmutedIcon;
    }
}
