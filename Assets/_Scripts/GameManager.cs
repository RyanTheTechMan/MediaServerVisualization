using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public List<GameObject> mediaTypes;
    public MediaDomain mediaDomain;

    public Button plexButton;
    public TMP_Dropdown plexServerDropdown;
    public TMP_Dropdown plexLibraryDropdown;
    public Button doneButton;

    private void Start()
    {
    }

    private IEnumerator CreatePile(int mediaType, Vector3 position) {
        for (int i = 0; i < mediaDomain.mediaItems.Length; i++) {
            Vector3 posOffset = new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 5f), Random.Range(-8f, 8f));
            Vector3 velOffset = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 3f), Random.Range(-3f, 3f));

            GameObject newObject = Instantiate(mediaTypes[mediaType]);
            newObject.transform.position = position + posOffset;
            newObject.GetComponent<Rigidbody>().velocity = velOffset;

            DVDCase newCase = newObject.GetComponent<DVDCase>();
            newCase.mediaData = mediaDomain.mediaItems[i];

            newCase.UpdateArt(); // TODO: should be done after generating the pile for performance reasons

            yield return new WaitForSeconds(1f/100f);
        }
    }

    public void OnPlexButtonClicked() {
        mediaDomain = new PlexSetup(this);
        StartCoroutine(SetupPlexServerList());
}

    private IEnumerator SetupPlexServerList() {
        PlexSetup plexSetup = (PlexSetup)mediaDomain;
        while(!plexSetup.IsAPIReady()) {
            Debug.Log("Waiting for API to be ready...");
            yield return new WaitForSeconds(2);
        }

        Debug.Log("API is ready!");

        plexSetup.UpdateServerList();

        while (plexSetup.plexServers.Count == 0) {
            Debug.Log("Waiting for servers to be found...");
            yield return new WaitForSeconds(2);
        }

        plexServerDropdown.ClearOptions();
        plexServerDropdown.AddOptions(new List<string> { "Select a server..." });
        plexServerDropdown.AddOptions(plexSetup.plexServers.ConvertAll(server => server.displayName));

        plexSetup.selectedServer = null;

    }

    public void OnPlexServerDropdownChanged() {
        PlexSetup plexSetup = (PlexSetup)mediaDomain;
        if (plexServerDropdown.value == 0) return;
        plexSetup.selectedServer = plexSetup.plexServers[plexServerDropdown.value-1];

        StartCoroutine(SetupPlexLibraryList());
    }

    private IEnumerator SetupPlexLibraryList() {
        PlexSetup plexSetup = (PlexSetup)mediaDomain;

        while (plexSetup.plexLibraries.Count == 0) {
            Debug.Log("Waiting for sections to be found...");
            yield return new WaitForSeconds(2);
        }

        plexLibraryDropdown.ClearOptions();
        plexLibraryDropdown.AddOptions(new List<string> { "Select a library..." });
        plexLibraryDropdown.AddOptions(plexSetup.plexLibraries.ConvertAll(section => section.title));

        plexSetup.selectedLibrary = null;
    }

    public void OnPlexLibraryDropdownChanged() {
        PlexSetup plexSetup = (PlexSetup)mediaDomain;
        if (plexLibraryDropdown.value == 0) return;
        plexSetup.selectedLibrary = plexSetup.plexLibraries[plexLibraryDropdown.value-1];
    }

    public void OnDoneButtonClicked() {
        PlexSetup plexSetup = (PlexSetup)mediaDomain;
        plexSetup.UpdateMediaList();
        StartCoroutine(DisplayMediaList());

    }

    private IEnumerator DisplayMediaList() {
        while (mediaDomain.mediaItems.Length == 0) {
            Debug.Log("Waiting for media to be found...");
            yield return new WaitForSeconds(2);
        }

        StartCoroutine(CreatePile(0, new Vector3(0, 5, 0)));
    }
}
