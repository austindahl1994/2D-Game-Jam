using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Vector3 scaleTarget = new (0.35f, 0.35f, 0.35f);
    private readonly float scaleDuration = 1.0f;
    private readonly float gravityScale = 3.3f;
    Rigidbody2D rb;
    CircleCollider2D cc;
    SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        cc.enabled = false;
    }

    public void StartScaling() {
        StartCoroutine(ScaleDown());
    }
    public IEnumerator ScaleDown() { 
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0.0f;

        while (elapsedTime < scaleDuration) {
            transform.localScale = Vector3.Lerp(initialScale, scaleTarget, elapsedTime / scaleDuration);
            if (elapsedTime >= 0.95f) { 
                cc.enabled = true;
                cc.radius = transform.localScale.x + 0.05f;
            }
            elapsedTime += Time.deltaTime;
            rb.gravityScale = elapsedTime * gravityScale;
            yield return null;
        }
        transform.localScale = scaleTarget;
        cc.enabled = false;
        sr.sortingOrder = 0;
        rb.gravityScale = 10;

        yield return new WaitForSeconds(1);

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Target")) {
            //Debug.Log("Hit target!");
            collision.GetComponent<Enemy>().Hit();
        }
    }
}
