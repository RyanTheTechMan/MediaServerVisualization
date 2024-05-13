using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnStyle : MonoBehaviour {
    public abstract IEnumerator Create(DisplayType displayType, Vector3 position, List<MediaData> mediaData);
    
    public IEnumerator CamCheck(List<DisplayType> listOfObjects) {
        Transform camTransform = Camera.main.transform;
        while (true) {
            Vector3 camPos = camTransform.position;

            foreach (var obj in listOfObjects) {
                Vector3 objectPos = obj.transform.position;
                float distance = Vector3.Distance(objectPos, camPos);

                if (distance >= 20) {
                    if (!obj.IsFrozen) {
                        obj.Freeze(true);
                    }
                    if (!obj.IsHidden) {
                        obj.Hide(true);
                    }
                }
                else if (distance >= 10) {
                    if (obj.IsHidden) {
                        obj.Hide(false);
                    }
                }
                else {
                    if (obj.IsFrozen || obj.IsHidden) {
                        obj.Freeze(false);
                        obj.Hide(false);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}