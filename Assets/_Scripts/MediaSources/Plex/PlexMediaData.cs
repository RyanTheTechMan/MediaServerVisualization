using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlexMediaData : MediaData {
    public uint plexID; // a.k.a. ratingKey

    public override IEnumerator UpdateCoverArtTexture() {
        Debug.Log("UpdateCoverArtTexture: " + coverArtURI);
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(coverArtURI)) {
            request.SetRequestHeader("X-Plex-Token", ((PlexSetup)mediaDomain).selectedServer.accessToken);
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                coverArtTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Debug.Log("UpdateCoverArtTexture: " + coverArtTexture.width + "x" + coverArtTexture.height);
            }
            else {
                Debug.LogError("Failed to download cover art: " + request.error);
            }
        }
    }

    public override IEnumerator UpdateBackgroundTexture() {
        Debug.Log("UpdateBackgroundTexture: " + backgroundArtURI);
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(backgroundArtURI)) {
            request.SetRequestHeader("X-Plex-Token", ((PlexSetup)mediaDomain).selectedServer.accessToken);
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                backgroundArtTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Debug.Log("UpdateBackgroundTexture: " + backgroundArtTexture.width + "x" + backgroundArtTexture.height);
            }
            else {
                Debug.LogError("Failed to download cover art: " + request.error);
            }
        }
    }
}