using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediaDomain {
    public MonoBehaviour monoBehaviour; // TODO: may not be needed
    public MediaData[] mediaItems = Array.Empty<MediaData>(); // TODO: Should be null until items were found
    protected bool apiReady = false; // When true, functions that return data can be called

    protected MediaDomain(MonoBehaviour monoBehaviour) {
        this.monoBehaviour = monoBehaviour;
    }

    // Returns null if token doesn't exist
    public bool IsAPIReady() {
        return apiReady;
    }
}