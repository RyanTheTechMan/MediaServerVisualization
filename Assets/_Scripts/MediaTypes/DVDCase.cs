using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class DVDCase : MonoBehaviour {
    public GameObject caseLeft;
    public GameObject caseRight;
    public Renderer coverRenderer;
    public TMP_Text coverText;
    public TMP_Text spineText;

    public MediaData mediaData = new MediaData();

    void Start() {
        spineText.text = mediaData.title;
        coverText.text = mediaData.title;
    }

    void Update() {
    }

    public void UpdateArt() {
        // mediaData.GetCoverArtTexture(this);

        if (mediaData.coverArtTexture != null) {
            coverRenderer.materials[0].mainTexture = mediaData.coverArtTexture;
            coverText.gameObject.SetActive(false);
            coverRenderer.gameObject.SetActive(true);
        }
        else {
            coverText.gameObject.SetActive(true);
            coverRenderer.gameObject.SetActive(false);
        }

    }
}
