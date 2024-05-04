using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexServer {
    internal PlexSetup plexSetup;
    private readonly JObject serverData;
    public string name => (string)serverData["name"] ?? "N/A";
    public bool signedInUserIsOwner => (bool)serverData["owned"];
    public string connectionURI = "N/A";
    public bool isLocal = false;
    public bool connectionReady = false;
    public string displayName => name + (signedInUserIsOwner ? " [Owner]" : "") + " (" + connectionURI + ")";
    internal string accessToken => (string)serverData["accessToken"]; // sorta like the authToken, but allows you to access the server's API

    internal List<PlexLibrary> plexLibraries = new();
    
    public PlexServer(PlexSetup plexSetup, JObject serverData) {
        this.plexSetup = plexSetup;
        this.serverData = serverData;
        plexSetup.gameManager.StartCoroutine(GetConnections());
    }

    private IEnumerator GetConnections() {
        JArray connections = JArray.Parse(serverData["connections"].ToString());
        List<Coroutine> coroutines = new();
        foreach (JObject connection in connections) {
            coroutines.Add(plexSetup.gameManager.StartCoroutine(TestConnection((bool)connection["local"], (string)connection["uri"])));
        }
        foreach (Coroutine coroutine in coroutines) { yield return coroutine; }
        connectionReady = true;
    }

    private IEnumerator TestConnection(bool isLocal, string connectionURI) {
        using (UnityWebRequest request = UnityWebRequest.Get(connectionURI)) {
            request.SetRequestHeader("X-Plex-Token", accessToken);
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                if (!this.isLocal) {
                    if (isLocal) {
                        this.isLocal = true;
                        this.connectionURI = connectionURI;
                    }
                    else {
                        this.connectionURI = connectionURI;
                    }
                }
                // Debug.Log("Successfully got connection for '" + name + "(" + connectionURI + ")" + "': " + request.result);
            }
            else {
                // Debug.LogError("Failed to get connection for '" + name + "(" + connectionURI + ")" + "': " + request.error);
            }
        }
    }

    public IEnumerator GetLibraries(Action<List<PlexLibrary>> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(connectionURI + "/library/sections")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", accessToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                // Debug.Log(request.downloadHandler.text);

                List<PlexLibrary> libraries = new();

                JObject data = JObject.Parse(request.downloadHandler.text);
                JArray directoryArray = (JArray)data["MediaContainer"]?["Directory"];
                foreach (JObject directory in directoryArray ?? new JArray()) {
                    libraries.Add(new PlexLibrary(this, directory));
                }

                callback(libraries);
            } else {
                Debug.LogError("Failed to get libraries for '" + displayName + "': " + request.error);
            }
        }
    }

}
