using System.Collections;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] private Transform ballHolder;
    [SerializeField] private Transform activeBallHolder;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject secondTarget;
    private SpriteRenderer sling;
    private SpriteRenderer activeSling;
    private GameObject currentBall;
    private Vector3 mouse;
    //private bool reloading;
    public float reloadTime = 0.5f;
    public float timeToTarget = 0.9f;

    private bool isHoldingDownMouse = false;
    public float holdStartTime;
    public float holdDuration;
    public float maxHoldTime = 0.5f;


    private void Start()
    {
        gm = GameManager.Instance;
        sling = gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
        activeSling = gameObject.transform.GetChild(2).GetComponent<SpriteRenderer>();
        CreateBall();
    }
    private void Update()
    {
        MoveSlingshot();
        if (Input.GetMouseButtonDown(0))
        {
            holdStartTime = Time.time;
            isHoldingDownMouse = true;
        }

        if (Input.GetMouseButtonUp(0) && isHoldingDownMouse) {
            holdDuration = Time.time - holdStartTime;
            isHoldingDownMouse = false;

            if (holdDuration < maxHoldTime)
            {
                timeToTarget = 4.0f - ((holdDuration / maxHoldTime) * 3.0f);
            }
            else {
                timeToTarget = 0.9f;
            }
            LaunchBall(timeToTarget);
        }
        if (!isHoldingDownMouse)
        {
            currentBall.transform.position = ballHolder.position;
            sling.enabled = true;
            activeSling.enabled = false;
            secondTarget.transform.localScale = Vector3.zero;
        }
        else {
            currentBall.transform.position = activeBallHolder.position;
            sling.enabled = false;
            activeSling.enabled = true;
            float h = Time.time - holdStartTime;
            float scaleValue = Mathf.Clamp(h / maxHoldTime, 0, 1) * 0.5f;
            secondTarget.transform.localScale = new Vector2(scaleValue, scaleValue);
        }
    }

    private void MoveSlingshot() {
        mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float tenPercentWidth = gm.ScreenSize.x * 0.1f;
        float clampedX = Mathf.Clamp(mouse.x, -gm.ScreenSize.x / 2 + tenPercentWidth, gm.ScreenSize.x / 2 - tenPercentWidth);
        transform.position = new Vector3(clampedX, -gm.ScreenSize.y / 2.2f, 0);
    }

    private void CreateBall()
    {
        //reloading = false;
        currentBall = Instantiate(ballPrefab, ballHolder.position, Quaternion.identity);
        currentBall.GetComponent<Rigidbody2D>().isKinematic = true; // Disable physics until launched
    }

    private void LaunchBall(float force)
    {
        //StartCoroutine(Reload());
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
        CreateBall();
    }
    //no need for a reload delay?
    /*
    private IEnumerator Reload() {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        CreateBall();
    }*/
}
