using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
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
    }

    //add logic for levels from notes, including spawnMultiple script and tag when multiple should spawn in a row rather than just the one at a time
    //figure out logic to have enemies spawn at correct time via 1 coroutine, each has their own coroutine? seems excessive
    /*
     foreach (Enemy e in enemiesToSpawn) {
            // Calculate the remaining time before the total duration is reached
            float remainingTime = totalDuration - currentTime;

            // Wait for the specified spawn delay or the remaining time, whichever is smaller
            float waitTime = Mathf.Min(enemyData.spawnTimeDelay, remainingTime);
            yield return new WaitForSeconds(waitTime);
            
            // Spawn the enemy
            SpawnEnemy(e.enemyPrefab);
            
            // Update the current time after spawning
            currentTime += waitTime;

            // Check if the total duration has been reached
            if (currentTime >= totalDuration) {
                break; // Exit the loop if the total duration is reached
            }
        }*/
    //have to sort enemies by time first
}
