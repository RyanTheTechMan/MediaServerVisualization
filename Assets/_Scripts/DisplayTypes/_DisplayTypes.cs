using System;
using System.Collections;
using TMPro;
using UnityEngine;

public abstract class DisplayType : MonoBehaviour {
    [NonSerialized] public MediaData mediaData;
    public Renderer mainArtRenderer;
    public Renderer backgroundArtRenderer;
    [SerializeField] protected TMP_Text titleText;
    [SerializeField] protected TMP_Text descriptionText;
    [HideInInspector] public bool IsFrozen;
    [HideInInspector] public bool IsHidden;

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

    public virtual void Freeze(bool frozen) {
        gameObject.GetComponent<Rigidbody>().isKinematic = frozen;
        IsFrozen = frozen;
    }

    public virtual void Hide(bool hidden) {
        gameObject.SetActive(!hidden);
        IsHidden = hidden;
    }
}