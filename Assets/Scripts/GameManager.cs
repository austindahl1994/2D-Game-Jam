using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private GameObject timerBoard;
    private int Score;

    public Vector2 ScreenSize { get; private set; }
    private Camera mainCamera;
    private Coroutine Timer = null; //will stop coroutine if force restart level/exit to main menu, add logic later
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else { 
            Destroy(gameObject);
        }
        mainCamera = Camera.main;
        CalculateScreenSize();
    }

    private void Start()
    {
        Timer = StartCoroutine(CountDown());
    }


    private void Update()
    {
        CalculateScreenSize();
        //toggle on and off depending if in a menu or not
        //Cursor.visible = false;
    }

    private void CalculateScreenSize() {
        float screenHalfHeightInWorldUnits = mainCamera.orthographicSize;
        float screenHalfWidthInWorldUnits = screenHalfHeightInWorldUnits * mainCamera.aspect;

        ScreenSize = new Vector2(screenHalfWidthInWorldUnits * 2, screenHalfHeightInWorldUnits * 2);
    }

    public void AddToScore(int value) { 
        Score += value;
        scoreboard.GetComponent<TextMeshPro>().text = Score.ToString("D3");
    }

    private IEnumerator CountDown()
    {
        int remainingTime = 60;

        while (remainingTime >= 0)
        {
            timerBoard.GetComponent<TextMeshPro>().text = remainingTime < 10
                ? "0" + remainingTime.ToString()
                : remainingTime.ToString();
            if (remainingTime < 10) { 
                timerBoard.GetComponent<TextMeshPro>().color = Color.red;
            }
            yield return new WaitForSeconds(1); 

            remainingTime--; 
        }
        timerBoard.GetComponent<TextMeshPro>().text = "00";
    }

    //create scene transition that uses uimanager curtain call and lights twice, once to dim scene, then close curtains, then open curtains and light up scene with delays in middle
}
