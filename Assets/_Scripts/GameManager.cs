using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public List<DisplayType> displayTypes; // List of all display types (Configured in Unity)

    [HideInInspector] public List<MediaAccount> mediaAccounts = new();

    private void Awake() {
        if (instance == null) instance = this;
        else Destroy(this);

        // PlayerPrefs.DeleteAll();
        LoadAccounts();
    }

    private void Start() {
        // SpawnStyleTest();
    }

    private void SpawnStyleTest() {
        // create 1000 random media data objects
        List<MediaData> mediaData = new();
        for (int i = 0; i < 1000; i++) {
            mediaData.Add(new PlexMediaData {
                title = "Title " + i,
                description = "example description",
            });
        }

        PileStyle spawnStyle = new();
        StartCoroutine(spawnStyle.Create(Vector3.zero, mediaData));
    }

    private void LoadAccounts() {
        mediaAccounts = AccountManager.LoadAccountsData();
        mediaAccounts.ForEach((account) => account.UpdateInfo((success) => {
            if (success) Debug.Log("Successfully loaded account '" + account.Username + "' of type '" + account.GetType() + "'");
            else Debug.LogWarning("Failed to load account '" + account.Username + "' of type '" + account.GetType() + "'");
        }));
    }
}