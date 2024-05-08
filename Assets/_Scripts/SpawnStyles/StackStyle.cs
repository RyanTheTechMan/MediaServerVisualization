using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StackStyle : SpawnStyle {
    public override IEnumerator Create(DisplayType displayType, Vector3 position, List<MediaData> mediaData) {
        List<DisplayType> listOfObjects = new List<DisplayType>();
        BoxCollider collider = displayType.GetComponent<BoxCollider>();
        if (collider == null) {
            Debug.LogError("Prefab must have a BoxCollider component.");
            yield break;
        }

        Vector3 colliderSize = collider.size;

        Vector3 currentPosition = position;
        foreach (MediaData data in mediaData) {
            GameObject newItem = Instantiate(displayType.gameObject, currentPosition, Quaternion.identity);
            newItem.GetComponent<Rigidbody>().isKinematic = true;
            
            DisplayType dType = newItem.GetComponent<DisplayType>();
            dType.mediaData = data;

            float maxSide = Mathf.Max(colliderSize.x, colliderSize.y, colliderSize.z);

            Quaternion rotation;
            Vector3 adjustedColliderSize = colliderSize;
            if (maxSide == colliderSize.x) {
                rotation = Quaternion.Euler(0, 0, -90);
                adjustedColliderSize = new Vector3(colliderSize.y, colliderSize.z, colliderSize.x);
            }
            else if (maxSide == colliderSize.y) {
                rotation = Quaternion.Euler(-90, 0, 0);
                adjustedColliderSize = new Vector3(colliderSize.x, colliderSize.z, colliderSize.y);
            }
            else {
                rotation = Quaternion.Euler(0, 90, 0);
                adjustedColliderSize = new Vector3(colliderSize.z, colliderSize.y, colliderSize.x);
            }

            newItem.transform.rotation = rotation;

            currentPosition += new Vector3(0, adjustedColliderSize.y * displayType.transform.localScale.y * 2, 0);
            listOfObjects.Add(dType);

            yield return null;
        }
        GameManager.instance.StartCoroutine(CamCheck(listOfObjects));
        yield return null;
    }
}