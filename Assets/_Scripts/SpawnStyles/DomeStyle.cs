using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DomeStyle : SpawnStyle {
    // public override IEnumerator Create(Vector3 location, List<MediaData> mediaData) {
    //     GameObject prefab = GameManager.instance.displayTypes[0].gameObject;
    //     BoxCollider collider = prefab.GetComponent<BoxCollider>();
    //     if (collider == null) {
    //         Debug.LogError("Prefab must have a BoxCollider component.");
    //         yield break;
    //     }
    //
    //     Vector3 size = collider.size;
    //     float maxDimension = Mathf.Max(size.x, size.y, size.z);
    //     int layers = Mathf.CeilToInt(mediaData.Count / (maxDimension * maxDimension));
    //
    //     int index = 0;
    //     for (int layer = 0; layer < layers; layer++) {
    //         int objectsInLayer = Mathf.Min(mediaData.Count - index, Mathf.FloorToInt(maxDimension * maxDimension));
    //         int rows = Mathf.CeilToInt(Mathf.Sqrt(objectsInLayer));
    //         int cols = Mathf.CeilToInt((float)objectsInLayer / rows);
    //
    //         Vector3 layerCenter = location + Vector3.up * (layer * maxDimension);
    //         Vector3 startPos = layerCenter - new Vector3((cols - 1) * size.x, 0, (rows - 1) * size.z) * 0.5f;
    //
    //         for (int row = 0; row < rows; row++) {
    //             for (int col = 0; col < cols; col++) {
    //                 if (index >= mediaData.Count)
    //                     yield break;
    //
    //                 Vector3 pos = startPos + new Vector3(col * size.x, 0, row * size.z);
    //                 GameObject obj = Object.Instantiate(prefab, pos, Quaternion.Euler(0, 90, 0));
    //                 DisplayType displayType = obj.GetComponent<DisplayType>();
    //                 displayType.mediaData = mediaData[index];
    //                 index++;
    //             }
    //         }
    //     }
    //
    //     yield return null;
    // }

// public override IEnumerator Create(Vector3 location, List<MediaData> mediaData) {
//     float maxRadius = 12.0f;
//     
//     GameObject prefab = GameManager.instance.displayTypes[0].gameObject;
//     BoxCollider collider = prefab.GetComponent<BoxCollider>();
//         if (!collider)
//         {
//             Debug.LogError("Prefab does not contain a BoxCollider.");
//             yield break;
//         }
//
//         Vector3 size = collider.size;
//         size.Scale(prefab.transform.localScale);
//         float width = Mathf.Max(size.x, size.z); // Horizontal dimension
//         float height = size.y; // Vertical dimension
//
//         int numberOfObjects = mediaData.Count;
//         float currentHeight = 0;
//         int objectIndex = 0;
//
//         while (objectIndex < numberOfObjects)
//         {
//             float radiusAtCurrentHeight = Mathf.Sqrt(maxRadius * maxRadius - currentHeight * currentHeight);
//             float circumference = 2 * Mathf.PI * radiusAtCurrentHeight;
//             int objectsInLayer = Mathf.FloorToInt(circumference / width);
//             objectsInLayer = Mathf.Min(objectsInLayer, numberOfObjects - objectIndex);
//
//             float angleStep = 360.0f / objectsInLayer;
//
//             for (int i = 0; i < objectsInLayer; i++)
//             {
//                 float angle = i * angleStep * Mathf.Deg2Rad;
//                 Vector3 position = new Vector3(
//                     location.x + radiusAtCurrentHeight * Mathf.Cos(angle),
//                     location.y + currentHeight,
//                     location.z + radiusAtCurrentHeight * Mathf.Sin(angle)
//                 );
//
//                 Quaternion rotation = Quaternion.Euler(0, -i * angleStep, 0);
//                 GameObject obj = Instantiate(prefab, position, rotation);
//                 obj.GetComponent<Rigidbody>().isKinematic = true;
//                 if (mediaData[objectIndex] != null) // Assuming there is an Assign method or similar
//                 {
//                     DisplayType displayType = obj.GetComponent<DisplayType>();
//                     displayType.mediaData = mediaData[objectIndex];
//                 }
//
//                 objectIndex++;
//                 if (objectIndex >= numberOfObjects) break;
//             }
//
//             currentHeight += height;
//             if (currentHeight >= maxRadius || radiusAtCurrentHeight <= width) break; // Stop if the height exceeds the radius or the layer cannot fit any more objects
//
//             yield return null;
//         }
//
//         yield return null;
//     }
    public override IEnumerator Create(DisplayType displayType, Vector3 center, List<MediaData> mediaData) {
        float maxRadius = 12.0f;

        GameObject prefab = displayType.gameObject;
        BoxCollider collider = prefab.GetComponent<BoxCollider>();
        if (collider == null) {
            Debug.LogError("Prefab does not have a BoxCollider!");
            yield break;
        }

        Vector3 size = collider.size;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(0, 0, 1)); // Rotate prefab to lay flat

        float maxDimension = Mathf.Max(size.x, size.z);
        int numberOfLayers = Mathf.CeilToInt(maxRadius / maxDimension);
        float layerHeight = size.y;

        int mediaIndex = 0;

        for (int i = 0; i < numberOfLayers && mediaIndex < mediaData.Count; i++) {
            float currentRadius = maxRadius * (numberOfLayers - i) / numberOfLayers;
            int objectsInLayer = Mathf.FloorToInt(2 * Mathf.PI * currentRadius / maxDimension);
            float angleStep = 360f / objectsInLayer;

            for (int j = 0; j < objectsInLayer && mediaIndex < mediaData.Count; j++) {
                float angle = j * angleStep;
                Vector3 localPos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)) * currentRadius;
                Vector3 spawnPosition = center + new Vector3(localPos.x, i * layerHeight, localPos.z);
                GameObject newObject = Instantiate(prefab, spawnPosition, rotation);
                newObject.GetComponent<Rigidbody>().isKinematic = true;
                newObject.GetComponent<DisplayType>().mediaData = mediaData[mediaIndex];

                mediaIndex++;
                yield return new WaitForNextFrameUnit();
            }
        }
    }
}