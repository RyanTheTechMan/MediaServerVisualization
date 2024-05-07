using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnStyle : MonoBehaviour {
    public abstract IEnumerator Create(Vector3 position, List<MediaData> mediaData);
}