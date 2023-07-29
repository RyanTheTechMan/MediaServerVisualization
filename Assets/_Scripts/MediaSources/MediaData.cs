using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MediaData {
    public string title;
    public string description;
    public string coverArt;
    public Texture2D coverArtTexture;
    // public string imdbID;
    // public string tvdbID;
    // public string tmdbID;
    public uint year;
    public uint duration;
    public long fileSize;

    public bool UpdateMediaData() {
        // requires imdbID, tvdbID, or tmdbID

        // if (imdbID == null && tvdbID == null && tmdbID == null) {
        //     Debug.LogError("No ID provided to get media data.");
        //     return false;
        // }

        // Get media data from the internet here. Maybe use OMDB API?

        if (coverArt == null) {
            Debug.LogError("No cover art found.");
            return false;
        }


        return true;
    }

    private IEnumerator GetCoverArtTexture(string url, Action<Texture2D> callback) {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url)) {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                callback?.Invoke(texture);
            }
            else {
                Debug.LogError("Failed to download cover art: " + request.error);
            }
        }
    }

    public void GetCoverArtTexture(MonoBehaviour monoBehaviour) {
        monoBehaviour.StartCoroutine(GetCoverArtTexture(coverArt, (texture) => {
            coverArtTexture = texture;
        }));
    }
}

