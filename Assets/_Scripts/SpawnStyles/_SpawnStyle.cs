using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnStyle : MonoBehaviour {
    public abstract IEnumerator Create(Vector3 center, List<MediaData> mediaData);
}