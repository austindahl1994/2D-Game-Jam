using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ScriptableObjects/EnemyConfig", order = 1)]
public class EnemySO : ScriptableObject
{
    [Header("Bounce and Repeat")]
    public Vector2 leftBound;
    public Vector2 rightBound;
    [Header("Point Value")]
    public int pointValue;
    [Header("Time before showing and flipping down")]
    public int timeBeforeShowingInScene;
    public int timeBeforeFlippingDown;
    [Header("Respawn time and how many cycles")]
    public int respawnTime; //if no respawn time, doesnt respawn unless cycles
    public int maxCycles;
    public bool bounce;
    public bool repeat; 
    [Header("Move speed and direction")]
    public float movementSpeed = 0;
    public bool moveDirectionRight;
    [Header("Start Up, Wave, Freq is how fast, Amp is how high")]
    public bool startFacingUp;
    public bool waveMotion;
    public bool alternate;
    public int waveFrequency; //How fast up/down jiggle
    public float waveAmplitude; //High high above/below initial y to go
}
