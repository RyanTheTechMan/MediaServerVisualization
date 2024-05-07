using TMPro;

public class VHS : DisplayType {
    public TMP_Text spineText;
    
    protected override void SetTitleText(string text) {
        base.SetTitleText(text);
        spineText.text = text;
    }
}