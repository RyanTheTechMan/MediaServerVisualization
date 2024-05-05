using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexServer : MediaServer {
    private readonly JObject serverData;
    public override string name => (string)serverData["name"];
    public bool signedInUserIsOwner => (bool)serverData["owned"];
    public string connectionURI { get; private set; } = "N/A";
    public bool isLocal { get; private set;}
    internal string accessToken => (string)serverData["accessToken"]; // sorta like the authToken, but allows you to access the server's API

    public PlexServer(PlexAccount account, JObject serverData) {
        this.Account = account;
        this.serverData = serverData;
        GameManager.instance.StartCoroutine(GetConnections());
    }

    private IEnumerator GetConnections() {
        Status = ServerStatus.WAITING;
        
        JArray connections = JArray.Parse(serverData["connections"].ToString());
        List<Coroutine> coroutines = new();
        foreach (JObject connection in connections) {
            coroutines.Add(GameManager.instance.StartCoroutine(TestConnection((bool)connection["local"], (string)connection["uri"])));
        }

        foreach (Coroutine coroutine in coroutines) {
            yield return coroutine;
        }

        Status = ServerStatus.READY;
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
            }
            else {
                Debug.LogError("Failed to get libraries for '" + name + "': " + request.error);
            }
        }
    }
    
    public override void UpdateLibraryList() {
        Debug.Log("Getting plex libraries...");
        GameManager.instance.StartCoroutine(GetLibraries(response => {
            Libraries = response.ConvertAll(library => (MediaLibrary)library);
            Debug.Log("Got " + Libraries.Count + " plex libraries.");
        }));
    }
}
