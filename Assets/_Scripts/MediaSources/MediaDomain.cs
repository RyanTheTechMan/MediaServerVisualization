using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MediaDomain {
    public readonly GameManager gameManager = GameManager.instance; // TODO: there is a chance this doesnt get GameManager
    public List<Account> accounts = new();
    public MediaData[] mediaItems = null;
    protected bool accountReady = false; // When true, functions that return data can be called

    // Returns null if token doesn't exist
    public bool IsAccountReady() {
        return accountReady;
    }

    // Returns a list of each server from the added accounts (For plex, this will be all servers in general, but for Jellyfin, this will be each server from the added accounts)
    public abstract IEnumerator SetupServerList();

    public abstract IEnumerator AddAccount();
}