using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public List<DisplayTypes> displayTypes; // List of all display types (Configured in Unity)

    [HideInInspector] public List<MediaAccount> mediaAccounts = new();

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this);
        }

        LoadAccounts();
    }

    private void Start() {
        // TempFunction();
    }

    private void TempFunction() {
        MediaAccount account = new PlexAccount();

        account.Setup((callback) => {
            Debug.Log("Account is ready.");
            mediaAccounts.Add(account);
            account.UpdateInfo((callback) => { AccountManager.SaveAccountsData(mediaAccounts); });
        });
    }

    private void LoadAccounts() {
        mediaAccounts = AccountManager.LoadAccountsData();
        mediaAccounts.ForEach((account) => account.UpdateInfo((success) => {
            if (success) {
                Debug.Log("Successfully loaded account '" + account.Username + "' of type '" + account.GetType() + "'");
            }
            else {
                Debug.LogWarning("Failed to load account '" + account.Username + "' of type '" + account.GetType() + "'");
            }
        }));
    }
}