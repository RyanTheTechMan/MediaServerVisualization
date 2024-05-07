using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class JellyfinServer : MediaServer {
    public JellyfinServer(JellyfinAccount account) {
        Account = account;
        GameManager.instance.StartCoroutine(Setup());
    }

    private IEnumerator Setup() {
        Status = ServerStatus.WAITING;

        using (UnityWebRequest request = UnityWebRequest.Get(((JellyfinAccount)Account).ServerURL + "/System/Info")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Authorization", ((JellyfinAccount)Account).authorization);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                JObject data = JObject.Parse(request.downloadHandler.text);
                // string localIP = (string)data["LocalAddress"]; TODO: Add local connection checking
                name = (string)data["ServerName"];
                Status = ServerStatus.READY;
            }
            else {
                Debug.LogError("Failed to get servername for IP address: " + ((JellyfinAccount)Account).ServerURL + " : " + request.error);
                Status = ServerStatus.ERROR;
            }
        }
    }

    public IEnumerator GetLibraries(Action<List<JellyfinLibrary>> callback) {
        //Debug.Log(((JellyfinAccount)Account).ServerURL + "/Items?userId=" + ((JellyfinAccount)Account).UserId);
        using (UnityWebRequest request = UnityWebRequest.Get(((JellyfinAccount)Account).ServerURL + "/Items?userId=" + ((JellyfinAccount)Account).UserId)) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Authorization", ((JellyfinAccount)Account).authorization);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                List<JellyfinLibrary> libraries = new();
                JObject data = JObject.Parse(request.downloadHandler.text);
                //Debug.Log(data);
                JArray itemsArray = (JArray)data["Items"];
                foreach (JObject item in itemsArray ?? new JArray()) {
                    libraries.Add(new JellyfinLibrary(this, item));
                }

                callback(libraries);
            }
            else {
                Debug.LogError("Failed to get libraries for " + name + " : " + request.error);
                Status = ServerStatus.ERROR;
            }
        }
    }
    
    public override void UpdateLibraryList(Action<bool> callback) {
        Debug.Log("Getting jellyfin libraries...");
        GameManager.instance.StartCoroutine(GetLibraries(response => {
            Libraries = response.ConvertAll(library => (MediaLibrary)library);
            Debug.Log("Got " + Libraries.Count + " jellyfin libraries.");
            callback(true);
        }));
    }
}
