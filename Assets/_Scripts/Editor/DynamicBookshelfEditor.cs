using UnityEditor;

[CustomEditor(typeof(DynamicBookshelf)), CanEditMultipleObjects]
public class DynamicBookshelfEditor : Editor {
    public void OnSceneGUI() {
        DynamicBookshelf bookshelf = (DynamicBookshelf)target;
        if (!bookshelf.updateBookshelf) return;
        bookshelf.ForceUpdate();
        bookshelf.updateBookshelf = false;
    }
}