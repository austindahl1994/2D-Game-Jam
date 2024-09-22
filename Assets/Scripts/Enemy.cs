using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float flipTime = 0.3f;
    private bool isRotating = false;
    private bool isHiding = false;
    private bool needsToReset = true;
    public int respawnTime = 0;
    private void Start()
    {
        //only if mean to come up at the start
        needsToReset = false;
        StartCoroutine(Flip(1));
    }

    public void Hit()
    {
        needsToReset = true;
        TargetFlip();
    }

    public void TargetFlip()
    {
        if (!isRotating)
        {
            if (isHiding)
            {
                //target not shown, should enable collider and flip it up
                EnableColliders();
                StartCoroutine(Flip(1));
            }
            else
            {
                //target was hit, disable colliders and flip it down
                DisableColliders();
                StartCoroutine(Flip(0));
            }
        }
    }

    //if 1, flips it up
    private IEnumerator Flip(int flipped)
    {
        isRotating = true;
        float elapsed = 0.0f;
        Quaternion startRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        Quaternion endRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        while (elapsed < flipTime)
        {
            if (flipped == 0)
            {
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / flipTime);
            }
            else
            {
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
        else
        {
            transform.rotation = startRotation;
            isHiding = false;
        }
        isRotating = false;
        if (respawnTime > 0 && needsToReset) {
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn() {
        yield return new WaitForSeconds(respawnTime);
        needsToReset = false;
        TargetFlip();
    }

    private void DisableColliders()
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();

        if (circleCollider != null) circleCollider.enabled = false;
        if (boxCollider != null) boxCollider.enabled = false;
        if (polygonCollider != null) polygonCollider.enabled = false;
    }

    private void EnableColliders() {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();

        if (circleCollider != null) circleCollider.enabled = true;
        if (boxCollider != null) boxCollider.enabled = true;
        if (polygonCollider != null) polygonCollider.enabled = true;
    }

}
