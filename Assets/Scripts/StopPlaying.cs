using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StopPlaying : MonoBehaviour
{
    //This just closes the game scene

    public void CloseGameScene(GameManager gameManager)
    {
        Destroy(gameManager.gameObject);
        SceneManager.LoadScene(0);
    }
}
