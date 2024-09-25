using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private Canvas worldCanvas, screenCanvas;
    [SerializeField] private GameObject textObject;

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
        worldCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(GameManager.Instance.ScreenSize.x, GameManager.Instance.ScreenSize.y);
    }
    public void SpawnText(Vector3 worldPosition, int value)
    {
        // Convert the world position to a screen position
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // Instantiate the text object and set its parent to the canvas
        GameObject spawnedText = Instantiate(textObject, screenCanvas.transform);

        // Set the position of the text object to the screen position
        RectTransform rectTransform = spawnedText.GetComponent<RectTransform>();
        rectTransform.position = screenPosition;

        if (spawnedText != null) { 
            spawnedText.GetComponentInChildren<TextMeshProUGUI>().text = "+" + value.ToString();
            Destroy(spawnedText, 1.0f);
        }
    }
}
