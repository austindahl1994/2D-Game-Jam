using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    //move all this to a Transition Manager
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private RectTransform leftCurtain, rightCurtain;
    [SerializeField] private Light2D worldLight;
    [SerializeField] private GameObject[] hideWithLights;
    private bool moving, lightsChange = false;
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
        CurtainsStartOpen();
    }
    private void Update()
    {
        //JUST FOR TESTING
        if (Input.GetMouseButtonDown(1))
        {
            CurtainCall();
        }
        if (Input.GetMouseButtonDown(2))
        {
            Lights();
        }
    }
    private void CurtainCall()
    {
        if (!moving)
        {
            StartCoroutine(Curtains());
        }
    }
    private void Lights()
    {
        if (!lightsChange)
        {
            StartCoroutine(LightSwap());
        }
    }
    private IEnumerator LightSwap()
    {
        lightsChange = true;
        int value = worldLight.intensity == 0 ? 1 : 0;
        if (value == 0)
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
            if (value == 1 && elapsedTime >= 1)
            {
                foreach (GameObject g in hideWithLights)
                {
                    g.SetActive(true);
                }
            }
            yield return null;
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
    private void CurtainsStartOpen() {
        float initial = leftCurtain.anchoredPosition.x;
        leftCurtain.anchoredPosition = new Vector2(-initial, leftCurtain.anchoredPosition.y);
        rightCurtain.anchoredPosition = new Vector2(initial, leftCurtain.anchoredPosition.y);
    }
}
