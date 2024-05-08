using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryStyle : SpawnStyle {
    public override IEnumerator Create(DisplayType displayType, Vector3 position, List<MediaData> mediaData) {
        Quaternion rotation = new Quaternion(0, 0, 0, 0);
        Vector3 xOffset = new Vector3(0, 0, 0);
        List<DisplayType> listOfObjects = new List<DisplayType>();
        foreach (MediaData mediaItem in mediaData) {
            xOffset.x = xOffset.x + 1;
            DisplayType newObject = Instantiate(displayType.gameObject).GetComponent<DisplayType>();
            newObject.transform.position = position + xOffset;
            newObject.transform.rotation = rotation;
            newObject.mediaData = mediaItem;
            listOfObjects.Add(newObject);
        }

        GameManager.instance.StartCoroutine(CamCheck(listOfObjects));
        yield return null;
    }
}