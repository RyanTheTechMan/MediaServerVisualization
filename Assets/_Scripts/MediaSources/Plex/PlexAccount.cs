using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexAccount : MediaAccount {
    [JsonProperty] internal string AuthToken;

    public override void Setup(Action<bool> callback) {
        Status = AccountStatus.WAITING;
        GameManager.instance.StartCoroutine(GenerateAndCheckPin(callback));
    }

    private IEnumerator GetServers(Action<List<PlexServer>> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get("https://plex.tv/api/v2/resources")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", AuthToken);
            request.SetRequestHeader("X-Plex-Product", Constants.productName);
            request.SetRequestHeader("X-Plex-Client-Identifier", Constants.clientIdentifier);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                // Debug.Log(request.downloadHandler.text);

                JArray jDevices = JArray.Parse(request.downloadHandler.text);

                // Filter out non-server devices
                jDevices = new JArray(jDevices.Where(device => device["provides"]?.ToString() == "server"));

                // Create a PlexServer object for each device
                List<PlexServer> servers = (from JObject device in jDevices select new PlexServer(this, device)).ToList();

                // Wait until all servers are ready or errored
                yield return new WaitUntil(() => servers.FindAll(server => server.Status is ServerStatus.READY or ServerStatus.ERROR).Count >= jDevices.Count);

                Debug.Log("All plex servers are ready.");
                callback(servers);
            }
            else {
                Debug.LogError("Failed to get devices: " + request.error);
                callback(new());
            }
        }
    }

    private IEnumerator CheckAccessTokenValidity(string token, Action<bool> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get("https://plex.tv/api/v2/user")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Product", Constants.productName);
            request.SetRequestHeader("X-Plex-Client-Identifier", Constants.clientIdentifier);
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

    private IEnumerator GeneratePin(Action<string, string> callback) {
        WWWForm form = new WWWForm();
        form.AddField("strong", "true");

        using (UnityWebRequest request = UnityWebRequest.Post("https://plex.tv/api/v2/pins", form)) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Product", Constants.productName);
            request.SetRequestHeader("X-Plex-Client-Identifier", Constants.clientIdentifier);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                JObject data = JObject.Parse(request.downloadHandler.text);
                callback(data["id"]?.ToString(), data["code"]?.ToString());
            }
            else {
                Debug.LogError("Failed to generate PIN: " + request.error);
                callback(null, null);
            }
        }
    }

    private string GetPinUrl(string pinCode) {
        var uriBuilder = new UriBuilder("https://app.plex.tv/auth") {
            Fragment = "?"
        };

        var parameters = new NameValueCollection() {
            { "clientID", Constants.clientIdentifier },
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
            request.SetRequestHeader("X-Plex-Client-Identifier", Constants.clientIdentifier);

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
    /// Checks the given PIN and validates the access token. Will retry for 2 minutes before giving up.
    /// </summary>
    /// <param name="pinId">The unique identifier for the PIN.</param>
    /// <param name="pinCode">The PIN code to check.</param>
    /// <param name="maxRetries">The maximum number of times to retry before giving up.</param>
    private IEnumerator CheckPinAndValidateAccessToken(Action<bool> callback, string pinId, string pinCode, uint maxRetries = 20) {
        uint attempts = 0;
        string token = null;

        Status = AccountStatus.WAITING;

        while (attempts < maxRetries && string.IsNullOrEmpty(token)) {
            yield return new WaitForSeconds(6);

            yield return VerifyPinAndRetrieveToken(pinId, pinCode, receivedToken => { token = receivedToken; });

            attempts++;

            if (!string.IsNullOrEmpty(token)) {
                Debug.Log($"Received auth token: {StringManipulation.ObfuscateString(token)}");

                yield return CheckAccessTokenValidity(token, isValid => {
                    if (isValid) {
                        Debug.Log("Access token is valid");
                        AuthToken = token;
                        Status = AccountStatus.READY;
                        callback(true);
                    }
                    else {
                        Debug.LogError("Access token is invalid");
                        Status = AccountStatus.ERROR;
                        callback(false);
                    }
                });
            }
            else if (attempts < maxRetries) {
                Debug.LogWarning("Failed to receive auth token, retrying...");
                token = null; // Ensure token is reset for retry logic
            }
            else {
                Debug.LogError("Failed to receive auth token after several attempts");
                Status = AccountStatus.ERROR;
                callback(false);
            }
        }
    }

    private IEnumerator GenerateAndCheckPin(Action<bool> callback) {
        yield return GeneratePin((id, code) => {
            Debug.Log($"Generated PIN: {code}, ID: {id}");

            Application.OpenURL(GetPinUrl(code));

            GameManager.instance.StartCoroutine(CheckPinAndValidateAccessToken(callback, id, code));
        });
    }

    public override void UpdateInfo(Action<bool> callback) {
        GameManager.instance.StartCoroutine(GetUsername((username) => {
            Username = username;
            callback(true);
        }));
    }

    public IEnumerator GetUsername(Action<string> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get("https://plex.tv/api/v2/user")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", AuthToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                //Debug.Log(request.downloadHandler.text);
                JObject data = JObject.Parse(request.downloadHandler.text);
                string username = (string)data["username"];
                callback(username);
            }
            else {
                Debug.LogError("Web request fail!");
            }
        }
    }

    public override void UpdateServerList(Action<bool> callback) {
        Debug.Log("Getting plex servers...");
        GameManager.instance.StartCoroutine(GetServers(response => {
            Servers = response.ConvertAll(server => (MediaServer)server);
            Debug.Log("Got " + Servers.Count + " plex servers.");
            callback(!response.Any());
        }));
    }
}