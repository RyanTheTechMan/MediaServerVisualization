using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public abstract class MediaAccount {
    [JsonIgnore] internal MediaSource MediaSource;
    [JsonIgnore] internal List<MediaServer> Servers = new();
    [JsonIgnore] public AccountStatus Status = AccountStatus.NOT_CONFIGURED;
    [JsonProperty] public string Username;
    [JsonProperty] public Texture ProfilePicture;

    /// <summary>
    /// Set up the account. Opens the login window, etc.
    /// </summary>
    public virtual void Setup(Action<bool> callback) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Attempts to pull new information from the account.
    /// </summary>
    public virtual void UpdateInfo(Action<bool> callback) {
        throw new NotImplementedException();
    }

    public virtual void UpdateServerList(Action<bool> callback) {
        throw new NotImplementedException();
    }
}

public enum AccountStatus {
    ERROR,
    NOT_CONFIGURED,
    WAITING,
    READY
}

public static class AccountManager {
    public static void SaveAccountsData(List<MediaAccount> accounts) {
        JsonSerializerSettings settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Objects
        };
        string json = JsonConvert.SerializeObject(accounts, settings);
        PlayerPrefs.SetString("AccountsData", json);
        PlayerPrefs.Save();
    }

    public static List<MediaAccount> LoadAccountsData() {
        string json = PlayerPrefs.GetString("AccountsData");
        JsonSerializerSettings settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Objects
        };
        return JsonConvert.DeserializeObject<List<MediaAccount>>(json, settings);
    }
}