using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempUIHandler : MonoBehaviour
{

    public Slider responseSlider;
    public Slider infectionSlider;

    public CellManager cellManager;

    // Update is called once per frame
    void Update()
    {
        cellManager.RecieveSimulationNumbers(responseSlider.value, infectionSlider.value);
    }
}
