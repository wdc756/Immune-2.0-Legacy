using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class UiMan : MonoBehaviour
{
    public GameObject buttonPrefab;
    public GameObject parentCanvas;
    public VisualManager visualMana;
    public BodySimulation body;
        
    public void LoadScene(VisualScene scene)
    {

        List<Vector3> pathPosition = scene.GetPathPositions();
        List<VisualScene> pathLink = scene.GetPathLinks();
        
        foreach(var path in pathLink)
        {
            string pathName = path.gameObject.name;

            GameObject newButton = Instantiate(buttonPrefab, parentCanvas.transform);
            newButton.GetComponentInChildren<Text>().text = path.gameObject.name;

            //newButton.GetComponent<Button>().onClick.AddListener(() => VisualManager.Instance.ChangeScene(sceneIndex));
            int sceneIndex = path.sceneIndex;
        }

        for(int i = 0; i < pathLink.Count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, parentCanvas.transform);
            newButton.GetComponentInChildren<Text>().text = pathLink[i].gameObject.name;

            //newButton.GetComponent<Button>().onClick.AddListener(() => VisualManager.Instance.ChangeScene(sceneIndex));
            int sceneIndex = pathLink[i].sceneIndex;
        }

    }

    private void SelectButton(int world, int index)
    {
        Debug.Log("Button has been hit");
    }
}
