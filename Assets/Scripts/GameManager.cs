using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Vector2 ScreenSize { get; private set; }
    private Camera mainCamera;
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


    private void Update()
    {
        CalculateScreenSize();

        Cursor.visible = false;
    }

    private void CalculateScreenSize() {
        float screenHalfHeightInWorldUnits = mainCamera.orthographicSize;
        float screenHalfWidthInWorldUnits = screenHalfHeightInWorldUnits * mainCamera.aspect;

        ScreenSize = new Vector2(screenHalfWidthInWorldUnits * 2, screenHalfHeightInWorldUnits * 2);
    }
}
