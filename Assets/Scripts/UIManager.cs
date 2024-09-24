using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject textObject;

    public static UIManager Instance;
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

    public void SpawnText(Vector3 worldPosition, int value)
    {
        // Convert the world position to a screen position
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // Instantiate the text object and set its parent to the canvas
        GameObject spawnedText = Instantiate(textObject, canvas.transform);

        // Set the position of the text object to the screen position
        RectTransform rectTransform = spawnedText.GetComponent<RectTransform>();
        rectTransform.position = screenPosition;

        if (spawnedText != null) { 
            spawnedText.GetComponentInChildren<TextMeshProUGUI>().text = "+" + value.ToString();
            Destroy(spawnedText, 1.0f);
        }
    }
}
