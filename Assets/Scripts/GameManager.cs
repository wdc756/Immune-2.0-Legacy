using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*
     This is like a main() function, thats responsible for initializing all the various systems in the game. 
    
     It will handle when to change scenes, what stuff to generate, as well as error handling.
     */

    public VisualManager visualManager;    

    void Start()
    {
        //makes sure this object is always in the scene
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        
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
        if (visualManager == null)
        {
            Error("Could not find visualManager in the game scene");
            yield break;
        }
        visualManager.SetUp(this);
    }

    //Links to an error screen that will output the string message by adding it to the string error list
    public void Error(string message)
    {
        Debug.LogWarning(message);
    }
}
