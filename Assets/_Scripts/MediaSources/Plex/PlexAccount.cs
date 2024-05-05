﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexAccount : MediaAccount {
    public string clientIdentifier;
    public string authToken;

    public override void Setup() {
        Status = AccountStatus.WAITING;
        clientIdentifier = "MSV-" + Guid.NewGuid().ToString();

        GameManager.instance.StartCoroutine(GenerateAndCheckPin());
    }

    public void Validate() {
        Status = AccountStatus.WAITING;
        GameManager.instance.StartCoroutine(CheckAccessTokenValidity(authToken, (valid) => {
            if (!valid) {
                Debug.LogWarning("Plex access token is invalid.");
                Status = AccountStatus.ERROR;
                GameManager.instance.StartCoroutine(GenerateAndCheckPin());
            }
            else {
                Debug.Log("Plex access token is valid.");
                Status = AccountStatus.READY;
            }
        }));
    }
    
    public IEnumerator GetServers(Action<List<PlexServer>> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get("https://plex.tv/api/v2/resources")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", authToken);
            request.SetRequestHeader("X-Plex-Product", Constants.productName);
            request.SetRequestHeader("X-Plex-Client-Identifier", clientIdentifier);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                // Debug.Log(request.downloadHandler.text);

                JArray jDevices = JArray.Parse(request.downloadHandler.text);

                List<PlexServer> servers = new();
                foreach (JObject device in jDevices) {
                    if (device["provides"]?.ToString() != "server") continue; // Skip non-server devices
                    servers.Add(new PlexServer(this, device));
                }

                callback(servers);
            }
            else {
                Debug.LogError("Failed to get devices: " + request.error);
            }
        }
    }
    
    public IEnumerator CheckAccessTokenValidity(string token, Action<bool> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get("https://plex.tv/api/v2/user")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Product", Constants.productName);
            request.SetRequestHeader("X-Plex-Client-Identifier", clientIdentifier);
            request.SetRequestHeader("X-Plex-Token", token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                callback(true);
            }
            else {
                callback(false);
            }
        }
    }

    public IEnumerator GeneratePin(Action<string, string> callback) {
        WWWForm form = new WWWForm();
        form.AddField("strong", "true");

        using (UnityWebRequest request = UnityWebRequest.Post("https://plex.tv/api/v2/pins", form)) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Product", Constants.productName);
            request.SetRequestHeader("X-Plex-Client-Identifier", clientIdentifier);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                JObject data = JObject.Parse(request.downloadHandler.text);
                callback(data["id"]?.ToString(), data["code"]?.ToString());
            }
            else {
                Debug.LogError("Failed to generate PIN: " + request.error);
            }
        }
    }

    public string GetPinUrl(string pinCode) {
        var uriBuilder = new UriBuilder("https://app.plex.tv/auth") {
            Fragment = "?"
        };

        var parameters = new NameValueCollection() {
            { "clientID", clientIdentifier },
            { "code", pinCode },
            { "context[plexDevice][product]", Constants.productName },
            // { "forwardUrl", "https://github.com/RyanTheTechMan/MediaServerVisualization" } // TODO: Add redirect to github "Thank you for signing in page"
        };

        var array = new StringBuilder();
        foreach (string key in parameters.Keys) {
            array.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(parameters[key]));
        }

        if (array.Length > 0) {
            array.Length--;
        }

        uriBuilder.Fragment += array.ToString();

        return uriBuilder.ToString();
    }

    /// <summary>
    /// Verifies the given PIN with Plex servers and retrieves an authentication token if the PIN is valid.
    /// </summary>
    /// <param name="pinId">The unique identifier for the PIN.</param>
    /// <param name="pinCode">The PIN code to verify.</param>
    /// <param name="callback">Action to call with the authentication token if retrieval is successful.</param>
    public IEnumerator VerifyPinAndRetrieveToken(string pinId, string pinCode, Action<string> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get($"https://plex.tv/api/v2/pins/{pinId}?code={pinCode}")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Client-Identifier", clientIdentifier);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                JObject data = JObject.Parse(request.downloadHandler.text);
                callback(data["authToken"]?.ToString());
            }
            else {
                Debug.LogError("Failed to check PIN: " + request.error);
            }
        }
    }

    /// <summary>
    /// Checks the given PIN and validates the access token.
    /// </summary>
    /// <param name="pinId">The unique identifier for the PIN.</param>
    /// <param name="pinCode">The PIN code to check.</param>
    public IEnumerator CheckPinAndValidateAccessToken(string pinId, string pinCode) {
        Status = AccountStatus.WAITING;
        yield return new WaitForSeconds(10);

        yield return VerifyPinAndRetrieveToken(pinId, pinCode, (token) => {
            if (token != null) {
                Debug.Log($"Received auth token: {StringManipulation.ObfuscateString(token)}");

                GameManager.instance.StartCoroutine(CheckAccessTokenValidity(token, (isValid) => {
                    if (isValid) {
                        Debug.Log("Access token is valid");
                        authToken = token;
                        Status = AccountStatus.READY;
                    }
                    else {
                        Debug.LogError("Access token is invalid");
                        Status = AccountStatus.ERROR;
                    }
                }));
            }
            else {
                Debug.LogError("Failed to receive auth token");
                Status = AccountStatus.ERROR;
            }
        });
    }
    
    public IEnumerator GenerateAndCheckPin() {
        yield return GeneratePin((id, code) => {
            Debug.Log($"Generated PIN: {code}, ID: {id}");

            Application.OpenURL(GetPinUrl(code));

            GameManager.instance.StartCoroutine(CheckPinAndValidateAccessToken(id, code));
        });
    }
    
    public void UpdateServerList() {
        Debug.Log("Getting plex servers...");
        GameManager.instance.StartCoroutine(GetServers(response => {
            Servers = response.ConvertAll(server => (MediaServer)server);
            Debug.Log("Got " + Servers.Count + " plex servers.");
        }));
    }
}