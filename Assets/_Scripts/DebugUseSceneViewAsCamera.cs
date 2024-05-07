using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

[ExecuteInEditMode]
public class DebugUseSceneViewAsCamera : MonoBehaviour {
    public bool updateOutOfPlayMode = false;

    void OnEnable() {
    #if UNITY_EDITOR
        EditorApplication.update += EditorUpdate;
    #endif
    }

    void OnDisable() {
    #if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
    #endif
    }

    void EditorUpdate() {
        UpdateCameraPosition();
    }

    private void Update() {
        if (Application.isPlaying) {
            UpdateCameraPosition();
        }
    }

    private void UpdateCameraPosition() {
        if (!Application.isEditor || SceneView.lastActiveSceneView == null) return;

        if ((!updateOutOfPlayMode || Application.isPlaying) && !Application.isPlaying) return;

        transform.position = SceneView.lastActiveSceneView.camera.transform.position;
        transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
    }
}