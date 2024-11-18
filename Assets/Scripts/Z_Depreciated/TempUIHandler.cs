using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempUIHandler : MonoBehaviour
{
    [Tooltip("Reference to the simulation manager, so we can tell the active section when a button is pressed")]
    public BodySimulation simulation;
    [Tooltip("The visual manger, used to get the active scene reference")]
    public VisualManager visualManager;

    //The active simulation section, which will be set as needed on runtime
    private BodySectionSimulation sectionSim;

    [SerializeField, Tooltip("The canvas the buttons should be added to")]
    private Transform canvasTransform;
    [SerializeField, Tooltip("The canvas rectTransform, used to clamp buttons")]
    private RectTransform canvasRectTransform;
    [SerializeField, Tooltip("The main camera, used to world to screen transformations")]
    private Camera cam;

    [SerializeField, Tooltip("Where the buttons will be parented to")]
    private GameObject buttonParent;
    [SerializeField, Tooltip("The button to be instantiated when loading in visualScenes")]
    private GameObject changeSceneButtonPre;
    //List of instantiated buttons, used to delete them when done
    private List<GameObject> changeSceneButtons = new List<GameObject>();



    //Called by VisualManger on LoadScene(), to generate new scene link buttons
    public void LoadSceneLinkButtons()
    {
        DestroyAllLinkButtons();

        VisualScene activeScene = visualManager.GetActiveScene();

        List<VisualScene> links = activeScene.GetPathLinks();
        List<Vector3> linkPositions = activeScene.GetPathPositions();

        for (int i = 0; i < links.Count; i++)
        {
            GameObject newButton = Instantiate(changeSceneButtonPre, canvasTransform);
            changeSceneButtons.Add(newButton);

            RectTransform newButtonRect = newButton.GetComponent<RectTransform>();
            Button button = newButton.GetComponent<Button>();
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();

            // Convert the off-screen world position to screen space
            Vector3 screenPos = cam.WorldToScreenPoint(linkPositions[i]);

            //Margin to add some space between the screen bounds
            float margin = 50;
            screenPos.x = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
            screenPos.y = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);

            // Convert the clamped screen position to a local position in the Canvas
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform as RectTransform, // The RectTransform of your Canvas
                screenPos,                        // The screen position to convert
                null,                             // Pass null for the Camera parameter in Screen Space - Overlay mode
                out localPos                      // The resulting local position
            );

            // Set the button's position in the Canvas
            newButtonRect.anchoredPosition = localPos;

            // Assign the onClick functionality
            int index = i; // Capture the loop variable, to prevent errors
            button.onClick.AddListener(() => visualManager.ChangeScene(links[index].sceneIndex));

            //change the button text
            buttonText.text = links[i].name;

            //Put the button in a container
            newButton.transform.SetParent(buttonParent.transform);
        }

    }
    //Destroys all the buttons in the scene
    public void DestroyAllLinkButtons()
    {
        foreach (GameObject obj in changeSceneButtons)
        {
            Destroy(obj);
        }
        changeSceneButtons.Clear();
    }
    //calls the active body section's function when a button is pressed
    public void CallBodySectionFunction(int type)
    {
        int activeScene = visualManager.activeScene;
        sectionSim = simulation.Sections[activeScene];

        if (sectionSim == null)
        {
            Debug.Log("Could not find BodySectionSimulation");
            return;
        }

        switch (type)
        {
            case 0:
                sectionSim.Alarm();
                break;
            case 1:
                sectionSim.Escalate();
                break;
            case 2:
                sectionSim.Deescalate();
                break;
            case 3:
                sectionSim.Scan();
                break;
            default:
                Debug.Log("Invalid int argument for CallBodySectionFunction: " +  type);
                break;
        }
    }




}
