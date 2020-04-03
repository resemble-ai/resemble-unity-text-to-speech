using UnityEngine;
using UnityEngine.Events;

public class SpaceToPlay : MonoBehaviour
{
    public Animator anim;
    public new AudioSource audio;
    public UnityEvent onTrigger;

    void Update()
    {
        if (!audio.isPlaying && Input.GetKeyDown(KeyCode.Space))
            Invoke();
    }

    public void Invoke()
    {
        audio.Play();
        anim.SetTrigger("Talk");
        onTrigger.Invoke();
    }
}
