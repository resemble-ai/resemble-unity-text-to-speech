using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pod", menuName = "Pod", order = 51), System.Serializable]
public class CharacterSet : ScriptableObject
{

    public string url;
    public new string name;
    public string oldText;
    public PodText text;
}
