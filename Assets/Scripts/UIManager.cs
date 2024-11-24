using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Tooltip("Reference to the simulation manager, so we can tell the active section when a button is pressed")]
    public BodySimulation simulation;
    [Tooltip("The visual manger, used to get the active scene reference")]
    public VisualManager visualManager;
    //Set on SetUp, used to get the max civilian death count
    private GameManager gameManager;

    //The active simulation section, which will be set as needed on runtime
    private BodySectionSimulation sectionSim;



    [Header("Scene transition buttons")]
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

    [Header("Thymus UI")]
    [SerializeField, Tooltip("The slider showing cell death")]
    private Slider cellDeathSlider;
    [SerializeField, Tooltip("The slider showing resource usage")]
    private Slider resourceSlider;

    [Header("Toolbar UI")]
    public GameObject toolbar;
    [SerializeField]
    private GameObject alarmButtonObject;
    private float alarmTime;
    public float alarmLockoutTime;
    public GameObject escalateButtonObject;
    private float escalateTime;
    public float escalateLockoutTime;
    public GameObject deescalateButtonObject;
    private float deescalateTime;
    public float deescalateLockoutTime;
    public GameObject scanButtonObject;
    private float scanTime;
    public float scanLockoutTime;

    public Color failColor;
    public Color escalateColor;
    public Color deescalateColor;



    //To be called by VisualManager during setup phase
    public void SetUp(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }



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
            float margin = 200; //Leave this as the higher number, because the build needs it
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



    private void Update()
    {
        UpdateActionButtons();
    }



    public void CallBodySectionFunction(int type)
    {
        int activeScene = visualManager.activeScene;
        sectionSim = simulation.Sections[activeScene - 1]; // again the thymus is not a valid section

        if (sectionSim == null)
        {
            Debug.Log("Could not find BodySectionSimulation");
            return;
        }

        switch (type)
        {
            case 0:
                //just a quick reference
                GameObject fillAreaObject = null;

                if (alarmTime == 0)
                {
                    fillAreaObject = alarmButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    fillAreaObject.SetActive(true);

                    if (sectionSim.Alarm())
                    {
                        fillAreaObject.GetComponent<Image>().color = escalateColor;
                    }
                    else
                    {
                        fillAreaObject.GetComponent<Image>().color = failColor;
                    }

                    alarmTime = alarmLockoutTime;
                }
                break;
            case 1:
                if (escalateTime == 0)
                {
                    fillAreaObject = escalateButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    fillAreaObject.SetActive(true);

                    sectionSim.Escalate();
                    escalateTime = escalateLockoutTime;
                }
                break;
            case 2:
                if (deescalateTime == 0)
                {
                    fillAreaObject = deescalateButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    fillAreaObject.SetActive(true);

                    sectionSim.Deescalate();
                    deescalateTime = deescalateLockoutTime;
                }
                break;
            case 3:
                if (scanTime == 0)
                {
                    fillAreaObject = scanButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    fillAreaObject.SetActive(true);

                    sectionSim.Scan();
                    scanTime = scanLockoutTime;
                }
                break;
            default:
                Debug.Log("Invalid int argument for CallBodySectionFunction: " + type);
                break;
        }
    }



    //To control the Thymus UI, called by VisualManager
    public void UpdateThymusUI(int civilianDeathCount, float resourceUsage)
    {
        //update civilian death slider
        cellDeathSlider.maxValue = gameManager.maxCivilianDeathCount;
        cellDeathSlider.value = civilianDeathCount;

        //update resource usage slider
        resourceSlider.value = resourceUsage;
    }

    //controls the player action buttons
    public void UpdateActionButtons()
    {
        if (alarmTime > 0)
        {
            alarmTime -= Time.deltaTime;

            alarmButtonObject.transform.GetChild(0).gameObject.GetComponent<Slider>().value = alarmTime;

            if (alarmTime < 0)
            {
                alarmTime = 0;
                alarmButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
        }

        if (escalateTime > 0)
        {
            escalateTime -= Time.deltaTime;

            escalateButtonObject.transform.GetChild(0).gameObject.GetComponent<Slider>().value = escalateTime;

            if (escalateTime < 0)
            {
                escalateTime = 0;
                escalateButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
        }

        if (deescalateTime > 0)
        {
            deescalateTime -= Time.deltaTime;

            deescalateButtonObject.transform.GetChild(0).gameObject.GetComponent<Slider>().value = deescalateTime;

            if (deescalateTime < 0)
            {
                deescalateTime = 0;
                deescalateButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
        }

        if (scanTime > 0)
        {
            scanTime -= Time.deltaTime;

            scanButtonObject.transform.GetChild(0).gameObject.GetComponent<Slider>().value = scanTime;

            if (scanTime < 0)
            {
                scanTime = 0;
                scanButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
