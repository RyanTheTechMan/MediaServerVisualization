using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PileStyle : SpawnStyle {
    public override IEnumerator Create(DisplayType displayType, Vector3 position, List<MediaData> mediaData) {
        const int spawnGroupSize = 10;
        int groupSize = 0;
        List<DisplayType> mediaObjects = new();

        foreach (MediaData mediaItem in mediaData) {
            Vector3 posOffset = new Vector3(Random.Range(-8f, 8f), Random.Range(0f, 5f), Random.Range(-8f, 8f));
            Vector3 velOffset = new Vector3(Random.Range(-3f, 3f), Random.Range(0f, 3f), Random.Range(-3f, 3f));

            DisplayType newObject = Instantiate(displayType.gameObject).GetComponent<DisplayType>(); // TODO: DisplayType is hardcoded here
            newObject.transform.position = position + posOffset;
            newObject.GetComponent<Rigidbody>().linearVelocity = velOffset;

            newObject.mediaData = mediaItem;
            mediaObjects.Add(newObject);

            if (groupSize >= spawnGroupSize) {
                groupSize = 0;
                yield return new WaitForSeconds(1f / 100f);
            }
            groupSize++;
        }
        GameManager.instance.StartCoroutine(CamCheck(mediaObjects));
        
        // TODO: This should be done as you get close to the art in a queued fashion. Not all at once, let alone, sequentially.
        // foreach (DisplayType mediaObject in mediaObjects) {
        //     mediaObject.UpdateArt();
        //     yield return new WaitForSeconds(0.5f);
        // }
    }
}