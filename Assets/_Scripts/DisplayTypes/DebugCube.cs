using System;
using UnityEngine;

public class DebugCube : DisplayType {
    public Material green;
    public Material yellow;
    public Material red;
    private Renderer _renderer;

    private void Awake() {
        _renderer = GetComponent<Renderer>();
    }

    public override void Freeze(bool frozen) {
        if (frozen) {
            _renderer.material = yellow;
        }
        else {
            _renderer.material = green;
        }
        base.Freeze(frozen);
    }

    public override void Hide(bool hidden) {
        if (hidden) {
            _renderer.material = red;
        }
        else Freeze(IsFrozen);
        IsHidden = hidden;
    }
}