using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySO ai;
    public float flipTime = 0.3f;
    public int respawnTime;
    private int moveDirection;
    private float speed;

    private bool isRotating = false;
    private bool isHiding = false;
    private bool needsToReset;
    private float initialHeight;

    private Coroutine MoveCoroutine = null;
    private Coroutine RespawnCoroutine = null; //respawn after time, or if hitting the repeat, can cancel and just flip
    private void Start()
    {
        initialHeight = transform.position.y;
        speed = ai.movementSpeed;
        respawnTime = ai.respawnTime;
        moveDirection = ai.moveDirectionRight ? 1 : -1;
        needsToReset = respawnTime != 0;
        if (ai.movementSpeed > 0) {
            MoveCoroutine = StartCoroutine(Move());
        }
        if (ai.startFacingUp)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        else { 
            StartCoroutine(Flip(true));
        }
    }

    public void Hit()
    {
        if (respawnTime > 0) { 
            RespawnCoroutine = StartCoroutine(Respawn());
        }
        TargetFlip();
    }

    //Called when the target is hit
    public void TargetFlip()
    {
        if (!isRotating)
        {
            if (isHiding)
            {
                //target not shown, should enable collider and flip it up
                EnableColliders();
                StartCoroutine(Flip(true));
            }
            else
            {
                //target was hit, disable colliders and flip it down
                DisableColliders();
                StartCoroutine(Flip(false));
            }
        }
    }

    //if 1, flips it up
    private IEnumerator Flip(bool flippedDown)
    {
        isRotating = true;
        float elapsed = 0.0f;
        Quaternion startRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        Quaternion endRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        while (elapsed < flipTime)
        {
            //if not flipped down, flip it down
            if (!flippedDown)
            {
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / flipTime);
            }
            else
            {
                //from 90 to 0 rotation, flips up if down
                transform.rotation = Quaternion.Lerp(endRotation, startRotation, elapsed / flipTime);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        //finalizing flip, ensuring it is 0 or 90 rotation, sets if it is hiding (not shown) or not
        if (!flippedDown)
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
    }
    //only needs to respawn after timer
    private IEnumerator Respawn() {
        yield return new WaitForSeconds(respawnTime);
        TargetFlip();
    }
    //Add a cycle counter, how many times it's bounced/repeated
    //Does the target have a respawn? Then respawn it if repeating
    private IEnumerator Move()
    {
        //check to see if which is right and left bound
        while (true)
        {
            //moves target left or right based on moveDir, which is 1 or -1
            transform.position += new Vector3(moveDirection * speed * Time.deltaTime, 0, 0);
            //if target tries going further than right bound
            if (transform.position.x >= ai.rightBound.x)
            {
                //If it needs to repeat, move it back over to other side
                if (ai.repeat)
                {
                    transform.SetPositionAndRotation(ai.leftBound, Quaternion.Euler(needsToReset ? 0 : 90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
                    if (RespawnCoroutine != null) { 
                        StopCoroutine(RespawnCoroutine);
                    }
                    if (needsToReset)
                    {
                        EnableColliders();
                    }
                    else
                    {
                        DisableColliders();
                    }
                }
                else if (ai.bounce)
                {
                    //also flip direction facing
                    moveDirection *= -1;
                }
                //set the game object to not active since done after moving
                else
                { 
                    gameObject.SetActive(false);
                }
            // if target is trying to go past left bound
            } else if (transform.position.x <= ai.leftBound.x) {
                //If it needs to repeat, move it back over to other side
                if (ai.repeat)
                {
                    //if needing to reset, flips it back over and enables collider
                    transform.SetPositionAndRotation(ai.rightBound, Quaternion.Euler(needsToReset ? 0 : 90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
                    if (RespawnCoroutine != null)
                    {
                        StopCoroutine(RespawnCoroutine);
                    }
                    if (needsToReset) {
                        EnableColliders();
                    } else { 
                        DisableColliders();
                    }
                }
                //if it needs to bounce, will change direction and flip it around
                else if (ai.bounce)
                {
                    //also flip direction facing
                    moveDirection *= -1;
                }
                //set the game object to not active since done after moving
                else
                {
                    gameObject.SetActive(false);
                }
            }
            if (ai.waveMotion) {
                Debug.Log("Waving enemy");
            }

            yield return null; // Wait for the next frame
        }
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
