using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    //move all this to a Transition Manager
    [SerializeField] private RectTransform leftCurtain, rightCurtain;
    [SerializeField] private Light2D worldLight;
    [SerializeField] private GameObject[] hideWithLights; //add west to this
    [SerializeField] private AudioClip curtainSFX;
    private bool moving = false, lightsChange = false;
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

    private void Start()
    {
        //CurtainsStartOpen();
        CurtainsStartClosed();
    }

    public void CurtainCall()
    {
        //Debug.Log("Curtains called and is moving: " + moving);
        if (!moving)
        {
            StartCoroutine(Curtains());
            AudioManager.Instance.PlaySFX(curtainSFX);
        }
    }
    public void Lights(bool lightsOn)
    {
        if (!lightsChange)
        {
            StartCoroutine(LightSwap(lightsOn));
        }
    }
    private IEnumerator LightSwap(bool lightsOn)
    {
        lightsChange = true;
        //if lights are on, want to turn them off
        int value = lightsOn ? 0 : 1;
        //if lights are currently on and we are going to be shutting them off, turn off timer/scoreboard
        if (lightsOn)
        {
            foreach (GameObject g in hideWithLights)
            {
                g.SetActive(false);
            }
        }
        float elapsedTime = 0;
        while (elapsedTime < 1.0f)
        {
            worldLight.intensity = Mathf.Lerp(worldLight.intensity, value, elapsedTime / 1.0f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (lightsOn)
        {
            foreach (GameObject g in hideWithLights)
            {
                g.SetActive(true);
            }
        }
        lightsChange = false;
    }
    private IEnumerator Curtains()
    {
        moving = true;
        float initial = leftCurtain.anchoredPosition.x;
        float elapsedTime = 0;
        float duration = 1.0f;

        while (elapsedTime < duration)
        {
            float newX = Mathf.Lerp(initial, -initial, elapsedTime / duration);

            leftCurtain.anchoredPosition = new Vector2(newX, leftCurtain.anchoredPosition.y);
            rightCurtain.anchoredPosition = new Vector2(-newX, leftCurtain.anchoredPosition.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        leftCurtain.anchoredPosition = new Vector2(-initial, leftCurtain.anchoredPosition.y);
        rightCurtain.anchoredPosition = new Vector2(initial, leftCurtain.anchoredPosition.y);

        moving = false;
    }
    private void CurtainsStartClosed()
    {
        float initial = leftCurtain.anchoredPosition.x;
        leftCurtain.anchoredPosition = new Vector2(initial, leftCurtain.anchoredPosition.y);
        rightCurtain.anchoredPosition = new Vector2(-initial, leftCurtain.anchoredPosition.y);
    }
    /*
    private void CurtainsStartOpen() {
        float initial = leftCurtain.anchoredPosition.x;
        leftCurtain.anchoredPosition = new Vector2(-initial, leftCurtain.anchoredPosition.y);
        rightCurtain.anchoredPosition = new Vector2(initial, leftCurtain.anchoredPosition.y);
    }*/

}
