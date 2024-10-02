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
    [SerializeField] private GameObject CountdownParent;
    [SerializeField] private TextMeshProUGUI endScore, highScoreTut, highScoreWest;
    [SerializeField] private Image colorButton;
    [SerializeField] private Sprite[] ssSprites;
    [SerializeField] private Slider sfxSlider, musicSlider;
    [SerializeField] private Button startButton, ssButton, exitButton;

    private RectTransform wRect, cRect;
    public bool inMenu = false, inLevelEndMenu = false;

    public int tutHS = 0, westHS = 0;
    // Public integers for SFX and Music volume (0-10)
    public int SFXVolume { get; private set; }
    public int MusicVolume { get; private set; }
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
        SFXVolume = 5;
        MusicVolume = 3;
    }
    private void Start()
    {
        wRect = worldCanvas.GetComponent<RectTransform>();
        cRect = counterCanvas.GetComponent<RectTransform>();
        ShowStart(true);
        sfxSlider.value = 5;
        musicSlider.value = 3;
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        highScoreTut.gameObject.SetActive(false);
        highScoreWest.gameObject.SetActive(false);
    }
    private void Update()
    {
        wRect.sizeDelta = new Vector2(GameManager.Instance.ScreenSize.x, GameManager.Instance.ScreenSize.y);
        cRect.sizeDelta = new Vector2(GameManager.Instance.ScreenSize.x, GameManager.Instance.ScreenSize.y);
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.inStartMenu)
        {
            SettingsStartMenu();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.Instance.loadingLevel && !GameManager.Instance.inStartMenu && !inLevelEndMenu) {
            SettingsMenu(true);
        }
    }

    private void SettingsStartMenu() {
        if (inMenu)
        {
            startButton.gameObject.SetActive(true);
            ssButton.gameObject.SetActive(true);
            blackScreen.gameObject.SetActive(false);
            settingsScreen.SetActive(false);
            exitButton.gameObject.SetActive(false);
            Cursor.visible = true;
            inMenu = false;
        }
        else
        {
            startButton.gameObject.SetActive(false);
            ssButton.gameObject.SetActive(false);
            blackScreen.gameObject.SetActive(true);
            settingsScreen.SetActive(true);
            exitButton.gameObject.SetActive(false);
            Cursor.visible = true;
            inMenu = true;
        }
    }

    public void SettingsMenu(bool v) {
        GameManager.Instance.PauseGame();
        if (inMenu || v == false) { 
            blackScreen.gameObject.SetActive(false);
            settingsScreen.SetActive(false);
            inMenu = false;
        }
        else
        {
            blackScreen.gameObject.SetActive(true);
            settingsScreen.SetActive(true);
            exitButton.gameObject.SetActive(true);
            inMenu = true;
        }
    }
    public void SettingsButtonMenu() {
        GameManager.Instance.PauseGame();
        if (GameManager.Instance.inStartMenu) { 
            SettingsStartMenu();
            return;
        }
        if (inMenu)
        {
            blackScreen.gameObject.SetActive(false);
            settingsScreen.SetActive(false);
            inMenu = false;
        }
        else
        {
            blackScreen.gameObject.SetActive(true);
            settingsScreen.SetActive(true);
            exitButton.gameObject.SetActive(true);
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

    public void ShowStart(bool show) {
        if (tutHS == 0)
        {
            highScoreTut.gameObject.SetActive(false);
        }
        else {
            highScoreTut.gameObject.SetActive(true);
        }
        if (westHS == 0)
        {
            highScoreWest.gameObject.SetActive(false);
        }
        else
        {
            highScoreWest.gameObject.SetActive(true);
        }
        startScreen.SetActive(show);
    }
    public void ShowEndLevel(bool show) {
        if (show)
        {
            inMenu = true;
            inLevelEndMenu = true;
            endLevelScreen.SetActive(true);
            endScore.text = GameManager.Instance.score.ToString();
            blackScreen.gameObject.SetActive(true);
        }
        else {
            inLevelEndMenu = false;
            if (endLevelScreen.activeInHierarchy) {
                inMenu = false;
                endLevelScreen.SetActive(false);
                blackScreen.gameObject.SetActive(false);
            }
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

    public void ChangeSSButtonColor(int color) { 
        colorButton.sprite = ssSprites[color];
    }

    private void OnSFXSliderChanged(float value)
    {
        SFXVolume = Mathf.RoundToInt(value);
        AudioManager.Instance.ChangeSFXVolume(SFXVolume);
    }
    private void OnMusicSliderChanged(float value)
    {
        MusicVolume = Mathf.RoundToInt(value); 
        AudioManager.Instance.ChangeMusicVolume(MusicVolume);
    }

    public void UpdateHighScore(int score, int level) {
        if (level == 0)
        {
            tutHS = score;
            highScoreTut.text = score.ToString();
        }
        else { 
            westHS = score;
            highScoreWest.text = score.ToString();
        }
    }
}
