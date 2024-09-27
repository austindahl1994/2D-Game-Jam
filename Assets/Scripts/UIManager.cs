using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private Canvas worldCanvas, screenCanvas, counterCanvas;
    [SerializeField] private GameObject textObject, settingsScreen, startScreen, endLevelScreen;
    [SerializeField] private Image blackScreen;
    [SerializeField] private TMP_Dropdown dd;
    [SerializeField] private GameObject CountdownParent;
    [SerializeField] private TextMeshProUGUI endScore;

    private RectTransform wRect, cRect;
    public bool inMenu = false;
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
        wRect = worldCanvas.GetComponent<RectTransform>();
        cRect = counterCanvas.GetComponent<RectTransform>();
        dd.onValueChanged.AddListener(OnDropdownValueChanged);
        ShowStart(true);
    }
    private void Update()
    {
        wRect.sizeDelta = new Vector2(GameManager.Instance.ScreenSize.x, GameManager.Instance.ScreenSize.y);
        cRect.sizeDelta = new Vector2(GameManager.Instance.ScreenSize.x, GameManager.Instance.ScreenSize.y);
        if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.Instance.loadingLevel && !GameManager.Instance.inStartMenu) {
            SettingsMenu();
        }
    }
    public void SettingsMenu() {
        GameManager.Instance.PauseGame();
        if (inMenu) { 
            blackScreen.gameObject.SetActive(false);
            settingsScreen.SetActive(false);
            inMenu = false;
        }
        else
        {
            blackScreen.gameObject.SetActive(true);
            settingsScreen.SetActive(true);
            inMenu = true;
        }
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
    private void OnDropdownValueChanged(int selectedIndex)
    {
        GameManager.Instance.SetSlingshotColor(selectedIndex);
    }
    public void ShowStart(bool show) { 
        startScreen.SetActive(show);
    }
    public void ShowEndLevel(bool show) {
        if (show)
        {
            endLevelScreen.SetActive(true);
            endScore.text = GameManager.Instance.score.ToString();
            blackScreen.gameObject.SetActive(true);
        }
        else {
            endLevelScreen.SetActive(false);
            blackScreen.gameObject.SetActive(false);
        }
    }
    //Ready, set, go!
    public void StartReady() {
        StartCoroutine(Cd());
    }
    private IEnumerator Cd() {
        CountdownParent.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        CountdownParent.transform.GetChild(1).gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        CountdownParent.transform.GetChild(2).gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        foreach (Transform child in CountdownParent.transform) { 
            child.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1.0f);
    }
}
