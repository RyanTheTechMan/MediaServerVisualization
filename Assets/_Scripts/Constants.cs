using UnityEngine;

public class Constants {
    public const string ProductName = "Media Server Visualizer";
    public static string clientIdentifier {
        get {
            string id = PlayerPrefs.GetString("clientIdentifier", "MSV-" + System.Guid.NewGuid());
            PlayerPrefs.SetString("clientIdentifier", id);
            return id;
        }
    }

    public static readonly string Version = Application.version;
}