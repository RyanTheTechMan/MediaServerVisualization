using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StackStyle : SpawnStyle {
    public override IEnumerator Create(DisplayType displayType, Vector3 position, List<MediaData> mediaData) {
        BoxCollider collider = displayType.Hitbox;
        List<DisplayType> listOfObjects = new List<DisplayType>();
        if (collider == null) {
            Debug.LogError("Prefab must have a BoxCollider component.");
            yield break;
        }

        Vector3 colliderSize = collider.size;
        colliderSize.Scale(displayType.transform.localScale);

        Vector3 currentPosition = position;
        foreach (MediaData data in mediaData) {
            GameObject newItem = Instantiate(displayType.gameObject, currentPosition, Quaternion.identity);
            newItem.GetComponent<Rigidbody>().isKinematic = true;
            
            DisplayType dType = newItem.GetComponent<DisplayType>();
            dType.mediaData = data;

            currentPosition += new Vector3(0, colliderSize.y, 0);

            listOfObjects.Add(dType);
            yield return null;
        }
        GameManager.instance.StartCoroutine(CamCheck(listOfObjects));
        yield return null;
    }
}