using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ScriptableObjects/EnemyConfig", order = 1)]
public class EnemySO : ScriptableObject
{
    public Vector2 leftBound;
    public Vector2 rightBound;
    public int pointValue;
    public int timeBeforeShowingInScene;
    public int timeBeforeTakingAction;
    public int timeBeforeFlippingDown;
    public int respawnTime;
    public float movementSpeed = 0;
    public bool moveDirectionRight;
    public bool startFacingUp;
    //change to bounce cycles, how many until targets stop and go down, -1 forever
    public bool bounce;
    //change to repeat cycles, how many until targets stop and go down, -1 forever
    public bool repeat; 
    public bool waveMotion;
}
