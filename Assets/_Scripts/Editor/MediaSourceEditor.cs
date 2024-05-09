using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.VectorGraphics;
using System.Collections.Generic;

[CustomEditor(typeof(MediaSource))]
public class MediaSourceEditor : Editor {
    SerializedProperty nameProp;
    SerializedProperty supportsSVGProp;
    SerializedProperty iconSVGProp;
    SerializedProperty bannerSVGProp;
    SerializedProperty iconImageProp;
    SerializedProperty bannerImageProp;

    private void OnEnable() {
        nameProp = serializedObject.FindProperty("name");
        supportsSVGProp = serializedObject.FindProperty("supportsSVG");
        iconSVGProp = serializedObject.FindProperty("iconSVG");
        bannerSVGProp = serializedObject.FindProperty("bannerSVG");
        iconImageProp = serializedObject.FindProperty("iconImage");
        bannerImageProp = serializedObject.FindProperty("bannerImage");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(nameProp);
        EditorGUILayout.PropertyField(supportsSVGProp);
        bool supportsSVG = supportsSVGProp.boolValue;

        if (supportsSVG) {
            EditorGUILayout.PropertyField(iconSVGProp, new GUIContent("Icon SVG"));
            EditorGUILayout.PropertyField(bannerSVGProp, new GUIContent("Banner SVG"));

            DisplaySVGWithMaterial(iconSVGProp);
            DisplaySVGWithMaterial(bannerSVGProp);
        }
        else {
            EditorGUILayout.PropertyField(iconImageProp, new GUIContent("Icon Image"));
            EditorGUILayout.PropertyField(bannerImageProp, new GUIContent("Banner Image"));

            DisplayImagePreview(iconImageProp);
            DisplayImagePreview(bannerImageProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private GameObject tempGO;
    private RenderTexture rt;
    private Camera tempCamera;

    private void OnDisable() {
        if (rt != null) DestroyImmediate(rt);
        if (tempGO != null) DestroyImmediate(tempGO);
        if (tempCamera != null) DestroyImmediate(tempCamera.gameObject);
    }

    private void DisplaySVGWithMaterial(SerializedProperty svgProp) {
        SVGImage svgImage = svgProp.objectReferenceValue as SVGImage;
        if (svgImage != null && svgImage.sprite != null) {
            if (rt == null) {
                rt = new RenderTexture(100, 100, 32);
                RenderTexture.active = rt;
            }

            // Ensure GameObjects and components are initialized
            InitializePreviewEnvironment();

            // Update properties of SVGImage if already exists
            SVGImage tempSVGImage = tempGO.GetComponentInChildren<SVGImage>();
            if (tempSVGImage != null) {
                tempSVGImage.sprite = svgImage.sprite;
                tempSVGImage.material = svgImage.material;
                tempSVGImage.preserveAspect = svgImage.preserveAspect;
            }

            tempCamera.Render();

            // Display the preview with dynamic sizing
            EditorGUILayout.LabelField("Preview:");
            float width = EditorGUIUtility.currentViewWidth;
            GUILayoutOption[] options = { GUILayout.Width(width), GUILayout.Height(width * rt.height / rt.width) };
            GUILayout.Label(rt, options);
            
            RenderTexture.active = null;
            
            if (tempGO != null) DestroyImmediate(tempGO);
            if (tempCamera != null) DestroyImmediate(tempCamera.gameObject);
        }
    }

    private void InitializePreviewEnvironment() {
        if (tempGO == null) {
            // Create the camera GameObject
            tempGO = new GameObject("SVGCanvasCamera");
            tempCamera = tempGO.AddComponent<Camera>();
            tempCamera.backgroundColor = Color.clear;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.orthographic = true; // Assuming you want a flat UI look
            tempCamera.orthographicSize = 50; // Adjust size as needed
            tempCamera.targetTexture = rt;

            // Create a Canvas GameObject as a child of the camera
            GameObject canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(tempGO.transform, false);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = tempCamera;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

            // Create SVGImage as a child of the canvas
            GameObject svgImageGO = new GameObject("SVGImage");
            svgImageGO.transform.SetParent(canvasGO.transform, false);
            SVGImage tempSVGImage = svgImageGO.AddComponent<SVGImage>();
            tempSVGImage.rectTransform.sizeDelta = new Vector2(100, 100);
        }
    }

    private void DisplayImagePreview(SerializedProperty imageProp) {
        Image image = imageProp.objectReferenceValue as Image;
        if (image != null && image.sprite != null) {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(100), GUILayout.Height(100));
            EditorGUI.DrawPreviewTexture(r, image.sprite.texture, null, ScaleMode.ScaleToFit, 0, 0);
        }
    }
}