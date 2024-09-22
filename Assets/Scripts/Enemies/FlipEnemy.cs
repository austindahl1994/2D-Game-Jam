using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipEnemy : MonoBehaviour
{
    public float flipTime = 1.0f;
    private bool isRotating = false;
    private bool isHiding = false;
    private void Start()
    {
        StartCoroutine(Flip(1));
    }

    public void Flip() {
        if (!isRotating) {
            if (isHiding)
            {
                StartCoroutine(Flip(1));
            }
            else {
                StartCoroutine(Flip(0));
            }
        }
    }

    //if 1, flips it up
    private IEnumerator Flip(int flipped) {
        isRotating = true;
        float elapsed = 0.0f;
        Quaternion startRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        Quaternion endRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        while (elapsed < flipTime) {
            if (flipped == 0)
            {
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / flipTime);
            }
            else {
                //from 90 to 0 rotation, shows/flips up
                transform.rotation = Quaternion.Lerp(endRotation, startRotation, elapsed / flipTime);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (flipped == 0)
        {
            transform.rotation = endRotation;
            isHiding = true;
        }
        else { 
            transform.rotation = startRotation;
            isHiding = false;
        }
        isRotating = false;   
    }
}
