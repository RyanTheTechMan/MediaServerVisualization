using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public List<DisplayType> displayTypes; // List of all display types (Configured in Unity)
    public List<MediaSource> mediaSources; // List of all media sources (Configured in Unity)

    [HideInInspector] public List<MediaAccount> mediaAccounts = new();
    
    [HideInInspector] public List<SpawnStyle> spawnStyles;

    private void Awake() {
        if (instance == null) instance = this;
        else Destroy(this);

        GetSpawnStyles();

        LoadAccounts();
    }

    private void Start() {
    }

    private void LoadAccounts() {
        mediaAccounts = AccountManager.LoadAccountsData();
        mediaAccounts.ForEach((account) => account.UpdateInfo((success) => {
            if (success) Debug.Log("Successfully loaded account '" + account.Username + "' of type '" + account.GetType() + "'");
            else Debug.LogWarning("Failed to load account '" + account.Username + "' of type '" + account.GetType() + "'");
        }));
    }
    
    private void GetSpawnStyles() {
        spawnStyles = new List<SpawnStyle>();
        foreach (Type type in typeof(SpawnStyle).Assembly.GetTypes()) {
            if (type.IsSubclassOf(typeof(SpawnStyle))) {
                spawnStyles.Add((SpawnStyle) Activator.CreateInstance(type));
            }
        }
    }
}