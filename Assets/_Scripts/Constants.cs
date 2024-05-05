using UnityEngine;

public class Constants {
    public const string productName = "Media Server Visualizer";
    public static string clientIdentifier {
        get {
            string id = PlayerPrefs.GetString("clientIdentifier", "MSV-" + System.Guid.NewGuid());
            PlayerPrefs.SetString("clientIdentifier", id);
            return id;
        }
    }
}