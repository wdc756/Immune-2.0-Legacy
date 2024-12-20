using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private GameManager gameManager;

    [Header("Basic tutorial vars")]
    public int activeStage = 0;
    /*
     0: Waiting for activation
    1: Escalate
    2: movement
    3: DeEscalate
    4: Alarm
    5: Scan
    6: DeEscalate (again)
    7: Start Game
     */
    public bool tryingToStartNextStage = false;

    [SerializeField, Tooltip("Vertical Max(absolute value)")]
    private float verticalMax = 5;
    [SerializeField, Tooltip("Horizontal Max(absolute value)")]
    private float horizontalMax = 9;

    public Vector3 path1;
    public Vector3 path2;

    public GameObject civilianPrefab;
    public List<Cell> civilians = new List<Cell>();

    public GameObject macrophagePrefab;
    public List<Cell> macrophages = new List<Cell>();
    public GameObject neutrophilePrefab;
    public List<Cell> neutrophiles = new List<Cell>();
    public GameObject dendriticPrefab;
    public List<Cell> dendritics = new List<Cell>();

    public GameObject bacteriaPrefab;
    public List<Cell> bacteria = new List<Cell>();



    public GameObject openScene2Button;
    public GameObject openScene3Button;
    public GameObject openGameButton;



    [Header("Toolbar UI")]

    public GameObject toolbar_1;
    public GameObject escalateButtonObject;
    private float escalateTime;
    public float escalateLockoutTime;

    public GameObject toolbar_2;
    public GameObject deescalateButtonObject;
    private float deescalateTime;
    public float deescalateLockoutTime;

    public GameObject toolbar_3;
    public GameObject alarmButtonObject;
    private float alarmTime;
    public float alarmLockoutTime;

    public GameObject toolbar_4;
    public GameObject scanButtonObject;
    private float scanTime;
    public float scanLockoutTime;

    public Color failColor;
    public Color escalateColor;
    public Color deescalateColor;



    public void StartTutorial(GameManager gameM)
    {
        gameManager = gameM;

        StartStage1();
    }
    void StartStage1()
    {
        tryingToStartNextStage = false;
        activeStage = 1;

        toolbar_1.SetActive(true);

        macrophages.Add(Instantiate(macrophagePrefab, path1, Quaternion.identity).GetComponent<Cell>());
        macrophages[0].ActivateCell(path1);
        macrophages[0].NewTask(1, GetRandomPosition());

        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[0].ActivateCell(path2);
        bacteria[0].NewTask(1, GetRandomPosition());

        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[1].ActivateCell(path2);
        bacteria[1].NewTask(1, GetRandomPosition());
    }
    void StartStage2()
    {
        tryingToStartNextStage = false;
        activeStage = 2;

        toolbar_1.SetActive(false);

        bacteria[0].NewTask(1, path2);
        bacteria[0].NewTask(12);

        macrophages[0].NewTask(1, path2);
        macrophages[0].NewTask(12);

        tryingToStartNextStage = true;
    }
    void StartStage3()
    {
        tryingToStartNextStage = false;
        activeStage = 3;

        bacteria.Add(Instantiate(bacteriaPrefab, path1, Quaternion.identity).GetComponent<Cell>());
        bacteria[0].ActivateCell(path1);
        bacteria[0].NewTask(1, GetRandomPosition());

        civilians.Add(Instantiate(civilianPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<Cell>());
        civilians[0].ActivateCell(GetRandomPosition());
        civilians.Add(Instantiate(civilianPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<Cell>());
        civilians[1].ActivateCell(GetRandomPosition());
        civilians.Add(Instantiate(civilianPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<Cell>());
        civilians[2].ActivateCell(GetRandomPosition());
        civilians.Add(Instantiate(civilianPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<Cell>());
        civilians[3].ActivateCell(GetRandomPosition());
        civilians.Add(Instantiate(civilianPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<Cell>());
        civilians[4].ActivateCell(GetRandomPosition());
        civilians.Add(Instantiate(civilianPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<Cell>());
        civilians[5].ActivateCell(GetRandomPosition());

        macrophages.Add(Instantiate(macrophagePrefab, path1, Quaternion.identity).GetComponent<Cell>());
        macrophages[0].ActivateCell(path1);
        macrophages[0].NewTask(1, GetRandomPosition());
        macrophages.Add(Instantiate(macrophagePrefab, path2, Quaternion.identity).GetComponent<Cell>());
        macrophages[1].ActivateCell(path2);
        macrophages[1].NewTask(14, civilians[0].gameObject);

        neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
        neutrophiles[0].ActivateCell(path2);
        neutrophiles[0].NewTask(14, bacteria[0].gameObject);
        neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
        neutrophiles[1].ActivateCell(path2);
        neutrophiles[1].NewTask(14, civilians[1].gameObject);
    }
    void StartStage4()
    {
        activeStage = 4;
        tryingToStartNextStage = false;

        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[0].ActivateCell(path2);
        bacteria[0].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[1].ActivateCell(path2);
        bacteria[1].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[2].ActivateCell(path2);
        bacteria[2].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[3].ActivateCell(path2);
        bacteria[3].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[4].ActivateCell(path1);
        bacteria[4].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[5].ActivateCell(path1);
        bacteria[5].NewTask(1, GetRandomPosition());
    }
    void StartStage5()
    {
        activeStage = 5;
        tryingToStartNextStage = false;

        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[0].ActivateCell(path2);
        bacteria[0].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[1].ActivateCell(path2);
        bacteria[1].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[2].ActivateCell(path2);
        bacteria[2].NewTask(1, GetRandomPosition());

        toolbar_4.SetActive(true);
    }
    void StartStage6()
    {
        activeStage = 6;
        tryingToStartNextStage = false;

        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[1].ActivateCell(path2);
        bacteria[1].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[2].ActivateCell(path2);
        bacteria[2].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[3].ActivateCell(path2);
        bacteria[3].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[4].ActivateCell(path2);
        bacteria[4].NewTask(1, GetRandomPosition());
        bacteria.Add(Instantiate(bacteriaPrefab, path2, Quaternion.identity).GetComponent<Cell>());
        bacteria[5].ActivateCell(path2);
        bacteria[5].NewTask(1, GetRandomPosition());

        macrophages[0].NewTask(15, bacteria[0].gameObject);
        macrophages[1].NewTask(15, bacteria[1].gameObject);

        neutrophiles[0].NewTask(14, bacteria[2].gameObject);

        neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
        neutrophiles[1].ActivateCell(path1);
        neutrophiles[1].NewTask(14, bacteria[3].gameObject);
        neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
        neutrophiles[2].ActivateCell(path1);
        neutrophiles[2].NewTask(14, bacteria[4].gameObject);
        neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
        neutrophiles[3].ActivateCell(path1);
        neutrophiles[3].NewTask(14, bacteria[5].gameObject);
    }
    void StartStage7()
    {
        Destroy(civilians[3].gameObject);
        Destroy(civilians[2].gameObject);
        Destroy(civilians[1].gameObject);
        Destroy(civilians[0].gameObject);

        openGameButton.SetActive(true);
    }



    void Update()
    {
        UpdateActionButtons();
        UpdatePersistCells();

        if (tryingToStartNextStage && activeStage == 1 && bacteria.Count == 1)
        {
            StartStage2();
        }
        else if (tryingToStartNextStage && activeStage == 2 && bacteria.Count == 0 && macrophages.Count == 0)
        {
            openScene2Button.SetActive(true);
            tryingToStartNextStage = false;
        }
        else if (!tryingToStartNextStage && activeStage == 3 && civilians.Count < 5 && bacteria.Count == 0)
        {
            toolbar_2.SetActive(true);
        }
        else if (tryingToStartNextStage && activeStage == 3 && macrophages.Count == 1 && neutrophiles.Count == 0)
        {
            toolbar_2.SetActive(false);
            StartStage4();
            toolbar_3.SetActive(true);
        }
        else if (tryingToStartNextStage && deescalateTime == 0 && toolbar_2.activeInHierarchy)
        {
            toolbar_2.SetActive(false);
        }
        else if (tryingToStartNextStage && alarmTime == 0 && toolbar_3.activeInHierarchy)
        {
            toolbar_3.SetActive(false);
        }
        else if (tryingToStartNextStage && activeStage == 4 && bacteria.Count == 0)
        {
            StartStage5();
        }
        else if (tryingToStartNextStage && scanTime == 0 && toolbar_4.activeInHierarchy)
        {
            toolbar_4.SetActive(false);
        }
        else if (tryingToStartNextStage && activeStage == 5 && bacteria.Count == 1)
        {
            toolbar_4.SetActive(false);
            StartStage6();
        }
        else if (!tryingToStartNextStage && activeStage == 6 && bacteria.Count == 0)
        {
            toolbar_2.SetActive(true);
        }
        else if (!tryingToStartNextStage && activeStage == 6 && neutrophiles.Count == 0 && macrophages.Count == 1)
        {
            macrophages[0].NewTask(1, path2);
            macrophages[0].NewTask(12);

            tryingToStartNextStage = true;
        }
        else if (tryingToStartNextStage && activeStage == 6 && neutrophiles.Count == 0 && macrophages.Count == 1)
        {
            macrophages[0].NewTask(1, path2);
            macrophages[0].NewTask(12);

            openScene3Button.SetActive(true);
        }
    }



    void Escalate()
    {
        if (activeStage == 1 && !tryingToStartNextStage)
        {
            macrophages[0].ClearTasks();
            macrophages[0].NewTask(15, bacteria[0].gameObject);

            bacteria[0].ClearTasks();
            bacteria[0].NewTask(1, GetAdjustedPosition(macrophages[0].gameObject.transform.position, 3f));
            tryingToStartNextStage = true;
        }
    }
    void DeEscalate()
    {
        if (activeStage == 3 && !tryingToStartNextStage)
        {
            macrophages[1].NewTask(1, path2);
            macrophages[1].NewTask(12);

            neutrophiles[1].NewTask(1, path2);
            neutrophiles[1].NewTask(12);
            neutrophiles[0].NewTask(1, path1);
            neutrophiles[0].NewTask(12);

            tryingToStartNextStage = true;
        }
        else if (activeStage == 6 && !tryingToStartNextStage)
        {
            macrophages[1].NewTask(1, path1);
            macrophages[1].NewTask(12);

            neutrophiles[3].NewTask(1, path1);
            neutrophiles[3].NewTask(12);
            neutrophiles[2].NewTask(1, path1);
            neutrophiles[2].NewTask(12);
            neutrophiles[1].NewTask(1, path2);
            neutrophiles[1].NewTask(12);
            neutrophiles[0].NewTask(1, path2);
            neutrophiles[0].NewTask(12);

            tryingToStartNextStage = true;
        }
    }
    bool Alarm()
    {
        if (activeStage == 4 && !tryingToStartNextStage)
        {
            macrophages.Add(Instantiate(macrophagePrefab, path1, Quaternion.identity).GetComponent<Cell>());
            macrophages[1].ActivateCell(path1);
            macrophages[1].NewTask(15, bacteria[0].gameObject);
            macrophages.Add(Instantiate(macrophagePrefab, path1, Quaternion.identity).GetComponent<Cell>());
            macrophages[2].ActivateCell(path1);
            macrophages[2].NewTask(15, bacteria[5].gameObject);
            macrophages[2].NewTask(1, path2);
            macrophages[2].NewTask(12);

            neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
            neutrophiles[0].ActivateCell(path2);
            neutrophiles[0].NewTask(14, bacteria[1].gameObject);
            neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
            neutrophiles[1].ActivateCell(path1);
            neutrophiles[1].NewTask(14, bacteria[2].gameObject);
            neutrophiles[1].NewTask(1, path1);
            neutrophiles[1].NewTask(12);
            neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
            neutrophiles[2].ActivateCell(path1);
            neutrophiles[2].NewTask(14, bacteria[3].gameObject);
            neutrophiles[2].NewTask(1, path1);
            neutrophiles[2].NewTask(12);
            neutrophiles.Add(Instantiate(neutrophilePrefab, path2, Quaternion.identity).GetComponent<Cell>());
            neutrophiles[3].ActivateCell(path2);
            neutrophiles[3].NewTask(14, bacteria[4].gameObject);
            neutrophiles[3].NewTask(1, path2);
            neutrophiles[3].NewTask(12);

            tryingToStartNextStage = true;

            return true;
        }
        return false;
    }
    void Scan()
    {
        if (activeStage == 5)
        {
            dendritics.Add(Instantiate(dendriticPrefab, path1, Quaternion.identity).GetComponent<Cell>());
            dendritics[0].ActivateCell(path1);
            dendritics[0].NewTask(15, bacteria[0].gameObject);
            dendritics[0].NewTask(1, path1);
            dendritics[0].NewTask(12);
            dendritics.Add(Instantiate(dendriticPrefab, path1, Quaternion.identity).GetComponent<Cell>());
            dendritics[1].ActivateCell(path1);
            dendritics[1].NewTask(15, bacteria[1].gameObject);
            dendritics[1].NewTask(1, path1);
            dendritics[1].NewTask(12);

            tryingToStartNextStage = true;
        }
    }



    public void OpenScene2()
    {
        if (activeStage == 2)
        {
            openScene2Button.SetActive(false);
            StartStage3();
        }
    }
    public void OpenScene3()
    {
        if (activeStage == 6)
        {
            openScene3Button.SetActive(false);
            StartStage7();
        }
    }
    public void StartGame()
    {
        gameManager.StartGame();
    }



    void UpdatePersistCells()
    {
        for (int i = civilians.Count - 1; i >= 0; i--)
        {
            if (civilians[i] == null)
            {
                civilians.RemoveAt(i);
            }
            else
            {
                civilians[i].ResetPersistTime();
            }
        }

        for (int i = macrophages.Count - 1; i >= 0; i--)
        {
            if (macrophages[i] == null)
            {
                macrophages.RemoveAt(i);
            }
            else
            {
                macrophages[i].ResetPersistTime();
            }
        }

        for (int i = neutrophiles.Count - 1; i >= 0; i--)
        {
            if (neutrophiles[i] == null)
            {
                neutrophiles.RemoveAt(i);
            }
            else
            {
                neutrophiles[i].ResetPersistTime();
            }
        }

        for (int i = dendritics.Count - 1; i >= 0; i--)
        {
            if (dendritics[i] == null)
            {
                dendritics.RemoveAt(i);
            }
            else
            {
                dendritics[i].ResetPersistTime();
            }
        }

        for (int i = bacteria.Count - 1; i >= 0; i--)
        {
            if (bacteria[i] == null)
            {
                bacteria.RemoveAt(i);
            }
            else
            {
                bacteria[i].ResetPersistTime();
            }
        }
    }




    public void CallBodySectionFunction(int type)
    {
        switch (type)
        {
            case 0:
                //just a quick reference
                GameObject fillAreaObject = null;

                if (alarmTime == 0)
                {
                    fillAreaObject = alarmButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    fillAreaObject.SetActive(true);

                    if (Alarm())
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

                    Escalate();
                    escalateTime = escalateLockoutTime;
                }
                break;
            case 2:
                if (deescalateTime == 0)
                {
                    fillAreaObject = deescalateButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    fillAreaObject.SetActive(true);

                    DeEscalate();
                    deescalateTime = deescalateLockoutTime;
                }
                break;
            case 3:
                if (scanTime == 0)
                {
                    fillAreaObject = scanButtonObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                    fillAreaObject.SetActive(true);

                    Scan();
                    scanTime = scanLockoutTime;
                }
                break;
        }
    }
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



    private Vector3 GetRandomPosition()
    {
        Vector3 newPosition = new Vector3();

        newPosition.x = Random.Range(-horizontalMax, horizontalMax);
        newPosition.y = Random.Range(-verticalMax, verticalMax);

        return newPosition;
    }
    public Vector3 GetAdjustedPosition(Vector3 currentPosition, float maxDistance)
    {
        float newDistance = Random.Range(maxDistance / 3, maxDistance);

        Vector3 newPosition = new Vector3();

        newPosition.x = Mathf.Clamp(currentPosition.x + Random.Range(-newDistance, newDistance), -horizontalMax, horizontalMax);
        newPosition.y = Mathf.Clamp(currentPosition.y + Random.Range(-newDistance, newDistance), -verticalMax, verticalMax);

        return newPosition;
    }
}
