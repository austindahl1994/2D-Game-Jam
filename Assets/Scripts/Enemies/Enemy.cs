using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemySO ai;
    [SerializeField] private GameObject floatingText;
    public float floatingTextDuration = 0.5f;
    private readonly float flipTime = 0.3f;
    private int respawnTime;
    private int moveDirection;
    public float speed;
    private int waveFreq;
    private float waveAmp;

    private bool isRotating = false;
    public bool isHiding = false;
    private bool needsToReset;
    private bool hasBeenStarted = false;
    private int cycleCount;
    private float timeSinceBounce;
    private Vector2 initialPosition;

    private Coroutine MoveCoroutine = null;
    private Coroutine RespawnCoroutine = null;
    private Coroutine FlipCoroutine = null;
    public AudioClip[] hitAudio;

    private Coroutine DownwardTransitionCoroutine;
    private bool downwardTransitionDone = false;
    private float initialScale;


    private void Start()
    {
        SetInitialStats();
    }
    //This will be called by Enemy Manager, starts the game objects actions
    private void SetInitialStats() {
        initialPosition = transform.position;
        initialScale = transform.localScale.x;
        speed = ai.movementSpeed;
        respawnTime = ai.respawnTime;
        moveDirection = ai.moveDirectionRight ? 1 : -1;
        needsToReset = respawnTime != 0 || ai.maxCycles == -1;
        cycleCount = 0;
        timeSinceBounce = 0;
        waveFreq = ai.waveFrequency;
        waveAmp = ai.waveAmplitude;
        isHiding = !ai.startFacingUp;
        DisableColliders();
        if (!ai.startFacingUp) {
            transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
    public void StartActions()
    {
        if (ai.level != GameManager.Instance.currentLevel) {
            Debug.Log("Enemy is in wrong level, not gunna do it's actions!");
            return;
        }
        //Debug.Log("Start actions called");
        if (hasBeenStarted)
        {
            //Debug.Log("Already started!");
            return;
        }
        else
        {
            hasBeenStarted = true;
        }
        //should the enemy start facing up? eg. off screen, if so, start it up, otherwise flip it
        if (ai.startFacingUp)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            EnableColliders();
        }
        else
        {
            //Debug.Log("Should flip over");
            TargetFlip();
        }
        if (ai.downwardTransition && !downwardTransitionDone)
        {
            DownwardTransitionCoroutine = StartCoroutine(DownwardTransition());
        }
        else if (speed > 0)
        {
            MoveCoroutine = StartCoroutine(Move());
        }
        //only needed if stationary, if moving then base on maxCycles
        if (ai.timeBeforeFlippingDown > 0)
        {
            FlipCoroutine = StartCoroutine(FlipTime());
        }
    }

    public void Hit()
    {
        UIManager.Instance.SpawnText(transform.position, ai.pointValue);
        GameManager.Instance.AddToScore(ai.pointValue);
        //moving, needs to be reset since maxCycles is >= 0, 0 for repeat, greater for bounce
        if (speed > 0 && ai.maxCycles >= 0)
        {
            ResetMove(); //flip is called here
        }
        else {
            isRotating = false;
            isHiding = false;
            TargetFlip(); //flip over no matter what
            if (respawnTime > 0)
            {
                RespawnCoroutine = StartCoroutine(Respawn());
            }
        }
    }
    //Called when the target is hit
    public void TargetFlip()
    {
        if (!isRotating)
        {
            if (isHiding)
            {
                //target not shown, should enable collider and flip it up
                StartCoroutine(Flip(true));
            }
            else
            {
                //target was hit, disable colliders and flip it down
                StartCoroutine(Flip(false));
            }
        }
    }
    //if flippedDown (true), flips it up, else if flippedUp (false), flip it down
    private IEnumerator Flip(bool flippedDown)
    {
        //Debug.Log($"Is flipped down: {flippedDown}, so will be flipping it up: {flippedDown}");
        isRotating = true;
        float elapsed = 0.0f;
        if (!flippedDown)
        {
            DisableColliders();
        }

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
        if (flippedDown)
        {
            EnableColliders();
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
    private IEnumerator Respawn(bool movingEnemy = false)
    {
        if (FlipCoroutine != null) {
            StopCoroutine(FlipCoroutine);
            FlipCoroutine = null;   
        }
        yield return new WaitForSeconds(respawnTime);
        if (movingEnemy)
        {
            Debug.Log("Moving enemy detected, calling start actions");
            yield return new WaitForSeconds(0.5f);
            StartActions();
        }
        else {
            TargetFlip();
            if (ai.timeBeforeFlippingDown > 0)
            {
                FlipCoroutine = StartCoroutine(FlipTime());
            }
        }
    }
    private IEnumerator Move()
    {
        bool isEven = transform.GetSiblingIndex() % 2 == 0;
        //Debug.Log("Enemy is even?" + isEven);
        int multipier = isEven && ai.alternate ? -1 : 1;

        while (true)
        {
            if (ai.waveMotion)
            {
                float verticalMove = Mathf.Sin((Time.time) * waveFreq) * waveAmp;
                transform.position = new Vector3(
                    transform.position.x + (moveDirection * speed * Time.deltaTime), // Move left/right
                    initialPosition.y + verticalMove * multipier, // Oscillate up/down around the initial Y position
                    transform.position.z // Keep the Z position the same
                );
            }
            else {
                transform.position += new Vector3(moveDirection * speed * Time.deltaTime, 0, 0);
            }
            //Debug.Log(verticalMove);
            // Move target left or right based on moveDirection

            // Check for boundaries
            if (transform.position.x >= ai.rightBound.x || transform.position.x <= ai.leftBound.x)
            {
                HandleBoundary();
            }
            timeSinceBounce += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
    }
    private void HandleBoundary()
    {
        bool isRightBound = transform.position.x >= ai.rightBound.x;
        cycleCount++;
        //check for -1, which means infinitely needs to reset
        if (ai.maxCycles == -1)
        {
            needsToReset = true;
        }
        //increment cycles, is it still less than maxCycles? Go again
        else if (cycleCount > 0 && cycleCount < ai.maxCycles)
        {
            needsToReset = true;
        }
        //Neither top are true, so don't reset it
        else {
            needsToReset = false;
        }
        if (ai.repeat)
        {
            Vector3 newPosition = isRightBound ? ai.leftBound : ai.rightBound;
            //Debug.Log(newPosition);
            //put it at the opposite boundry, flip it back up if it needs to reset
            transform.SetPositionAndRotation(new Vector2(newPosition.x, initialPosition.y), Quaternion.Euler(needsToReset ? 0 : 90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z));
            if (RespawnCoroutine != null)
            {
                StopCoroutine(RespawnCoroutine);
                RespawnCoroutine = null;
            }
            if (ai.maxCycles == 0) {
                ResetMove();
            } else if (needsToReset)
            {
                isHiding = false;
                EnableColliders();
            }
            else
            {
                isHiding = true;
                TargetFlip();
            }
        }
        else if (ai.bounce)
        {
            //Debug.Log("Bounce called");
            //Debug.Log($"cycle count of {cycleCount}");
            //Debug.Log($"Needs to reset {needsToReset} and time since: {timeSinceBounce}");
            if (needsToReset && timeSinceBounce > 0.45f)
            {
                moveDirection *= -1;
                timeSinceBounce = 0;
            }
            else if (!needsToReset || cycleCount == ai.maxCycles){
                ResetMove();
            }
            // Flip direction
        }
        else
        {
            isHiding = false;
            TargetFlip();
        }
    }
    //ADD - only needed for stationary objects
    private IEnumerator FlipTime()
    {
        yield return new WaitForSeconds(ai.timeBeforeFlippingDown);
        if (RespawnCoroutine != null) {
            StopCoroutine(RespawnCoroutine);
            RespawnCoroutine = null;
        }
        isHiding = false;
        TargetFlip();
        yield return new WaitForSeconds(0.5f); //time waiting for flip
        ResetEnemy(); //reset the enemy to original position/status
        if (respawnTime > 0)
        { //means it should respawn forever
            RespawnCoroutine = StartCoroutine(Respawn());
        }
    }
    private IEnumerator DownwardTransition()
    {
        float elapsedTime = 0f;
        float scaleAndMoveTime = 1.0f; // how long to move/scale
        float initialScale = transform.localScale.x; // All 3 scales are the same scale
        yield return new WaitForSeconds(0.3f); // let flip coroutine finish first?

        while (elapsedTime < scaleAndMoveTime)
        {
            if (ai.scaleUp)
            {
                float scaledAmount = Mathf.Lerp(initialScale, ai.finalScale, elapsedTime / scaleAndMoveTime);
                transform.localScale = new Vector3(scaledAmount, scaledAmount, scaledAmount);
            }

            // Update position with correct syntax
            Vector3 currentPosition = transform.position;
            float newYPosition = Mathf.Lerp(ai.initialDowny, ai.endingDowny, elapsedTime / scaleAndMoveTime);
            transform.position = new Vector3(currentPosition.x, newYPosition, currentPosition.z);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        downwardTransitionDone = true;

        yield return new WaitForSeconds(0.5f); // Delay before moving?

        MoveCoroutine = StartCoroutine(Move()); // Call next actions after delay
    }
    private void ResetMove()
    {
        StartCoroutine(ResetMovingEnemy());
    }
    private IEnumerator ResetMovingEnemy()
    {
        if (GameManager.Instance.currentLevel == 0)
        {
            yield return null;
        }
        isHiding = false; //set so that targetflip will flip it over
        TargetFlip(); //it either swapped sides, bounced last time, or was hit
        yield return new WaitForSeconds(0.5f); //time for flip to finish
        ResetEnemy(); //resets to original position
        yield return new WaitForSeconds(1); //waits for reset to finish
        RespawnCoroutine = StartCoroutine(Respawn(true));
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
    public int GetSpawnDelay() { 
        return ai.timeBeforeShowingInScene;
    }
    public void StopAllActions()
    {
        StopAllCoroutines();
        MoveCoroutine = null;
        RespawnCoroutine = null;
        DownwardTransitionCoroutine = null;
        FlipCoroutine = null;
    }
    public void ResetEnemy()
    {
        if (MoveCoroutine != null) {
            StopCoroutine(MoveCoroutine);
            MoveCoroutine = null;
        }
        if (RespawnCoroutine != null) { 
            StopCoroutine (RespawnCoroutine);
            RespawnCoroutine = null;
        }
        if (DownwardTransitionCoroutine != null)
        {
            StopCoroutine(DownwardTransitionCoroutine);
            DownwardTransitionCoroutine = null;
        }
        if (FlipCoroutine != null) {
            StopCoroutine(FlipCoroutine);
            FlipCoroutine = null;
        }
        if (!ai.startFacingUp)
        {
            transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            DisableColliders();
        }
        transform.localScale = new Vector3(initialScale, initialScale, initialScale);
        transform.position = initialPosition;
        downwardTransitionDone = false;
        isHiding = !ai.startFacingUp;
        isRotating = false;
        hasBeenStarted = false;
        cycleCount = 0;
        timeSinceBounce = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Hit();
            if (hitAudio.Length > 0)
            {
                AudioManager.Instance.PlaySFX(hitAudio[Random.Range(0, hitAudio.Length)]);
            }
        }
    }
}
