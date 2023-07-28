using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public List<GameObject> mediaTypes;
    public MediaDomain mediaDomain;

    // Start is called before the first frame update
    void Start()
    {
        // In this project, we will make a way to create a visualization of DVD cases. These cases will be shown in a library looking scene where you can walk up to a case and see the cover art and information about the movie.
        // You will be able to walk around the library and see all the cases. You will also be able to pick up a case and put it back down. The case will be able to be picked up and put down anywhere in the library.
        // The case will also be able to be put back in the same spot it was picked up from.

        // GameObject[] mediaObject = new GameObject[10];
        // for (int i = 0; i < mediaObject.Length; i++)
        // {
        //     GameObject newObject = Instantiate(mediaTypes[0]);
        //     newObject.transform.position = new Vector3(i, 2, 0);
        //     mediaObject[i] = newObject;
        // }
        //
        // foreach (GameObject media in mediaObject)
        // {
        //     media.GetComponent<DVDCase>().mediaData.title = "DVD Case " + media.transform.position.x;
        //     media.GetComponent<DVDCase>().mediaData.description = "This is a DVD case.";
        //     media.GetComponent<DVDCase>().mediaData.coverArt = "https://cdn.discordapp.com/attachments/749653590326378499/1134322123008127096/3071240_sa.png";
        //
        //     media.GetComponent<DVDCase>().mediaData.UpdateMediaData();
        // }

        // while (mediaDomain.GetToken() == null) {
            // get user to login, show login window
            if (true) {
                // assume plex was selected
                StartCoroutine(DisplayPlex());

            }

            // }

        CreatePile(0, new Vector3(0, 5, 0));
    }

    private IEnumerator DisplayPlex() {
        mediaDomain = new PlexSetup(this);
        PlexSetup plexSetup = (PlexSetup)mediaDomain;

        yield return null;
    }

    public void CreatePile(int mediaType, Vector3 position) {

        MediaData mediaData = new MediaData();
        mediaData.title = "DVD Case";
        mediaData.description = "This is a DVD case.";
        // mediaData.coverArt = "https://cdn.discordapp.com/attachments/749653590326378499/1134322123008127096/3071240_sa.png";

        for (int i = 0; i < 500; i++) {
            Vector3 posOffset = new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 5f), Random.Range(-8f, 8f));
            Vector3 velOffset = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 3f), Random.Range(-3f, 3f));

            GameObject newObject = Instantiate(mediaTypes[mediaType]);
            newObject.transform.position = position + posOffset;
            newObject.GetComponent<Rigidbody>().velocity = velOffset;

            DVDCase newCase = newObject.GetComponent<DVDCase>();
            newCase.mediaData = mediaData;

            if (i == 0) {
                newCase.UpdateVisuals();
            }
        }
    }
}
