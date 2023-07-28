using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;

public class Plex : MediaSource {

    private const string productName = "MediaServerVisualizer";
    private string clientIdentifier = "MediaServerVisualizer" + Guid.NewGuid();

    public Plex(MonoBehaviour monoBehaviour) : base(monoBehaviour) {

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
                var response = JsonUtility.FromJson<GetPinResponse>(request.downloadHandler.text);
                callback(response.id, response.code);
            }
            else {
                Debug.LogError("Failed to generate PIN: " + request.error);
            }
        }
    }

    [Serializable]
    public class GetPinResponse
    {
        public string id;
        public string code;
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
            { "context[device][product]", productName },
            { "forwardUrl", "https://your-cool-plex-app.com" }
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
                var response = JsonUtility.FromJson<PinResponse>(request.downloadHandler.text);
                callback(response.authToken);
            }
            else {
                Debug.LogError("Failed to check PIN: " + request.error);
            }
        }
    }

    [Serializable]
    public class PinResponse
    {
        public string authToken;
    }
}