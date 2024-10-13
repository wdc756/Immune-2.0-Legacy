using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public SimulationManager Manager;
    public BodySimulation Body;

    public TMP_InputField ticksInput;
    public Slider stressSlider;
    public Slider responseSlider;
    public Slider progressSlider;


    public void UpdateTickNumber()
    {
        //Debug.Log("Updating number");
        Manager.tickTotal = int.Parse(ticksInput.text);
        Manager.ticks = int.Parse(ticksInput.text);
    }

    void FixedUpdate()
    {
        stressSlider.value = Body.firstSection.StressLevelPercent;
        progressSlider.value = Body.firstSection.InfectionProgressPercent;
        Body.firstSection.Response.LevelPercent = responseSlider.value;
    }

}
