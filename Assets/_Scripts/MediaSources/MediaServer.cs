using System;
using System.Collections.Generic;

public abstract class MediaServer {
    internal MediaAccount Account;
    internal List<MediaLibrary> Libraries = new();
    
    public virtual string name { get; protected set; } = "N/A";
    
    public ServerStatus Status = ServerStatus.NOT_CONFIGURED;
    
    public virtual void UpdateLibraryList(Action<bool> callback) {
        throw new System.NotImplementedException();
    }
}

public enum ServerStatus {
    ERROR,
    NOT_CONFIGURED,
    WAITING,
    READY
}