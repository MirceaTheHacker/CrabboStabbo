using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static IEnumerator FadeGameObject(GameObject obj, float lerpDuration)
    {
        SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer> (); 
        Color startLerp = spriteRenderer.material.color;
        Color targetLerp = startLerp;
        targetLerp.a = 0;
        float lerpStart_Time = Time.time;
        float lerpProgress;
        bool lerping = true;
        while (lerping) {
            yield return new WaitForEndOfFrame();
            lerpProgress = Time.time - lerpStart_Time;
            if (spriteRenderer != null) {
                foreach(SpriteRenderer sprtRenderer in obj.GetComponentsInChildren<SpriteRenderer> ()) {
                    sprtRenderer.material.color = Color.Lerp(startLerp, targetLerp, lerpProgress / lerpDuration);
                }
            }
            else {
                lerping = false;
            }
         
            if (lerpProgress >= lerpDuration) {
                lerping = false;
            }
        }
     // when reaching this point the gameObject is invisible (alpha = 0) and it has no colliders so there is no point in keeping it and it's components in memory
     Destroy(obj);
    }
}
