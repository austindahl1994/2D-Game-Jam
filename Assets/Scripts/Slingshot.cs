using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] private List<Sprite> slingShotSprites = new ();
    [SerializeField] private Transform ballHolder;
    [SerializeField] private GameObject slingString;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject outerTarget;
    [SerializeField] private GameObject innerTarget;
    private GameObject currentBall;
    private Vector3 mouse;
    private bool reloading;
    public float reloadTime = 0.5f;
    public float timeToTarget = 0.9f;

    private bool isHoldingDownMouse = false;
    public float holdStartTime;
    public float holdDuration;
    public float maxHoldTime = 0.5f;

    private Vector2 initialHolder;
    private Vector2 initialStringScale;

    private Coroutine ReloadCoroutine = null;

    private void Start()
    {
        gm = GameManager.Instance;
        initialHolder = ballHolder.localPosition;
        initialStringScale = slingString.transform.localScale;
        CreateBall();
    }
    private void Update()
    {
        MoveSlingshot();

        if (!reloading)
        {
            if (Input.GetMouseButton(0) && !isHoldingDownMouse)
            {
                // If mouse is already down and we haven't started holding, treat it as a new hold
                holdStartTime = Time.time; // Start timing the hold from the current time
                isHoldingDownMouse = true;
            }

            if (Input.GetMouseButtonDown(0))
            {
                holdStartTime = Time.time;
                isHoldingDownMouse = true;
            }

            if (Input.GetMouseButtonUp(0) && isHoldingDownMouse)
            {
                holdDuration = Time.time - holdStartTime;
                isHoldingDownMouse = false;

                if (holdDuration < maxHoldTime)
                {
                    timeToTarget = 4.0f - ((holdDuration / maxHoldTime) * 3.0f);
                }
                else
                {
                    timeToTarget = 0.9f;
                }
                LaunchBall(timeToTarget);
            }

            if (currentBall != null)
            {
                currentBall.transform.position = ballHolder.position;
            }

            if (!isHoldingDownMouse)
            {
                outerTarget.transform.localScale = new Vector3(0.75f, 0.75f, 1);
                innerTarget.GetComponent<SpriteRenderer>().color = Color.gray;
            }
            else
            {
                //counting up every second it is held down
                float h = Time.time - holdStartTime;
                float scaleValue = Mathf.Clamp(.75f - (h / maxHoldTime), 0, .75f);
                outerTarget.transform.localScale = new Vector2(scaleValue, scaleValue);
                //y position of holder between 0.5 and 0
                ballHolder.localPosition = new Vector2(ballHolder.localPosition.x, Mathf.Clamp(0.5f - h, 0, 0.5f));
                //slingString scale y between 0.5 and 1
                slingString.transform.localScale = new Vector2(initialStringScale.x, Mathf.Clamp(h + 0.5f, 0.5f, 1));
                if (scaleValue <= 0)
                {
                    innerTarget.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }

    private void MoveSlingshot() {
        mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float tenPercentWidth = gm.ScreenSize.x * 0.1f;
        float clampedX = Mathf.Clamp(mouse.x, -gm.ScreenSize.x / 2 + tenPercentWidth, gm.ScreenSize.x / 2 - tenPercentWidth);
        transform.position = new Vector3(clampedX, -gm.ScreenSize.y / 2.4f, 0);
    }

    private void CreateBall()
    {
        if (ReloadCoroutine != null) { 
            StopCoroutine(ReloadCoroutine);
            ReloadCoroutine = null;
        }
        currentBall = Instantiate(ballPrefab, ballHolder.position, Quaternion.identity);
        currentBall.GetComponent<Rigidbody2D>().isKinematic = true; // Disable physics until launched
        reloading = false;
    }

    private void LaunchBall(float force)
    {
        ReloadCoroutine = StartCoroutine(Reload());
        Rigidbody2D rb = currentBall.GetComponent<Rigidbody2D>();
        currentBall.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        currentBall.GetComponent<SpriteRenderer>().sortingOrder = 25;
        currentBall.GetComponent<Ball>().StartScaling();
        rb.isKinematic = false;
        rb.transform.position = ballHolder.position;
        Vector2 targetPosition = GetComponentInChildren<Target>().TargetPosition;
        Vector2 launchPosition = ballHolder.position;
        Vector2 direction = targetPosition - launchPosition;
        float gravity = Physics2D.gravity.y; 
        float verticalVelocity = (direction.y - (0.5f * gravity * force)) / force;
        float horizontalVelocity = direction.x / force;
        rb.velocity = new Vector2(horizontalVelocity, verticalVelocity);
        currentBall = null;
        //CreateBall();
    }

    
    private IEnumerator Reload() {
        reloading = true;
        ballHolder.localPosition = initialHolder;
        slingString.transform.localScale = initialStringScale;
        yield return new WaitForSeconds(reloadTime);
        CreateBall();
    }

    public void SetSlingshot(int color) {
        GetComponent<SpriteRenderer>().sprite = slingShotSprites[color];
    }
}
