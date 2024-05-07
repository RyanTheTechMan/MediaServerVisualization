using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class DebugUseSceneViewAsCamera : MonoBehaviour {
    public bool updateOutOfPlayMode = false;

    private void Update() {
        if (!Application.isEditor) return;

        if ((!updateOutOfPlayMode || Application.isPlaying) && !Application.isPlaying) return;
        
        transform.position = SceneView.lastActiveSceneView.camera.transform.position;
        transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
    }
}