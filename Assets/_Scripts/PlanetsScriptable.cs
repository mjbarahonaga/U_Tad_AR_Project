using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Planets
{
    None,
    Mercury,
    Venus,
    Earth,
    Mars,
    Jupiter,
    Saturn,
    Uranus,
    Neptune,
    Pluto,
    Moon
}

[CreateAssetMenu(fileName = "PlanetData", menuName = "Planets/Data")]
public class PlanetsScriptable : ScriptableObject
{
    public Planets Planet = Planets.None;
    [Tooltip("In Seconds")]
    public float ObitalPeriod = 60.0f;
    [Tooltip("In Seconds")]
    public float RotationPeriod = 0.1643f;  //60 sec /365 (Solar cycle days)
    [TextArea]
    public string Info;
}
