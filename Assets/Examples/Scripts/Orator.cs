using System.Collections.Generic;
using UnityEngine;
using Resemble;

public class Orator : MonoBehaviour
{

    public Speech mySpeech;

    // Play the audio clip with the name of clipName
    public void PlayDialogue(string clipName)
    {
        AudioClip audioClip = mySpeech.GetAudio(clipName);
        AudioSource.PlayClipAtPoint(audioClip, transform.position);
    }

    // Returns a list containing all choices for the dialog at the given index.
    public List<string> GetDialogueWheelChoices(int dialogueIndex)
    {
        Label label = new Label("Dialogue", dialogueIndex);
        List<Clip> clips = mySpeech.GetClipsWithLabel(label);
        List<string> choiceName = new List<string>();
        foreach (var clip in clips)
            choiceName.Add(clip.userdata);
        return choiceName;
    }
}
