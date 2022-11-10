using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PresencePanel : TabPanel
{
    [SerializeField] GameObject UserListItemPrefab;
    [SerializeField] ElementPresenterBase UserListContainer;
    [SerializeField] ButtonInteractable ConnectButton;
    [SerializeField] TMPro.TMP_Text ConnectionStatusLabel;

    public void ToggleConnection()
    {
        if (TelepresenceRoomManager.Instance.IsConnected)
            TelepresenceRoomManager.Instance.Disconnect();
        else
            TelepresenceRoomManager.Instance.Connect();
    }

    private void OnEnable()
    {
        TelepresenceRoomManager.Instance.OnConnectionStatusChanged += Instance_OnConnectionStatusChanged;
        TelepresenceRoomManager.Instance.OnUserListChanged += Instance_OnUserListChanged;
        updateUserList();
        updateConnectionStatus();
    }

    

    private void OnDisable()
    {
        TelepresenceRoomManager.Instance.OnConnectionStatusChanged -= Instance_OnConnectionStatusChanged;
        TelepresenceRoomManager.Instance.OnUserListChanged -= Instance_OnUserListChanged;
    }

    private void Instance_OnUserListChanged()
    {
        updateUserList();
    }

    private void Instance_OnConnectionStatusChanged()
    {
        updateConnectionStatus();
    }

    private void updateConnectionStatus()
    {
        Debug.Log("Presence Panel sees connection stauts updated: " + TelepresenceRoomManager.Instance.IsConnected);
        if(TelepresenceRoomManager.Instance.IsConnected)
        {
            ConnectionStatusLabel.text = "Connected";
            ConnectButton.Text = "Disconnect";
        }
        else
        {
            ConnectionStatusLabel.text = "Disconnected";
            ConnectButton.Text = "Connect";
        }
    }

    private void updateUserList()
    {
        UserListContainer.transform.ClearChildrenImmediate();

        var users = TelepresenceRoomManager.Instance.NetworkUsers.OrderBy(i => i.Model.displayName).OrderBy(i => !i.IsLocalUser);

        foreach (var networkUser in users)
        {
            var listItem = Instantiate(UserListItemPrefab).GetComponent<UserListItem>();
            listItem.transform.SetParent(UserListContainer.SceneChildrenContainer.transform, false);
            listItem.transform.Reset();
            listItem.SetUser(networkUser);
        }
        UserListContainer.ExecuteLayout();
    }
}
