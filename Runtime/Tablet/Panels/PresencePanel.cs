using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PresencePanel : TabPanel
{
    [SerializeField] GameObject UserListItemPrefab;
    [SerializeField] ElementPresenterBase UserListContainer;

    private void OnEnable()
    { 
        //UserInfo.CurrentUser.DisplayName
        TelepresenceRoomManager.Instance.OnUserListChanged += Instance_OnUserListChanged;
        updateUserList();
    }

    private void OnDisable()
    {
        TelepresenceRoomManager.Instance.OnUserListChanged -= Instance_OnUserListChanged;
    }

    private void Instance_OnUserListChanged()
    {
        updateUserList();
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
