using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public List<DisplayTypes> displayTypes; // List of all display types (Configured in Unity)
    
    [HideInInspector]
    public List<MediaAccount> mediaAccounts = new();

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this);
        }
    }

    private void Start() {
        TempFunction();
    }

    private void TempFunction() {
        MediaAccount account = new PlexAccount();

        account.Setup(() => {
            Debug.Log("Account is ready.");
        
            mediaAccounts.Add(account);
        });
    }
}