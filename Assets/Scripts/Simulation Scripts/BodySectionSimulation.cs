using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
     * i tried to make it readable and comprehesible but i can only do so much (Trent)
     */
    [Header("Immune Response")]
    public ImmuneSystemResponse Response;

    [Header("Infection")]
    public Pathogen Infection;
    private BodySimulation parent;
    private AdaptiveImmuneSystem AIS;
    public float InfectionProgressPercent { get; private set; }
    public float StressLevelPercent {  get; private set; }

    [Header("(Debug) Escalation/Deescalation")]
    public bool ResponseIsChanging;
    [SerializeField] private float responseChangeTotal; // Total amount of percent change over the whole time period
    [SerializeField] private float responseChangeDelta; // Change in response level per tick
    [SerializeField] private int responseChangeTicks;   // Amount of ticks left to change the response
    [SerializeField] private float targetResponsePercent;   // For floating point "correction"
    [SerializeField] private float previousResponse;   // For floating point "correction"

    public float ResourceDemand { get; private set; }

    [Header("Status")]
    public bool statusResponseChange;
    public bool statusScan;
    public bool statusAlarmLockout;

    [Header("(Debug) Alarm Lockout")]
    [SerializeField] private int lockoutTicks;

    void Start()
    {
        parent = GetComponentInParent<BodySimulation>();
        AIS = GetComponentInParent<AdaptiveImmuneSystem>();
        ChangeResponse(ImmuneSystemResponse.ResponseType.MACNEUTRO, parent.ResponseDefaultLevelPercent);
    }


    public void Escalate()
    {
        if (ResponseIsChanging || Response.LevelPercent + parent.ReponseChangeDelta > 100f) return;
        ResponseIsChanging = true;
        statusResponseChange = true;
        responseChangeTotal = parent.ReponseChangeDelta;
        responseChangeTicks += (int) (parent.ReponseChangeTime / SimulationManager.TickDelta);
        responseChangeDelta = responseChangeTotal / (float)responseChangeTicks;

        targetResponsePercent = Response.LevelPercent + responseChangeTotal;
        previousResponse = Response.LevelPercent;
    }

    public void Deescalate()
    {
        if (ResponseIsChanging || Response.LevelPercent - parent.ReponseChangeDelta < 0f) return;
        ResponseIsChanging = true;
        statusResponseChange = true;
        responseChangeTotal = -parent.ReponseChangeDelta;
        responseChangeTicks += (int)(parent.ReponseChangeTime / SimulationManager.TickDelta);
        responseChangeDelta = responseChangeTotal / (float)responseChangeTicks;

        targetResponsePercent = Response.LevelPercent + responseChangeTotal;
        previousResponse = Response.LevelPercent;
    }

    public void Kickback()
    {
        // Interrupts any other changes in response
        Response.LevelPercent = previousResponse;

        responseChangeTicks = 0;
        responseChangeTotal = 0f;
        responseChangeDelta = 0f;
        ResponseIsChanging = false;

        Deescalate();
    }

    public bool Alarm()
    {
        if (ResponseIsChanging || statusAlarmLockout) return false;
        // Don't do anything if we don't reach the minimum flag percent or we are already responding more than the alarm level
        if (InfectionProgressPercent < parent.AlarmFlagPercent || Response.LevelPercent >= parent.AlarmResponsePercent)
        {
            Debug.Log("Alarm dismissed, did not meet requirements");
            lockoutTicks = (int)(parent.AlarmLockoutTime / SimulationManager.TickDelta);
            statusAlarmLockout = true;
            Debug.Log($"lockout ticks: {lockoutTicks}");
            return false;
        }
        ResponseIsChanging = true;
        
        responseChangeTotal = parent.AlarmResponsePercent - Response.LevelPercent;
        float responseTime = (responseChangeTotal / parent.AlarmResponsePercent) * parent.AlarmMaxResponseTime;
        responseChangeTicks = (int)(responseTime / SimulationManager.TickDelta);
        responseChangeDelta = responseChangeTotal / (float)responseChangeTicks;

        targetResponsePercent = parent.AlarmResponsePercent;

        return true;
    }

    public void Scan()
    {
        Debug.Log($"Scanning section manually {this.gameObject.name}");
        statusScan = true;
        AIS.BeginScan(this, false);
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
        HandleStress();
        HandlePathogen();
        HandleResponse();

        // Calculate the resource demand for this body part
        ResourceDemand = Response.LevelPercent - parent.ResponseDefaultLevelPercent;

        // Lockouts
        if (statusAlarmLockout)
        {
            lockoutTicks--;
            if (lockoutTicks <= 0)
            {
                Debug.Log("Lockout release");
                statusAlarmLockout = false;
            }
        }
    }

    [Header("Debug and stupid")]
    public float StressFactor;

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

    private void HandlePathogen()
    {
        // If there's no infection don't run the simulation logic
        if (Infection == null) return;

        gain = Infection.GainRatePercent;
        loss = Infection.MaxLossRatePercent * (Response.LevelPercent / 100f);
        if (Infection.ResponseWeakness == Response.Type) loss *= Infection.WeaknessFactor;

        InfectionProgressPercent += gain - loss;

        if (InfectionProgressPercent <= 0f)
        {
            Infection = null;       // Remove the infection if we win
            InfectionProgressPercent = 0f;
            // Set default response
            ChangeResponse(ImmuneSystemResponse.ResponseType.MACNEUTRO, parent.ResponseDefaultLevelPercent);
        }
    }
    private void HandleStress()
    {
        // Right now stress level response is based on immune response level alone.
        // We can add small penalties for other conditions that factor in
        stressratio = Response.LevelPercent / 100f;

        StressLevelPercent += StressLevelBalancingFunc(stressratio) * StressFactor;

        // Min cap the stress level. can't be less than 0 stress
        StressLevelPercent = Mathf.Max(0f, StressLevelPercent);

        // Max cap the stress level, it shouldn't be more than 100% stress
        StressLevelPercent = Mathf.Min(StressLevelPercent, 100f);
    }

    private void HandleResponse()
    {
        if (!ResponseIsChanging) return;

        Response.UpdateResponse(responseChangeDelta);
        responseChangeTicks--;

        // Reset everything
        if (responseChangeTicks == 0 || responseChangeTotal == 0f)
        {
            Response.LevelPercent = targetResponsePercent;
            previousResponse = Response.LevelPercent;

            responseChangeTicks = 0;
            responseChangeTotal = 0f;
            responseChangeDelta = 0f;
            ResponseIsChanging = false;
            statusResponseChange = false;
        }
    }

    public void HeavyWeaponsResponse(ImmuneSystemResponse.ResponseType newType)
    {
        // Reset the response changing stuff
        Response.LevelPercent = parent.PostscanResponsePercent;

        responseChangeTicks = 0;
        responseChangeTotal = 0f;
        responseChangeDelta = 0f;
        ResponseIsChanging = false;

        ChangeResponse(newType, parent.PostscanResponsePercent);
    }
}
