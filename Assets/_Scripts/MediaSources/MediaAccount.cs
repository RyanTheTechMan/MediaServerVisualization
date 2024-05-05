using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class MediaAccount {
    internal MediaSource MediaSource;
    internal List<MediaServer> Servers = new();

    public AccountStatus Status = AccountStatus.NOT_CONFIGURED;

    public string Username;
    public Texture ProfilePicture;

    /// <summary>
    /// Set up the account. Opens the login window, etc.
    /// </summary>
    public virtual void Setup(Action<bool> callback) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Attempts to pull new information from the account.
    /// </summary>
    public virtual void UpdateInfo() {
        throw new NotImplementedException();
    }
    
    public virtual void UpdateServerList(Action<bool> callback) {
        throw new NotImplementedException();
    }
}

public static class AccountManager {
    public static void SaveAccountData(MediaAccount data, string filePath) {
        string json = JsonUtility.ToJson(data);
        string encryptedJson = SecurePlayerPrefs.EncryptString(json);
        File.WriteAllText(filePath, encryptedJson);
    }

    public static MediaAccount LoadAccountData(string filePath) {
        if (!File.Exists(filePath)) return null;
        string encryptedJson = File.ReadAllText(filePath);
        string json = SecurePlayerPrefs.DecryptString(encryptedJson);
        return JsonUtility.FromJson<MediaAccount>(json);
    }
}

public enum AccountStatus {
    ERROR,
    NOT_CONFIGURED,
    WAITING,
    READY
}