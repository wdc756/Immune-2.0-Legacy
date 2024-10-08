using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodySectionSimulation : Simulated
{
    /*
     * BodySectionSimulation stores all of the essential data for a body part's
     * state, including the stress level (in percent) and infection progress (in percent)
     * 
     * BodySectionSimulation also contains the tick function for calculating these numbers
     * based on current immune response conditions and present pathogens, if any.
     * The BodySimulation (haven't written that yet) will simply dispatch these
     * tick functions and also control the immune responses and whatnot
     * 
     * i tried to make it readable and comprehesible but i can only do so much
     */

    public ImmuneSystemResponse Response;
    public float ResponseDefaultLevelPercent = 10f;
    public Pathogen Infection;

    public float InfectionProgressPercent { get; private set; }
    public float StressLevelPercent {  get; private set; }

    void Start()
    {
        ChangeResponse(ImmuneSystemResponse.ResponseType.MACNEUTRO, 10f);
        //Debug.Log($"Min: {StressLevelScalingSigmoid(0f)} Max: {StressLevelScalingSigmoid(1f)}");
    }

    public void Escalate(float delta)
    {
        Response.Escalate(delta);
    }

    public void Deescalate(float delta)
    {
        Response.Deescalate(delta);
    }

    public void ChangeResponse(ImmuneSystemResponse.ResponseType responseType, float responseLevel)
    {
        Response = new ImmuneSystemResponse(responseType, responseLevel);
    }

    // Gain and Loss are the percent change per tick that is calculated
    float gain, loss;
    // Stress ratio is a 0-1 float that is based off of the immune response level and progress
    // and whatnot, it is run through a sigmoid function that translates it into a stress percentage change
    float stressratio;
    public override void Tick()
    {
        Debug.Log($"Running tick for bodysection {this.name} pathogen {Infection.name}");

        gain = Infection.GainRatePercent;
        loss = Infection.MaxLossRatePercent * (Response.LevelPercent / 100f);
        if (Infection.ResponseWeakness == Response.Type) loss *= Infection.WeaknessFactor;

        InfectionProgressPercent += gain - loss;

        // Right now stress level response is based on immune response level alone.
        // We can add small penalties for other conditions that factor in
        stressratio = Response.LevelPercent / 100f; 

        StressLevelPercent += StressLevelBalancingFunc(stressratio);

        // Min cap the stress level. can't be less than 0 stress
        StressLevelPercent = Mathf.Max(0f, StressLevelPercent);
    }

    public float StressFactor = 0.5f;

    private float StressLevelBalancingFunc(float ratio)
    {
        // Sigmoid (not good)
        /*
        float a = 2.2f;
        float b = 3.6f;
        float d = -0.2f;
        float l = -0.5f;

        float c = a * l;

        float denominator = 1 + Mathf.Exp(-b * (ratio + d));
        float result = a / denominator + c;
        */

        // Quadratic (increasing stress cost delta as response increases)
        float a = 6.4f;
        float c = -0.6f;

        float result = a * (ratio * ratio) + c;
        return result * StressFactor;
    }

}
