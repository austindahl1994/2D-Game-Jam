using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject tutorialScoreboard, tutorialTimerBoard, slingshot, westernScoreBoard, westTimerBoard;
    [SerializeField] private AudioClip startingBell, finalDings;
    public Vector2 ScreenSize { get; private set; }
    public bool gamePaused = false, loadingLevel = false, canPlay = false, inStartMenu = true;
    public int score = 0, currentLevel = 1, colorCount = 0;
    public int tutorialTimer = 30, westernTimer = 60;
    private Coroutine Timer = null; //will stop coroutine if force restart level/exit to main menu, add logic later
    //private Coroutine TransitionCoroutine;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else { 
            Destroy(gameObject);
        }
        CalculateScreenSize();
    }

    private void Start()
    {
        //game is paused at start
        PauseGame();
    }
    private void Update()
    {
        CalculateScreenSize();
    }
    public void PauseGame() {
        if (gamePaused)
        {
            Time.timeScale = 1.0f;
            Cursor.visible = false;
            gamePaused = false;
        }
        else {
            Time.timeScale = 0.0f;
            Cursor.visible = true;
            gamePaused = true;
        }
    }
    public void AddToScore(int value) {
        GameObject scoreBoard;
        if (currentLevel == 0)
        {
            scoreBoard = tutorialScoreboard;
        }
        else {
            scoreBoard = westernScoreBoard;
        }
        score += value;
        scoreBoard.GetComponent<TextMeshPro>().text = score.ToString("D3");
    }
    public void ResetTimers() {
        GameObject timerBoard;
        if (currentLevel == 0)
        {
            timerBoard = tutorialTimerBoard;
            timerBoard.GetComponent<TextMeshPro>().text = tutorialTimer.ToString();
        }
        else
        {
            timerBoard = westTimerBoard;
            timerBoard.GetComponent<TextMeshPro>().text = westernTimer.ToString();
        }
        timerBoard.GetComponent<TextMeshPro>().color = Color.white;
    }
    public void ResetScores() {
        score = 0;
        tutorialScoreboard.GetComponent<TextMeshPro>().text = "000";
        westernScoreBoard.GetComponent<TextMeshPro>().text = "000";
    }
    public void StartPlaying() {
        //game no longer paused
        UIManager.Instance.ShowStart(false);
        EnemyManager.Instance.ChangeShownLevel(1);

        if (gamePaused) { 
            PauseGame();
        }
        loadingLevel = true;
        StartCoroutine(StartGameTransition());
    }
    public void ExitToMenu() {
        if (Timer != null) { 
            StopCoroutine(Timer);
            Timer = null;
        }
        StopAllCoroutines();
        TransitionManager.Instance.CurtainsStartClosed();
        EnemyManager.Instance.ForceStopLevel();
        UIManager.Instance.SettingsMenu(false);
        UIManager.Instance.ShowStart(true);
        UIManager.Instance.ShowEndLevel(false);
        currentLevel = 0;
        EnemyManager.Instance.ResetCurrentLevelEnemies();
        ResetScores();
        canPlay = false;
        inStartMenu = true;
        PauseGame();
        Debug.Log("Exit to menu called");
    }
    private void ChangeLevel() {
        if (currentLevel == 0) {
            EnemyManager.Instance.ChangeShownLevel(currentLevel);
        }
    }
    public void NextLevel() {
        StartCoroutine(NextLevelCoroutine());
        loadingLevel = true;
    }
    //curtains open, mid open player controls slingshot, fully open, wait a second, UIManager starts counting down timer to begin game for 6 seconds, ready... set... GO! 2 seconds for each, at 7 seconds  
    private IEnumerator CountDown()
    {
        GameObject timerBoard;
        int remainingTime;
        if (currentLevel == 0) {
            timerBoard = tutorialTimerBoard;
            remainingTime = tutorialTimer;
        } else {
            timerBoard = westTimerBoard;
            remainingTime = westernTimer;
        }

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
        AudioManager.Instance.PlaySFX(finalDings);
        //add delay, show Time Up! wait a sec, lower all current enemies, reset them, show end level
        StartCoroutine(EndLevel());
    }
    private IEnumerator StartGameTransition() {
        while (true) {
            //yield return new WaitForSeconds(1);
            inStartMenu = false;
            ResetTimers();
            TransitionManager.Instance.CurtainCall();
            yield return new WaitForSeconds(1);
            UIManager.Instance.StartReady();
            //UI manager starts counting down for 6seconds or w.e
            yield return new WaitForSeconds(3);
            //enemy manager starts the game
            AudioManager.Instance.PlaySFX(startingBell);
            canPlay = true;
            EnemyManager.Instance.StartPlayingLevel(currentLevel);
            Timer = StartCoroutine(CountDown());
            loadingLevel = false;
            break;
        }
    }
    private IEnumerator NextLevelCoroutine() {
        while (true) { 
            PauseGame();
            UIManager.Instance.ShowEndLevel(false);
            ResetScores();
            ResetTimers();
            EnemyManager.Instance.ResetCurrentLevelEnemies();
            yield return new WaitForSeconds(0.2f);
            TransitionManager.Instance.Lights(true);
            TransitionManager.Instance.CurtainCall();
            yield return new WaitForSeconds(1);
            ChangeLevel();
            yield return new WaitForSeconds(1);
            TransitionManager.Instance.CurtainCall();
            currentLevel = (currentLevel + 1) % 2;
            yield return new WaitForSeconds(0.5f);
            TransitionManager.Instance.Lights(false);
            yield return new WaitForSeconds(1.2f);
            ResetScores();
            ResetTimers();
            UIManager.Instance.StartReady();
            yield return new WaitForSeconds(3);
            AudioManager.Instance.PlaySFX(startingBell);
            EnemyManager.Instance.StartPlayingLevel(currentLevel);
            Timer = StartCoroutine(CountDown());
            canPlay = true;
            loadingLevel = false;
            break;
        }
    }
    private IEnumerator EndLevel() {
        EnemyManager.Instance.EndLevel();
        canPlay = false;
        slingshot.GetComponent<Slingshot>().ResetSS();
        yield return new WaitForSeconds(1);
        UIManager.Instance.ShowEndLevel(true);
        PauseGame();
    }
    public void SetSlingshotColor() {
        colorCount = (colorCount + 1) % 3;
        slingshot.GetComponent<Slingshot>().SetSlingshot(colorCount);
        UIManager.Instance.ChangeSSButtonColor(colorCount);
    }
    private void CalculateScreenSize() {
        float screenHalfHeightInWorldUnits = Camera.main.orthographicSize;
        float screenHalfWidthInWorldUnits = screenHalfHeightInWorldUnits * Camera.main.aspect;

        ScreenSize = new Vector2(screenHalfWidthInWorldUnits * 2, screenHalfHeightInWorldUnits * 2);
    }

    //TODO, have static image as the main background when game loads in, when play is clicked, title image fades to black, loses opacity, main game loads in with curtains closed, after time curtains open and start playing
    //Have BG image disappear, curtains come in, then light activates
}
