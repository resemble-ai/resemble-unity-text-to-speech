using UnityEngine;

public class SpaceToPlay : MonoBehaviour
{
    public Animator anim;
    public new AudioSource audio;

    void Update()
    {
        if (!audio.isPlaying && Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            audio.Play();
            anim.SetTrigger("Talk");
        }
    }
}
