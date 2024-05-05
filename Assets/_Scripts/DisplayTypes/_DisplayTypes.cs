using System;
using System.Collections;
using TMPro;
using UnityEngine;

public abstract class DisplayTypes : MonoBehaviour {
    [NonSerialized] public MediaData mediaData;
    public Renderer mainArtRenderer;
    public Renderer backgroundArtRenderer;
    [SerializeField] protected TMP_Text titleText;
    [SerializeField] protected TMP_Text descriptionText;

    public virtual void Start() {
        SetTitleText(mediaData.title);
        SetDescriptionText(mediaData.description);
    }

    protected virtual void SetTitleText(string text) {
        if (titleText != null) titleText.text = text;
    }

    protected virtual void SetDescriptionText(string text) {
        if (descriptionText != null) descriptionText.text = text;
    }

    public virtual IEnumerator UpdateArt() {
        Coroutine mainArtCoroutine = StartCoroutine(mediaData.UpdateMainArtTexture());
        Coroutine backgroundArtCoroutine = StartCoroutine(mediaData.UpdateBackgroundArtTexture());

        yield return mainArtCoroutine;
        yield return backgroundArtCoroutine;

        Debug.Log("Done with UpdateArt() for " + mediaData.title);
    }
}