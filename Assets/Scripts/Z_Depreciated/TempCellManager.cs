using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempCellManager : MonoBehaviour
{
    public Slider responseSlider;
    public Slider infectionSlider;
    public int responseType;

    public CellManager cellManager;
    public VisualManager visualManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        visualManager.ReceiveTestingNumbers(responseSlider.value, infectionSlider.value, responseType);
    }
}
