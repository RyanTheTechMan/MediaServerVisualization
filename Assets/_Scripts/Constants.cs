using UnityEngine;

public class Constants {
    public const string productName = "Media Server Visualizer";
    public string clientIdentifier => PlayerPrefs.GetString("clientIdentifier", "MSV-" + System.Guid.NewGuid().ToString());

    // Current structure:
    // Media Source (PlexSource, JellyfinSource, etc.) - More or less just for display. Name, icon, banner.
    // Game Manager - Holds list of Accounts
    //     Media Account (PlexAccount, JellyfinAccount, etc.)
    //         Media Server (PlexServer, JellyfinServer, etc.)
    //             Media Library (PlexLibrary, JellyfinLibrary, etc.)
    //                 Media Data (PlexMediaData, JellyfinMediaData, etc.)
    
    // For example, Plex:
    // PlexSource
    //  List of PlexAccount
    //      List of PlexServer
    //          List of PlexLibrary
    //              List of PlexMediaData
    
    // The Accounts have Servers in them.
    // A Server has Libraries in it.
    // A Library has Media Data in it.
    // And the Media Data is the actual information about the media.
}