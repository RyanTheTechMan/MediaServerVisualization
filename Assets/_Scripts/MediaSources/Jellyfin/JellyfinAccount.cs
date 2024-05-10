using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

// https://jellyfin.org/docs/general/contributing/branding
// https://github.com/jellyfin/jellyfin-ux/tree/master/branding/SVG (banner-light / banner-dark & icon-solid-white)

public class JellyfinAccount : MediaAccount {
    internal string authorization => "MediaBrowser Token=\"" + AccessToken + "\"";
    [JsonProperty] internal string AccessToken = "";
    [JsonProperty] internal string ServerURL = ""; // TODO: Add slot to add server ip.
    [JsonProperty] internal string UserId = "";

    public override void Setup(Action<bool> callback) {
        Status = AccountStatus.WAITING;
        GameManager.instance.StartCoroutine(GenerateToken((accessToken, userId) => {
            AccessToken = accessToken;
            UserId = userId;
            Status = AccountStatus.READY;
            callback(accessToken != null);
        }));
    }

    private IEnumerator GenerateToken(Action<string, string> callback) {
        // Convert the JSON string to byte array
        string Username = ""; // TODO: Add slots to type in userinfo
        string Pw = "";
        string jsonPayload = "{\"Username\":\"" + Username + "\", \"Pw\":\"" + Pw + "\"}";

        //Debug.Log("URI: " + ServerURL + "/Users/AuthenticateByName " + "Data: " + jsonPayload);
        var jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(ServerURL + "/Users/AuthenticateByName", "POST")) {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "MediaBrowser Client=\"other\", Device=\"my-script\", DeviceId=\"some-unique-id\", Version=\"0.0.0\"");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                JObject data = JObject.Parse(request.downloadHandler.text);
                callback(data["AccessToken"]?.ToString(), data["User"]?["Id"]?.ToString());
            }
            else {
                Debug.LogError("Failed to generate token : " + request.error);
                callback(null, null);
            }
        }
    }

    public override void UpdateServerList(Action<bool> callback) {
        Servers = new() { new JellyfinServer(this) };
        callback(true);
    }

    public override void UpdateInfo(Action<bool> callback) {
        GameManager.instance.StartCoroutine(UpdateInfoCoroutine(callback));
    }

    private IEnumerator UpdateInfoCoroutine(Action<bool> callback) {
        Status = AccountStatus.WAITING;

        List<Coroutine> coroutines = new();

        bool failed = false;

        coroutines.Add(GameManager.instance.StartCoroutine(GetUsername((username) => {
            Username = username;
            if (username == null) {
                failed = true;
            }
        })));

        foreach (Coroutine coroutine in coroutines) {
            yield return coroutine;
        }

        if (failed) {
            Status = AccountStatus.ERROR;
            callback(false);
        }
        else {
            Status = AccountStatus.READY;
            callback(true);
        }
    }

    public IEnumerator GetUsername(Action<string> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(ServerURL + "/Users/Me")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("Authorization", authorization);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                JObject data = JObject.Parse(request.downloadHandler.text);
                Username = (string)data["Name"];
                callback(Username);
            }
            else {
                Debug.LogError("Failed to get servername for IP address: " + ServerURL + " : " + request.error);
                callback(null);
                ;
            }
        }
    }
}