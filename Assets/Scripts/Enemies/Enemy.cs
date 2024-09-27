using System.Collections;
using TMPro;
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

    public AudioClip[] spawnAudio;
    public AudioClip[] hitAudio;

    private void Start()
    {
        SetInitialStats();
    }
    //This will be called by Enemy Manager, starts the game objects actions
    private void SetInitialStats() {
        initialPosition = transform.position;
        speed = ai.movementSpeed;
        respawnTime = ai.respawnTime;
        moveDirection = ai.moveDirectionRight ? 1 : -1;
        needsToReset = respawnTime != 0 || ai.maxCycles == -1;
        cycleCount = 0;
        timeSinceBounce = 0;
        waveFreq = ai.waveFrequency;
        waveAmp = ai.waveAmplitude;
        isHiding = !ai.startFacingUp;
        if (!ai.startFacingUp) {
            transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
    public void StartActions() {
        //if it has speed, it is moving, so start moving
        if (hasBeenStarted)
        {
            return;
        }
        else { 
            hasBeenStarted = true;
        }
        if (ai.movementSpeed > 0)
        {
            MoveCoroutine = StartCoroutine(Move());
        }
        //should the enemy start facing up? eg. off screen, if so, start it up, otherwise flip it
        if (ai.startFacingUp)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        else
        {
            StartCoroutine(Flip(true));
        }
    }
    public void Hit()
    {
        UIManager.Instance.SpawnText(transform.position, ai.pointValue);
        GameManager.Instance.AddToScore(ai.pointValue);
        if (respawnTime > 0) { 
            RespawnCoroutine = StartCoroutine(Respawn());
        }
        //could be flipped over already, but hit from sheriff
        if (!isHiding) { 
            TargetFlip();
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
        isRotating = true;
        float elapsed = 0.0f;

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
            DisableColliders();
        }
        else
        {
            /*
            if (spawnAudio.Length > 0) { 
                AudioManager.Instance.PlaySFX(spawnAudio[Random.Range(0, spawnAudio.Length)]);
            }*/
            EnableColliders();
        }

        //finalizing flip, ensuring it is 0 or 90 rotation, sets if it is hiding (not shown) or not
        if (flippedDown)
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
    private IEnumerator Respawn() {
        yield return new WaitForSeconds(respawnTime);
        TargetFlip();
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

            if (needsToReset)
            {
                isHiding = false;
                EnableColliders();
            }
            else
            {
                isHiding = true;
                DisableColliders();
                //and remove from scene?
            }
        }
        else if (ai.bounce)
        {
            if (needsToReset && timeSinceBounce > 0.45f)
            {
                transform.localScale = new Vector3(moveDirection > 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                if (moveDirection > 0)
                {
                    gameObject.GetComponent<SpriteRenderer>().sortingOrder = -1;
                }
                else {
                    gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
                }
                moveDirection *= -1;
                timeSinceBounce = 0;
            }
            else if (!needsToReset){
                if (MoveCoroutine != null) { 
                    StopCoroutine(MoveCoroutine);
                    MoveCoroutine = null;
                    TargetFlip();
                }
            }
            // Flip direction
        }
        else
        {
            // Deactivate object if no repeat or bounce, needsToRespawn is false (spwncnt && cyc)
            // Or just flip it over
            gameObject.SetActive(false);
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
    public int GetSpawnDelay() { 
        return ai.timeBeforeShowingInScene;
    }
    public void StopAllActions() {
        if (MoveCoroutine != null)
        {
            StopCoroutine(MoveCoroutine);
            MoveCoroutine = null;
        }
        if (RespawnCoroutine != null)
        {
            StopCoroutine(RespawnCoroutine);
            RespawnCoroutine = null;
        }
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
        transform.position = initialPosition;
        if (!ai.startFacingUp)
        {
            transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        isHiding = !ai.startFacingUp;
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
            StartCoroutine(SecondSound());
        }
    }

    private IEnumerator SecondSound() {
        yield return new WaitForSeconds(0.2f);
        if (spawnAudio != null) { 
            AudioManager.Instance.PlaySFX(spawnAudio[0]);
        }
        yield return null;
    }
}
