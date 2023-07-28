using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexServer {
    internal PlexSetup plexSetup;
    internal JObject serverData;
    public string name => (string)serverData["name"] ?? "N/A";
    public bool signedInUserIsOwner => (bool)serverData["owned"];
    public string connectionURL = "N/A";
    public string displayName => name + (signedInUserIsOwner ? " [Owner]" : "") + " (" + connectionURL + ")";

    internal List<PlexLibrary> plexLibraries = new();

    public PlexServer(PlexSetup plexSetup, JObject serverData) {
        this.plexSetup = plexSetup;
        this.serverData = serverData;


        JArray connections = JArray.Parse(serverData["connections"].ToString());
        foreach (JObject connection in connections) {
            Debug.Log("got connection: " + connection);
            if (!(bool)connection["local"]) { // TODO: Should use local if available, otherwise, should fallback to remote.
                Debug.Log("found remote connection");
                connectionURL = (string)connection["uri"];
                break; // TODO: Should test connection before breaking. If connection fails, should try next connection.
            }
            else {
                Debug.Log("found local connection");
            }
        }

        plexSetup.monoBehaviour.StartCoroutine(GetLibraries(response => { // TODO: Move to where selecting Library
            plexLibraries = response;
        }));
    }

    public IEnumerator GetLibraries(Action<List<PlexLibrary>> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(connectionURL + "/library/sections")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", plexSetup.authToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log(request.downloadHandler.text);

                List<PlexLibrary> libraries = new List<PlexLibrary>();

                JObject data = JObject.Parse(request.downloadHandler.text);
                JArray directoryArray = (JArray)data["MediaContainer"]?["Directory"];
                foreach (JObject directory in directoryArray ?? new JArray()) {
                    // string title = (string)directory["title"];
                    // string type = (string)directory["type"];
                    // string key = (string)directory["key"];
                    // long updatedAt = (long)directory["updatedAt"];
                    // long createdAt = (long)directory["createdAt"];
                    // long scannedAt = (long)directory["scannedAt"];
                    // string location = (string)directory["Location"]?.FirstOrDefault()?["path"];

                    libraries.Add(new PlexLibrary(this, directory));
                }

                callback(libraries);
            } else {
                Debug.LogError("Failed to get devices: " + request.error);
            }
        }
    }

}
