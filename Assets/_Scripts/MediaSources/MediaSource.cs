using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class MediaSource : MonoBehaviour {
    public new string name;

    public bool supportsSVG = true;
    
    public SVGImage iconSVG;
    public SVGImage bannerSVG;
    
    public Image iconImage;
    public Image bannerImage;
}