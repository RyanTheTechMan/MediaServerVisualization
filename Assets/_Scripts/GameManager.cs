using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MediaDomain mediaDomain; // The currently selected media domain (Plex, Emby, Jellyfin, etc.)
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        mediaDomain = new PlexSetup(); // TEMPORARY - Goes around UI
        
        StartCoroutine(AddAccountToMediaDomain());
    }

    private IEnumerator AddAccountToMediaDomain()
    {
        
        StartCoroutine(mediaDomain.AddAccount()); // (Should be called on have an add account button)
        
        while(!mediaDomain.IsAccountReady()) {
            Debug.Log("Waiting for Account to be added...");
            yield return new WaitForSeconds(2);
        }

        Debug.Log("Account has been added.");
        
        StartCoroutine(mediaDomain.SetupServerList());
    }
}