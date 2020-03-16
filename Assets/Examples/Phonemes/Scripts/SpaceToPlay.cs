using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpaceToPlay : MonoBehaviour
{
    private new AudioSource audio;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            audio.Play();
    }
}
