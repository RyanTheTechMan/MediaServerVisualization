using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DVDCase : MonoBehaviour {
    public GameObject caseLeft;
    public GameObject caseRight;
    public Renderer coverRenderer;

    public MediaData mediaData = new MediaData();

    void Start() {
    }

    void Update() {
        coverRenderer.materials[0].mainTexture = mediaData.coverArtTexture; // TODO: not every frame
    }

    public void UpdateVisuals() {
        mediaData.GetCoverArtTexture(this);
    }
}
