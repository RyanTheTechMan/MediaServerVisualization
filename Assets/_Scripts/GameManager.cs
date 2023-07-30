// ReSharper disable once RedundantUsingDirective
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public List<MediaType> mediaTypes;
    public MediaDomain mediaDomain;

    public Button plexButton;
    public TMP_Dropdown plexServerDropdown;
    public TMP_Dropdown plexLibraryDropdown;
    public Button doneButton;

    private void Start()
    {
    }

    private IEnumerator CreatePile(int mediaTypeIndex, Vector3 position) {
        const int spawnGroupSize = 10;
        int groupSize = 0;
        List<MediaType> mediaObjects = new();
        foreach (MediaData mediaItem in mediaDomain.mediaItems) {
            Vector3 posOffset = new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 5f), Random.Range(-8f, 8f));
            Vector3 velOffset = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 3f), Random.Range(-3f, 3f));

            MediaType newObject = Instantiate(mediaTypes[mediaTypeIndex].gameObject).GetComponent<MediaType>();
            newObject.transform.position = position + posOffset;
            newObject.GetComponent<Rigidbody>().velocity = velOffset;

            newObject.mediaData = mediaItem;
            mediaObjects.Add(newObject);

            if (groupSize >= spawnGroupSize) {
                groupSize = 0;
                yield return new WaitForSeconds(1f/100f);
            }
            groupSize++;
        }


        // TODO: This should be done as you get close to the art in a queued fashion. Not all at once, let alone, sequentially.
        foreach (MediaType mediaObject in mediaObjects) {
            StartCoroutine(mediaObject.UpdateArt());
            yield return new WaitForSeconds(0.5f);
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
        mediaDomain.mediaItems = null;
        plexSetup.UpdateMediaList();
        StartCoroutine(DisplayMediaList());

    }

    private IEnumerator DisplayMediaList() {
        while (mediaDomain.mediaItems == null) {
            Debug.Log("Waiting for media to be found...");
            yield return new WaitForSeconds(2);
        }

        StartCoroutine(CreatePile(0, new Vector3(0, 5, 0)));
    }
}
