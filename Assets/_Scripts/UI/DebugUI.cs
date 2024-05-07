using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour {
    public Button plexButton;
    public Button jellyfinButton;

    public TMP_Dropdown accountDropdown;
    public TMP_Dropdown serverDropdown;
    public TMP_Dropdown libraryDropdown;

    public Button doneButton;
    public Button deleteAllButton;

    private void Start() {
        UpdateAccountDropdown();
        
        serverDropdown.ClearOptions();
        serverDropdown.AddOptions(new List<string> { "Select an account first..." });
        
        libraryDropdown.ClearOptions();
        libraryDropdown.AddOptions(new List<string> { "Select a server first..." });
        
        plexButton.onClick.AddListener(OnPlexButtonClicked);
        jellyfinButton.onClick.AddListener(OnJellyfinButtonClicked);
        doneButton.onClick.AddListener(OnDoneButtonClicked);
        deleteAllButton.onClick.AddListener(() => {
            GameManager.instance.mediaAccounts.Clear();
            AccountManager.SaveAccountsData(GameManager.instance.mediaAccounts);
            UpdateAccountDropdown();
            Debug.Log("Deleted all accounts.");
        });
        
        accountDropdown.onValueChanged.AddListener((value) => {
            if (value == 0) {
                serverDropdown.ClearOptions();
                serverDropdown.AddOptions(new List<string> { "Select an account first..." });
            }
            else {
                OnAccountSelect();
            }
        });
        
        serverDropdown.onValueChanged.AddListener((value) => {
            if (value == 0) {
                libraryDropdown.ClearOptions();
                libraryDropdown.AddOptions(new List<string> { "Select a server first..." });
            }
            else {
                OnServerSelect();
            }
        });
    }

    private void UpdateAccountDropdown() {
        accountDropdown.ClearOptions();
        accountDropdown.AddOptions(new List<string> { "Select an account..." });
        accountDropdown.AddOptions(GameManager.instance.mediaAccounts.ConvertAll(account => account.Username));
    }

    public void OnPlexButtonClicked() {
        accountDropdown.ClearOptions();
        accountDropdown.AddOptions(new List<string> { "Adding account..." });
        MediaAccount account = new PlexAccount();
        account.Setup((result) => {
            if (result) {
                Debug.Log("Plex account API is ready.");
                account.UpdateInfo((success) => {
                    if (success) {
                        Debug.Log("Account is ready.");
                        GameManager.instance.mediaAccounts.Add(account);
                        AccountManager.SaveAccountsData(GameManager.instance.mediaAccounts);
                        UpdateAccountDropdown();
                    }
                    else {
                        Debug.LogWarning("Failed to load account '" + account.Username + "' of type '" + account.GetType() + "'");
                        UpdateAccountDropdown();
                    }
                });
            }
            else {
                Debug.LogWarning("Failed to setup Plex account API.");
                UpdateAccountDropdown();
            }
        });
    }

    public void OnJellyfinButtonClicked() {
        accountDropdown.ClearOptions();
        accountDropdown.AddOptions(new List<string> { "Adding account..." });
        MediaAccount account = new JellyfinAccount();
        account.Setup((result) => {
            if (result) {
                Debug.Log("Jellyfin account API is ready.");
                account.UpdateInfo((success) => {
                    if (success) {
                        Debug.Log("Account is ready.");
                        GameManager.instance.mediaAccounts.Add(account);
                        AccountManager.SaveAccountsData(GameManager.instance.mediaAccounts);
                        UpdateAccountDropdown();
                    }
                    else {
                        Debug.LogWarning("Failed to load account '" + account.Username + "' of type '" + account.GetType() + "'");
                        UpdateAccountDropdown();
                    }
                });
            }
            else {
                Debug.LogWarning("Failed to setup Jellyfin account API.");
                UpdateAccountDropdown();
            }
        });
    }
    
    private void OnAccountSelect() {
        MediaAccount account = GameManager.instance.mediaAccounts[accountDropdown.value - 1];
        serverDropdown.ClearOptions();
        serverDropdown.AddOptions(new List<string> { "Loading servers..." });
        account.UpdateServerList((success) => {
            if (success) {
                serverDropdown.ClearOptions();
                serverDropdown.AddOptions(new List<string> { "Select a server..." });
                serverDropdown.AddOptions(account.Servers.ConvertAll(server => server.name));
            }
            else {
                Debug.LogWarning("Failed to load servers for account '" + account.Username + "'");
                serverDropdown.ClearOptions();
                serverDropdown.AddOptions(new List<string> { "Failed to load servers" });
            }
        });
    }
    
    public void OnServerSelect() {
        MediaAccount account = GameManager.instance.mediaAccounts[accountDropdown.value - 1];
        MediaServer server = account.Servers[serverDropdown.value - 1];
        libraryDropdown.ClearOptions();
        libraryDropdown.AddOptions(new List<string> { "Loading libraries..." });
        server.UpdateLibraryList((success) => {
            if (success) {
                libraryDropdown.ClearOptions();
                libraryDropdown.AddOptions(new List<string> { "Select a library..." });
                libraryDropdown.AddOptions(server.Libraries.ConvertAll(library => library.name));
            }
            else {
                Debug.LogWarning("Failed to load libraries for server '" + server.name + "'");
                libraryDropdown.ClearOptions();
                libraryDropdown.AddOptions(new List<string> { "Failed to load libraries" });
            }
        });
    }

    public void OnLibrarySelect() {
        MediaAccount account = GameManager.instance.mediaAccounts[accountDropdown.value - 1];
        MediaServer server = account.Servers[serverDropdown.value - 1];
        MediaLibrary library = server.Libraries[libraryDropdown.value - 1];
        Debug.Log("Selected library: " + library.name);
    }

    public void OnDoneButtonClicked() {
        MediaLibrary library = GameManager.instance.mediaAccounts[accountDropdown.value - 1].Servers[serverDropdown.value - 1].Libraries[libraryDropdown.value - 1];
        library.UpdateMediaList((callback) => {
            if (callback) {
                Debug.Log("Media list updated with " + library.MediaData.Count + " items.");
                
                PileStyle spawnStyle = new();
                StartCoroutine(spawnStyle.Create(Vector3.zero, library.MediaData));
            }
            else {
                Debug.LogWarning("Failed to update media list.");
            }
        });
    }
}