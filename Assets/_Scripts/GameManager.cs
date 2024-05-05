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
    }

    private void Start() {
        TempFunction();
    }

    private void TempFunction() {
        MediaAccount account = new PlexAccount();

        account.Setup((callback) => {
            Debug.Log("Account is ready.");

            mediaAccounts.Add(account);
            account.UpdateServerList((result) => {
                account.Servers.ForEach(server => Debug.Log(server.name));
                account.Servers[0].UpdateLibraryList((result) => {
                    account.Servers[0].Libraries.ForEach(libary => Debug.Log(libary.name));
                    account.Servers[0].Libraries[0].UpdateMediaList((result) => { account.Servers[0].Libraries[0].MediaData.ForEach(media => Debug.Log(media.title)); });
                });
            });
        });
    }
}