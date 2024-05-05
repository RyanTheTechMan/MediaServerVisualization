using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerSelect : MonoBehaviour {
    
    public List<DisplayTypes> mediaTypes;
    public MediaSource MediaSource;

    public Button plexButton;
    public TMP_Dropdown plexServerDropdown;
    public TMP_Dropdown plexLibraryDropdown;
    public Button doneButton;

    private void Start()
    {
    }
/*
    public void OnPlexButtonClicked() {
        MediaSource = new PlexSetup(this);
        StartCoroutine(SetupPlexServerList());
    }

    private IEnumerator SetupPlexServerList() {
        PlexSetup plexSetup = (PlexSetup)MediaSource;
        while(!plexSetup.IsAccountReady()) {
            Debug.Log("Waiting for API to be ready...");
            yield return new WaitForSeconds(2);
        }

        Debug.Log("API is ready!");

        plexSetup.UpdateServerList();

        yield return new WaitUntil(() => plexSetup.plexServers.Find(server => server.connectionReady) != null);

        plexServerDropdown.ClearOptions();
        plexServerDropdown.AddOptions(new List<string> { "Select a server..." });

        plexSetup.selectedServer = null;

        foreach (PlexServer server in plexSetup.plexServers) {
            StartCoroutine(TestConnections(server));
        }
    }

    private IEnumerator TestConnections(PlexServer server)
    {
        yield return new WaitWhile(() => !server.connectionReady);
        plexServerDropdown.AddOptions(new List<string> { server.displayName });
    }

    public void OnPlexServerDropdownChanged() {
        PlexSetup plexSetup = (PlexSetup)MediaSource;
        if (plexServerDropdown.value == 0) return;
        plexSetup.selectedServer = plexSetup.plexServers[plexServerDropdown.value-1];

        StartCoroutine(SetupPlexLibraryList());
    }

    private IEnumerator SetupPlexLibraryList() {
        PlexSetup plexSetup = (PlexSetup)MediaSource;

        yield return new WaitUntil(() => plexSetup.plexLibraries.Count > 0);

        plexLibraryDropdown.ClearOptions();
        plexLibraryDropdown.AddOptions(new List<string> { "Select a library..." });
        plexLibraryDropdown.AddOptions(plexSetup.plexLibraries.ConvertAll(section => section.title));

        plexSetup.selectedLibrary = null;
    }

    public void OnPlexLibraryDropdownChanged() {
        PlexSetup plexSetup = (PlexSetup)MediaSource;
        if (plexLibraryDropdown.value == 0) return;
        plexSetup.selectedLibrary = plexSetup.plexLibraries[plexLibraryDropdown.value-1];
    }

    public void OnDoneButtonClicked() {
        PlexSetup plexSetup = (PlexSetup)MediaSource;
        MediaSource.mediaItems = null;
        plexSetup.UpdateMediaList();
        StartCoroutine(DisplayMediaList());

    }

    private IEnumerator DisplayMediaList() {
        while (MediaSource.mediaItems == null) {
            Debug.Log("Waiting for media to be found...");
            yield return new WaitForSeconds(2);
        }

        StartCoroutine(CreatePile(0, new Vector3(0, 5, 0)));
    }
    */
}