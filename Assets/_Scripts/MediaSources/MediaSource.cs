using System;
using UnityEngine;

public class MediaSource {
    public MonoBehaviour monoBehaviour; // TODO: may not be needed
    private string token;

    protected MediaSource(MonoBehaviour monoBehaviour) {
        this.monoBehaviour = monoBehaviour;
    }

    // Returns null if token doesn't exist
    public string GetToken() {
        return token;
    }
}