using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private GameManager gm;
    public Vector3 TargetPosition { get; private set; }
    private Vector3 mouse;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    private void Update()
    {
        if (!GameManager.Instance.gamePaused) { 
            mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float tenPercentHeight = gm.ScreenSize.y * 0.1f;
            float twentyFiveHigh = gm.ScreenSize.y * 0.25f;
            float tenPercentWidth = gm.ScreenSize.x * 0.1f;
            float clampedX = Mathf.Clamp(mouse.x, -gm.ScreenSize.x / 2 + tenPercentWidth, gm.ScreenSize.x / 2 - tenPercentWidth);
            float clampedY = Mathf.Clamp(mouse.y, -gm.ScreenSize.y / 2 + twentyFiveHigh, gm.ScreenSize.y / 2 - tenPercentHeight);
            transform.position = new Vector3(clampedX, clampedY, 0);
            TargetPosition = transform.position;
        }
    }
}
