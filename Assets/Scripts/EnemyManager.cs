using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    [SerializeField] private Transform tutorialLevel;
    [SerializeField] private Transform westLevel;
    public List<GameObject> tutorialEnemies;
    public List<GameObject> westEnemies;
    private List<List<GameObject>> enemyGroups = new();
    private Vector2 offScreen;
    private Coroutine StopActionCoroutine = null;
    //private Coroutine StartLevelCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        tutorialEnemies ??= new List<GameObject>();
        westEnemies ??= new List<GameObject>();
    }

    private void OnEnable()
    {
        GetAllEnemies(tutorialLevel, tutorialEnemies);
        GetAllEnemies(westLevel, westEnemies);
        enemyGroups.Add(tutorialEnemies);
        enemyGroups.Add(westEnemies);
        SortEnemies(tutorialEnemies);
        SortEnemies(westEnemies);
        offScreen = new Vector2(GameManager.Instance.ScreenSize.x * 10, GameManager.Instance.ScreenSize.y * 10);
        //Debug.Log(enemyGroups.Count);
        /*
         westLevel.position = new Vector2(GameManager.Instance.ScreenSize.x * 10, GameManager.Instance.ScreenSize.y * 10);
        */
    }

    private void GetAllEnemies(Transform level, List<GameObject> list)
    {
        AddEnemyToList(level, list);
    }

    //recursively called to check each transform of a level and add enemies to the list
    private void AddEnemyToList(Transform currentTransform, List<GameObject> enemyList)
    {
        if (currentTransform.CompareTag("enemy"))
        {
            enemyList.Add(currentTransform.gameObject);
        }

        foreach (Transform child in currentTransform)
        {
            AddEnemyToList(child, enemyList);
        }
    }
    public void ChangeShownLevel(int cl) {
        if (cl == 0) //1 for testing west, 0 normally to play starting with tutorial/ducks
        {
            //Debug.Log("Moving west out of way");
            tutorialLevel.transform.position = Vector2.zero;
            westLevel.position = offScreen;
        }
        else {
            //Debug.Log("Moving tutorial out of way");
            westLevel.transform.position = Vector2.zero;
            tutorialLevel.position = offScreen;
        }
    }
    private void SortEnemies(List<GameObject> enemies)
    {
        enemies.Sort((a, b) =>
        {
            var enemyA = a.GetComponent<Enemy>();
            var enemyB = b.GetComponent<Enemy>();
            if (enemyA == null || enemyB == null) return 0;
            return enemyA.GetSpawnDelay().CompareTo(enemyB.GetSpawnDelay());
        });
    }
    public void ResetCurrentLevelEnemies() {
        Debug.Log($"Resestting enemies for current level: {GameManager.Instance.currentLevel}");
        foreach (GameObject enemy in enemyGroups[GameManager.Instance.currentLevel]) { 
            enemy.GetComponent<Enemy>().StopAllActions();
            enemy.GetComponent<Enemy>().ResetEnemy();
        }
    }
    public void StartPlayingLevel(int level) {
        Debug.Log("Starting level: " + level);
        if (level == 0)
        {
            StartCoroutine(StartLevel(tutorialEnemies));
        }
        else {
            StartCoroutine(StartLevel(westEnemies));
        }
    }
    public void EndLevel() {
        StartCoroutine(EndPlayingLevel());
    }
    public void ForceStopLevel() {
        //Debug.Log("Force stop called");
        StopAllCoroutines();
        StopActionCoroutine = null;
        foreach (GameObject enemy in enemyGroups[GameManager.Instance.currentLevel])
        {
            enemy.GetComponent<Enemy>().StopAllActions();
            enemy.GetComponent<Enemy>().ResetEnemy();
        }
    } //TODO
    private IEnumerator StartLevel(List<GameObject> enemies)
    {
        // This will hold the latest time to wait for
        float currentTime = 0f;

        // Create a list to hold all enemy spawn delays
        List<int> spawnDelays = new();

        // First, gather all spawn delays
        foreach (GameObject enemy in enemies)
        {
            if (enemy.TryGetComponent<Enemy>(out Enemy ec))
            {
                spawnDelays.Add(ec.GetSpawnDelay());
            }
        }

        // Now, wait for each unique spawn time
        foreach (int delay in spawnDelays)
        {
            // If the current time is less than the delay, wait
            if (currentTime < delay)
            {
                yield return new WaitForSeconds(delay - currentTime);
                currentTime = delay; // Update current time to the latest delay
            }

            // Spawn enemies with the current delay
            foreach (GameObject enemy in enemies)
            {
                if (enemy.TryGetComponent<Enemy>(out Enemy ec) && ec.GetSpawnDelay() == delay)
                {
                    ec.StartActions(); // Start actions for enemies with this delay
                }
            }
        }

        yield return null; // Optional final yield to ensure completion
    }

    //when level done, new coroutine for each enemy to stop actions and flip over
    private IEnumerator EndPlayingLevel()
    {
        Debug.Log("End playing level called for level " + GameManager.Instance.currentLevel);
        foreach (GameObject enemy in enemyGroups[GameManager.Instance.currentLevel])
        {
            StopActionCoroutine = StartCoroutine(StopEnemyActions(enemy));
        }

        yield return null;
    }
    private IEnumerator StopEnemyActions(GameObject enemy)
    {
        Enemy ec = enemy.GetComponent<Enemy>();
        ec.StopAllActions();
        yield return new WaitForSecondsRealtime(0.5f); 
        if (!ec.isHiding)
        {
            ec.TargetFlip();
        }
        yield return new WaitForSecondsRealtime(0.4f);
        ec.ResetEnemy();
        yield return null;
    }

}
