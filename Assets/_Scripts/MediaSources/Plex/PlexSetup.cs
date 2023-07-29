using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PlexSetup : MediaDomain {
    public const string productName = "Media Server Visualizer";
    internal readonly string clientIdentifier;

    internal string authToken;

    internal List<PlexServer> plexServers = new();
    internal List<PlexLibrary> plexLibraries => selectedServer.plexLibraries;

    private int selectedServerIndex = -1;
    private int selectedLibraryIndex = -1;

    public PlexServer selectedServer {
        get {
            if (selectedServerIndex < 0 || selectedServerIndex >= plexServers.Count) {
                return null;
            }
            return plexServers[selectedServerIndex];
        }
        set {
            if (value == null) {
                selectedServerIndex = -1;
                return;
            }
            selectedServerIndex = plexServers.IndexOf(value);
            UpdateLibraryList();
        }
    }

    public PlexLibrary selectedLibrary {
        get {
            if (selectedLibraryIndex < 0 || selectedLibraryIndex >= plexLibraries.Count) {
                return null;
            }
            return plexLibraries[selectedLibraryIndex];
        }
        set {
            if (value == null) {
                selectedLibraryIndex = -1;
                return;
            }
            selectedLibraryIndex = plexLibraries.IndexOf(value);
        }
    }

    public PlexSetup(MonoBehaviour monoBehaviour) : base(monoBehaviour) {
        if (PlayerPrefs.HasKey("plexClientIdentifier")) {
            clientIdentifier = PlayerPrefs.GetString("plexClientIdentifier");
        }
        else {
            clientIdentifier = "MSV-" + Guid.NewGuid();
            PlayerPrefs.SetString("plexClientIdentifier", clientIdentifier);
        }
        if (PlayerPrefs.HasKey("plexAuthToken")) {
            authToken = PlayerPrefs.GetString("plexAuthToken");
            monoBehaviour.StartCoroutine(CheckAccessTokenValidity(authToken, (valid) => {
                if (!valid) {
                    Debug.LogWarning("Plex access token is invalid.");
                    authToken = null;
                    monoBehaviour.StartCoroutine(GenerateAndCheckPin());
                }
                else {
                    Debug.Log("Plex access token is valid.");
                    apiReady = true;
                }
            }));
        }
        else {
            monoBehaviour.StartCoroutine(GenerateAndCheckPin());
        }
    }

    public IEnumerator CheckAccessTokenValidity(string userToken, Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get("https://plex.tv/api/v2/user")) {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Product", productName);
            request.SetRequestHeader("X-Plex-Client-Identifier", clientIdentifier);
            request.SetRequestHeader("X-Plex-Token", userToken);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                callback(true);
            }
            else {
                callback(false);
            }
        }
    }

    public IEnumerator GeneratePin(Action<string, string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("strong", "true");

        using (UnityWebRequest request = UnityWebRequest.Post("https://plex.tv/api/v2/pins", form))
        {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Product", productName);
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
    public string GetPinUrl(string pinCode)
    {
        var uriBuilder = new UriBuilder("https://app.plex.tv/auth")
        {
            Fragment = "?"
        };

        var parameters = new NameValueCollection()
        {
            { "clientID", clientIdentifier },
            { "code", pinCode },
            { "context[plexDevice][product]", productName },
            // { "forwardUrl", "https://github.com/RyanTheTechMan/MediaServerVisualization" } // TODO: Add redirect to github "Thank you for signing in page"
        };

        var array = new StringBuilder();
        foreach (string key in parameters.Keys)
        {
            array.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(parameters[key]));
        }

        if (array.Length > 0)
        {
            array.Length--;
        }

        uriBuilder.Fragment += array.ToString();

        return uriBuilder.ToString();
    }

    public IEnumerator CheckPin(string pinId, string pinCode, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"https://plex.tv/api/v2/pins/{pinId}?code={pinCode}"))
        {
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

    public IEnumerator GenerateAndCheckPin() {
        // Generate a pin
        yield return GeneratePin((id, code) => {
            Debug.Log($"Generated PIN: {code}, ID: {id}");

            // Open the authorization URL in a web browser
            Application.OpenURL(GetPinUrl(code));

            // Start a coroutine to check the pin
            monoBehaviour.StartCoroutine(CheckPin(id, code));
        });
    }

    public IEnumerator CheckPin(string pinId, string pinCode) {
        yield return new WaitForSeconds(10);

        yield return CheckPin(pinId, pinCode, (authToken) => {
            if (authToken != null) {
                Debug.Log($"Received auth token: {StringManipulation.ReplaceLastXPercentOfString(authToken, 0.5, '*')}");

                monoBehaviour.StartCoroutine(CheckAccessTokenValidity(authToken, (isValid) => {
                    if (isValid) {
                        Debug.Log("Access token is valid");
                        this.authToken = authToken;
                        apiReady = true;
                        PlayerPrefs.SetString("plexAuthToken", authToken);
                    } else {
                        Debug.LogError("Access token is invalid");
                    }
                }));
            } else {
                Debug.LogError("Failed to receive auth token");
            }
        });
    }

    public IEnumerator GetServers(Action<List<PlexServer>> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get("https://plex.tv/api/v2/resources"))
        {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("X-Plex-Token", authToken);
            request.SetRequestHeader("X-Plex-Product", PlexSetup.productName);
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
            else
            {
                Debug.LogError("Failed to get devices: " + request.error);
            }
        }
    }

    public void UpdateServerList() {
        if (!IsAPIReady()) return;
        Debug.Log("Getting plex servers...");
        monoBehaviour.StartCoroutine(GetServers(response => {
            plexServers = response;
            Debug.Log("Got " + plexServers.Count + " plex servers.");
        }));
    }

    public void UpdateLibraryList() { // This is automatically called when selectedServer is changed.
        if (selectedServer == null) return;
        Debug.Log("Getting plex libraries...");
        monoBehaviour.StartCoroutine(selectedServer.GetLibraries(response => {
            selectedServer.plexLibraries = response;
            Debug.Log("Got " + selectedServer.plexLibraries.Count + " plex libraries.");
        }));
    }

    public void UpdateMediaList() { // This is automatically called when selectedLibrary is changed.
        if (selectedLibrary == null) return;
        Debug.Log("Getting plex media...");
        monoBehaviour.StartCoroutine(selectedLibrary.GetItems(response => {
            mediaItems = response;
            Debug.Log("Got " + mediaItems.Length + " items in library " + selectedLibrary.title + " (" + selectedServer.displayName + ")");
        }));
    }
}