using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexLibrary : MediaLibrary {
    private readonly JObject libraryData; // Data about the library
    public override string name => (string)libraryData["title"];
    public string key => (string)libraryData["key"];

    public PlexLibrary(PlexServer plexServer, JObject libraryData) {
        Server = plexServer;
        this.libraryData = libraryData;

        libraryType = (string)libraryData["type"] switch {
            "movie" => LibraryType.MOVIE,
            "show" => LibraryType.SHOW,
            _ => LibraryType.UNKNOWN
        };
    }

    public IEnumerator GetItems(Action<List<PlexMediaData>> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(((PlexServer)Server).connectionURI + "/library/sections/" + key + "/all")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", ((PlexServer)Server).accessToken);
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                // Debug.Log(request.downloadHandler.text);

                List<PlexMediaData> mediaDataList = new();

                JObject data = JObject.Parse(request.downloadHandler.text);
                JArray itemArray = (JArray)data["MediaContainer"]?["Metadata"];
                foreach (JObject mediaItem in itemArray ?? new JArray()) {
                    long totalSize = 0;
                    if (mediaItem["Media"] is JArray mediaArray) {
                        foreach (JObject media in mediaArray) {
                            if (media["Part"] is not JArray partArray) continue;
                            totalSize += partArray.Cast<JObject>().Sum(part => (long)part["size"]);
                        }
                    }

                    Debug.Log(((PlexServer)Server).connectionURI + (string)mediaItem["thumb"] + ".jpg");

                    mediaDataList.Add(new PlexMediaData {
                        title = (string)(mediaItem["title"] ?? "N/A"),
                        description = (string)(mediaItem["summary"] ?? "N/A"),
                        plexID = (uint)(mediaItem["ratingKey"] ?? 0),
                        year = (uint)(mediaItem["year"] ?? 0),
                        duration = (ulong)(mediaItem["duration"] ?? 0),
                        fileSize = totalSize,
                        coverArtURI = CreateResizedImageUrl(((PlexServer)Server).connectionURI, (string)mediaItem["thumb"]),
                        backgroundArtURI = CreateResizedImageUrl(((PlexServer)Server).connectionURI, (string)mediaItem["art"]),
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

    private static string CreateResizedImageUrl(string baseUrl, string imageUri, int width = 4000, int height = 4000) {
        return $"{baseUrl}/photo/:/transcode?width={width}&height={height}&minSize=1&upscale=0&url={imageUri}";
    }

    public override void UpdateMediaList(Action<bool> callback) {
        Debug.Log("Getting plex media...");
        GameManager.instance.StartCoroutine(GetItems(response => {
            MediaData = response.ConvertAll(media => (MediaData)media);
            Debug.Log("Got " + MediaData.Count + " items in library " + name + " (" + Server.name + ")");
            callback(true);
        }));
    }
}