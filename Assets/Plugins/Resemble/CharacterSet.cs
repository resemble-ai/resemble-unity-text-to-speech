using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterSet", menuName = "Ressemble Character Set", order = 51), System.Serializable]
public class CharacterSet : ScriptableObject
{
    public string voice;
    public Tuning pitch = Tuning.Medium;
    public Tuning speed = Tuning.Medium;
    public List<Pod> pods = new List<Pod>();

    public enum Tuning
    {
        XLow,
        Low,
        Medium,
        High,
        XHigh
    }
}