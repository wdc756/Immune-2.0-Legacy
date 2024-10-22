using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBodyPart : MonoBehaviour
{
    public BodySectionSimulation Section;

    public Slider stressSlider;
    public Slider responseSlider;
    public Slider progressSlider;

    public TMP_Text responseText;

    void FixedUpdate()
    {
        stressSlider.value = Section.StressLevelPercent;
        progressSlider.value = Section.InfectionProgressPercent;
        responseSlider.value = Section.Response.LevelPercent;

        responseText.text = "Current Response: " + Section.Response.Type.ToString();
    }
}
