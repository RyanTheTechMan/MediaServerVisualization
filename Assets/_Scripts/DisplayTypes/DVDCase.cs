using System.Collections;
using TMPro;
using UnityEngine;

public class DvdType : DisplayTypes {
    public GameObject caseLeft;
    public GameObject caseRight;
    public TMP_Text spineText;

    public override void Start() {
        base.Start();

    }

    protected override void SetTitleText(string text) {
        base.SetTitleText(text);
        spineText.text = text;
    }


    void Update() {

    }

    public override IEnumerator UpdateArt() {
        yield return base.UpdateArt();

        if (mediaData.coverArtTexture != null) {
            mainArtRenderer.materials[0].mainTexture = mediaData.coverArtTexture;
            titleText.gameObject.SetActive(false);
            mainArtRenderer.gameObject.SetActive(true);
        }
        else {
            titleText.gameObject.SetActive(true);
            mainArtRenderer.gameObject.SetActive(false);
        }

        if (mediaData.backgroundArtTexture != null) {
            backgroundArtRenderer.materials[0].mainTexture = mediaData.backgroundArtTexture;
        }
    }


}
