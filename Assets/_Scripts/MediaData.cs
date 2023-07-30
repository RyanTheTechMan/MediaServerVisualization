using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MediaData : MonoBehaviour {
    public string title;
    public string description;
    public uint year;
    public uint duration;
    public long fileSize;

    public string coverArtURI;
    public string backgroundArtURI;
    public Texture2D coverArtTexture;
    public Texture2D backgroundArtTexture;

    public MediaDomain mediaDomain;

    public virtual IEnumerator UpdateMainArtTexture() {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(coverArtURI)) {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                coverArtTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
            else if (request.responseCode == 404) {
                Debug.LogWarning("No main art found for " + title);
            }
            else {
                Debug.LogError("Failed to download cover art: " + request.error);
            }
        }
    }

    public virtual IEnumerator UpdateBackgroundArtTexture() {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(backgroundArtURI)) {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                backgroundArtTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
            else if (request.responseCode == 404) {
                Debug.LogWarning("No background art found for " + title);
            }
            else {
                Debug.LogError("Failed to download cover art: " + request.error);
            }
        }
    }
}

