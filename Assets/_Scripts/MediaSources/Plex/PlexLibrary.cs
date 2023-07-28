using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexLibrary {
    internal PlexServer plexServer;
    private readonly JObject libraryData; // Data about the library
    private JObject mediaData; // Data about the media in the library - Retrieved Async when needed
    public string title => (string)libraryData["title"];
    public Type type;
    public string key => (string)libraryData["key"];
    public List<PlexMediaData> mediaItems = new();
    public uint mediaCount => (uint)mediaData["MediaContainer"]?["size"]; // Will always be 0 if mediaData is null

    public PlexLibrary(PlexServer plexServer, JObject libraryData) {
        this.plexServer = plexServer;
        this.libraryData = libraryData;

        type = (string)libraryData["type"] switch {
            "movie" => Type.MOVIE,
            "show" => Type.SHOW,
            _ => Type.UNKNOWN
        };

        plexServer.plexSetup.monoBehaviour.StartCoroutine(GetItems(response => {
            mediaItems = response;
            Debug.Log("Got " + mediaItems.Count + " items in library " + title + " (" + plexServer.displayName + ")");
        }));
    }

    private IEnumerator GetItems(Action<List<PlexMediaData>> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(plexServer.connectionURL + "/library/sections/" + key + "/all")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", plexServer.accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log(request.downloadHandler.text);

                List<PlexMediaData> mediaDataList = new();

                JObject data = JObject.Parse(request.downloadHandler.text);
                JArray itemArray = (JArray)data["MediaContainer"]?["Metadata"];
                foreach (JObject mediaItem in itemArray ?? new JArray()) {
                    long totalSize = 0;
                    if (mediaItem["Media"] is JArray mediaArray) {
                        foreach (JObject media in mediaArray) {
                            JArray partArray = media["Part"] as JArray;
                            if (partArray != null) {
                                foreach (JObject part in partArray) {
                                    totalSize += (long)part["size"];
                                }
                            }
                        }
                    }

                    mediaDataList.Add(new PlexMediaData {
                        title = (string)(mediaItem["title"] ?? "N/A"),
                        description = (string)(mediaItem["summary"] ?? "N/A"),
                        plexID = (uint)(mediaItem["ratingKey"] ?? 0),
                        year = (uint)(mediaItem["year"] ?? 0),
                        duration = (uint)(mediaItem["duration"] ?? 0),
                        fileSize = totalSize,
                    });
                }

                callback(mediaDataList);
            } else {
                Debug.LogError("Failed to get libraries items for " + title + " (" + plexServer.displayName + "): " + request.error);
            }
        }
    }



    public enum Type {
        UNKNOWN,
        MOVIE,
        SHOW,
    }

}

