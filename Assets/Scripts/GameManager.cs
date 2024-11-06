using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*
     This is like a main() function, thats responsible for initializing all the various systems in the game. 
    
     It will handle when to change scenes, what stuff to generate, as well as error handling.
     */

    VisualManager visualManager;    

    void Start()
    {
        //makes sure the game will always start in the main menu
        if (SceneManager.GetActiveScene() != SceneManager.GetSceneAt(0))
        {
            Debug.LogError("Scene other than main menu was loaded first");
            SceneManager.LoadScene(0);
        }

        //makes sure this object is always in the scene
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        
    }

    //called by the Start Game button in the main menu
    void StartGame()
    {
        //loads the game scene
        SceneManager.LoadScene(1);

        visualManager = FindObjectOfType<VisualManager>();
        if (visualManager == null)
        {
            Error("Could not find visualManager in the game scene");
            return;
        }
        visualManager.SetUp(this);
    }

    //Links to an error screen that will output the string message by adding it to the string error list
    public void Error(string message)
    {
        //add string to list of all errors then display errors
    }
}
