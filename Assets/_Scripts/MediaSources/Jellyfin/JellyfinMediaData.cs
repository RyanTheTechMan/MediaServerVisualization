using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class JellyfinMediaData : MediaData {
    public string jellyfinID; // a.k.a. ratingKey

    public override IEnumerator UpdateMainArtTexture() {
        // Debug.Log("UpdateMainArtTexture: " + coverArtURI);
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(coverArtURI)) {
            request.SetRequestHeader("Authorization", ((JellyfinAccount)MediaLibrary.Server.Account).authorization);
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                coverArtTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                // Debug.Log("UpdateMainArtTexture: " + coverArtTexture.width + "x" + coverArtTexture.height);
            }
            else if (request.responseCode == 404) {
                Debug.LogWarning("No main art found for " + title);
            }
            else {
                Debug.LogError("Failed to download cover art: " + request.error);
            }
        }
    }

    public override IEnumerator UpdateBackgroundArtTexture() {
        Debug.Log("UpdateBackgroundArtTexture: " + backgroundArtURI);
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(backgroundArtURI)) {
            request.SetRequestHeader("Authorization", ((JellyfinAccount)MediaLibrary.Server.Account).authorization);
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                backgroundArtTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Debug.Log("UpdateBackgroundArtTexture: " + backgroundArtTexture.width + "x" + backgroundArtTexture.height);
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
