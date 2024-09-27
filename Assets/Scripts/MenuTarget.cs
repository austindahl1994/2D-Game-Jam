using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTarget : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (gameObject.CompareTag("play")) 
            {
                Debug.Log("Play hit");
            } 
            else if (gameObject.CompareTag("red")) 
            {
                GameManager.Instance.SetSlingshotColor(0);
            }
            else if (gameObject.CompareTag("blue"))
            {
                GameManager.Instance.SetSlingshotColor(1);
            }
            else if (gameObject.CompareTag("green"))
            {
                GameManager.Instance.SetSlingshotColor(2);
            }
            else if (gameObject.CompareTag("exit"))
            {
                Debug.Log("Exit hit");
            }
        }
    }
    private IEnumerator Flip(bool flippedDown)
    {
        float elapsed = 0.0f;
        float flipTime = 0.3f;
        Quaternion startRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        Quaternion endRotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        while (elapsed < flipTime)
        {
            //if flipped down, flip it up 
            if (flippedDown)
            {
                transform.rotation = Quaternion.Lerp(endRotation, startRotation, elapsed / flipTime);
            }
            else
            {
                //If its flipped up, flip it down
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsed / flipTime);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        //Now opposite of what it was starting
        flippedDown = !flippedDown;
        if (flippedDown)
        {
            gameObject.GetComponent<CircleCollider2D>().enabled = false;
        }
        else
        {
            gameObject.GetComponent<CircleCollider2D>().enabled = true;
        }

        //finalizing flip, ensuring it is 0 or 90 rotation, sets if it is hiding (not shown) or not
        if (flippedDown)
        {
            transform.rotation = endRotation;
        }
        else
        {
            transform.rotation = startRotation;
        }
    }
}
