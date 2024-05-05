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
        MediaAccount account = new PlexAccount(); // TEMPORARY - Goes around UI

        StartCoroutine(SetupAccount(account));
    }

    private IEnumerator SetupAccount(MediaAccount account) {
        account.Setup();
        while(account.Status != AccountStatus.READY) {
            Debug.Log("Waiting for Account to be ready...");
            yield return new WaitForSeconds(2);
        }
        
        Debug.Log("Account is ready.");
        
        mediaAccounts.Add(account);
    }
}