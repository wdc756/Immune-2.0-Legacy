using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    /*
     This is like a main() function, thats responsible for initializing all the various systems in the game. 
    
     It will handle when to change scenes, what stuff to generate, as well as error handling.
     */

    public VisualManager visualManager;
    public SimulationManager simulationManager;
    public CellManager cellManager;
    public StopPlaying stopPlaying;

    public GameObject pauseMenu;
    public bool canPause = false;
    public bool isPaused = false;
    public GameObject gameOverMenu;

    public GameObject gameWinMenu;

    public int maxCivilianDeathCount;

    private bool tryingToClose = false;

    public Slider gameWinDeathBar;

    void Start()
    {
        //makes sure this object is always in the scene
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        if (tryingToClose)
        {
            if (!cellManager.AnyBacteriaLeft())
            {
                gameWinMenu.SetActive(true);

                gameWinDeathBar = gameWinMenu.transform.GetChild(1).transform.GetChild(0).gameObject.GetComponent<Slider>();

                gameWinDeathBar.maxValue = maxCivilianDeathCount;
                gameWinDeathBar.value = simulationManager.gameObject.GetComponent<BodySimulation>().CivilianDeathCount;

                Resume();
                canPause = false;
            }
        }
    }



    //called by the Start Game button in the main menu
    public void StartGame()
    {
        StartCoroutine(StartGameAsync());
    }
    //Async scene loading because loading a scene is an async task, so variable setting during scene loading has issues
    private IEnumerator StartGameAsync()
    {
        //starts the loading process
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        visualManager = FindObjectOfType<VisualManager>();
        simulationManager = FindObjectOfType<SimulationManager>();
        if (visualManager == null)
        {
            Error("Could not find visualManager in the game scene");
            yield break;
        }
        visualManager.SetUp(this);
        simulationManager.SetUp(this);

        //set up buttons

        Button pauseButton = GameObject.Find("PauseButton").GetComponent<Button>();
        pauseButton.onClick.AddListener(() => Pause());

        pauseMenu = GameObject.Find("PauseMenu");

        Button resumeButton = pauseMenu.transform.GetChild(0).gameObject.GetComponent<Button>();
        resumeButton.onClick.AddListener(() => Resume());

        Button exitButton = pauseMenu.transform.GetChild(2).gameObject.GetComponent<Button>();
        exitButton.onClick.AddListener(() => Exit());

        GameObject instructions = pauseMenu.transform.GetChild(4).gameObject;
        Button backButton = instructions.transform.GetChild(1).gameObject.GetComponent<Button>();
        backButton.onClick.AddListener(() => Resume());

        pauseMenu.SetActive(false);


        gameOverMenu = GameObject.Find("GameOverMenu");

        exitButton = gameOverMenu.transform.GetChild(0).gameObject.GetComponent<Button>();
        exitButton.onClick.AddListener(() => Exit());

        gameOverMenu.SetActive(false);

        gameWinMenu = GameObject.Find("GameWinMenu");
        Debug.Log(gameWinMenu);

        exitButton = gameWinMenu.transform.GetChild(0).gameObject.GetComponent<Button>();
        exitButton.onClick.AddListener(() => Exit());

        gameWinMenu.SetActive(false);


        canPause = true;
        Pause();
    }
    
    //temp, called by exit button
    public void StartTesting()
    {
        StartCoroutine(StartTestingAsync());
    }
    private IEnumerator StartTestingAsync()
    {
        //starts the loading process
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(2);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        visualManager = FindObjectOfType<VisualManager>();
        //simulationManager = FindObjectOfType<SimulationManager>();
        if (visualManager == null)
        {
            Error("Could not find visualManager in the game scene");
            yield break;
        }
        visualManager.SetUp(this);
        //simulationManager.SetUp(this);
    }



    public void Pause()
    {
        if (canPause)
        {
            Time.timeScale = 0f;
            pauseMenu.SetActive(true);
            isPaused = true;
        }
    }
    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void Win()
    {
        if (!gameOverMenu.activeInHierarchy)
        {
            tryingToClose = true;
            cellManager = FindObjectOfType<CellManager>();

            //remove all bacteria
        }
    }
    public void Lose()
    {
        if (!gameWinMenu.activeInHierarchy)
        {
            gameOverMenu.SetActive(true);
            Resume();
            canPause = false;
        }
    }
    public void Exit()
    {
        stopPlaying = FindObjectOfType<StopPlaying>();
        stopPlaying.CloseGameScene(this);
    }



    public void CloseApp()
    {
        Application.Quit();
    }


    //Links to an error screen that will output the string message by adding it to the string error list
    public void Error(string message)
    {
        Debug.LogWarning(message);
    }
}
