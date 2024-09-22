using System.Collections;
using UnityEngine;

public class MovingEnemy : Enemy
{
    private GameManager gm;
    private int moveDirection; // 1 for right, -1 for left
    private float speed = 2.0f; // Movement speed
    private float boundary; // The boundary for movement

    private void Start()
    {
        gm = GameManager.Instance;
        if (transform.position.x < 0)
        {
            moveDirection = 1;
        }
        else
        {
            moveDirection = -1;
        }

        boundary = gm.ScreenSize.x / 2; 
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        while (true) 
        {
            transform.position += new Vector3(moveDirection * speed * Time.deltaTime, 0, 0);
            if (transform.position.x >= boundary || transform.position.x <= -boundary)
            {
                moveDirection *= -1;
            }

            yield return null; // Wait for the next frame
        }
    }
}
