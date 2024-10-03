using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmuneSystemResponse
{
    public float LevelPercent {  get; private set; }
    public ResponseType Type { get; private set; }

    public enum ResponseType
    {
        MACNEUTRO,
        COMPLIMENT,
        KILLERT,
        ANTIBODIES
    }

    // Escalate increases the immune response level by the specified delta
    public void Escalate(float delta)
    {
        LevelPercent += delta;
    }

    // Deescalate decreases the immune response level by the specified delta
    public void Deescalate(float delta)
    {
        LevelPercent -= delta;
    }

    public ImmuneSystemResponse(ResponseType type, float level)
    {
        Type = type;
        LevelPercent = level;
    }

    public override string ToString()
    {
        return $"{Type}: {LevelPercent}";
    }
}
