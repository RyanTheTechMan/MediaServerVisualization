using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;

public class JellyfinLibrary : MediaLibrary {
    private readonly JObject libraryData;
    public override string name => (string) libraryData["Name"];
    public string id => (string)libraryData["Id"];
    public string collectionType => (string)libraryData["CollectionType"];

    public JellyfinLibrary(JellyfinServer jellyfinServer, JObject libraryData) {
        Server = jellyfinServer;
        this.libraryData = libraryData;

        libraryType = collectionType switch {
            "movies" => LibraryType.MOVIE,
            "tvshows" => LibraryType.SHOW,
            _ => LibraryType.UNKNOWN
        };
    }
    
    public IEnumerator GetItems(Action<List<JellyfinMediaData>> callback) {
        // Debug.Log(id);
        using (UnityWebRequest request = UnityWebRequest.Get(((JellyfinAccount)Server.Account).ServerURL + "/Items?sortOrder=Descending&sortOrder=Descending&enableImages=true&fields=Overview&parentId=" + id + "&userId=" + ((JellyfinAccount)Server.Account).UserId)) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Authorization", ((JellyfinAccount)Server.Account).authorization);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                // Debug.Log(request.downloadHandler.text);

                List<JellyfinMediaData> mediaDataList = new();

                JObject data = JObject.Parse(request.downloadHandler.text);
                JArray itemArray = (JArray)data["Items"];
                foreach (JObject mediaItem in itemArray ?? new JArray()) {
                    mediaDataList.Add(new JellyfinMediaData {
                        title = (string)(mediaItem["Name"] ?? "N/A"),
                        description = (string)(mediaItem["Overview"] ?? "N/A"),
                        jellyfinID = (string)mediaItem["Id"] ?? "N/A",
                        year = (uint)(mediaItem["ProductionYear"] ?? 0),
                        duration = (ulong)(mediaItem["RunTimeTicks"] ?? 0),
                        coverArtURI = CreateResizedImageUrl(((JellyfinAccount)Server.Account).ServerURL, (string)mediaItem["Id"], "Primary"),
                        backgroundArtURI = CreateResizedImageUrl(((JellyfinAccount)Server.Account).ServerURL, (string)mediaItem["Id"], "Backdrop"),
                        MediaLibrary = this,
                    });
                }

                callback(mediaDataList);
            }
            else {
                Debug.LogError("Failed to get libraries items for " + name + " (" + Server.name + "): " + request.error);
            }
        }
    }
    private static string CreateResizedImageUrl(string baseUrl, string imageID, string imageType, int width = 4000, int height = 4000) {
        return $"{baseUrl}/Items/{imageID}/Images/{imageType}?maxWidth={width}&maxHeight={height}";
    }

    public override void UpdateMediaList(Action<bool> callback) {
        Debug.Log("Getting jellyfin media...");
        GameManager.instance.StartCoroutine(GetItems(response => {
            MediaData = response.ConvertAll(media => (MediaData)media);
            Debug.Log("Got " + MediaData.Count + " items in library " + name + " (" + Server.name + ")");
            callback(true);
        }));
    }
}