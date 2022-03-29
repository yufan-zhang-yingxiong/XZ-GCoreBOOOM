using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clip_CrystalClick;
    public AudioClip clip_Jump;
    public AudioClip clip_TorchOn;
    public AudioClip clip_TorchOff;

    public float vlm_CrystalClick = 0.1f;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void hit(float factor)
    {
        source.volume = vlm_CrystalClick * factor;
        source.PlayOneShot(clip_CrystalClick);
    }

    public void jump(float factor)
    {
        source.volume =  0.1f * factor;
        source.PlayOneShot(clip_Jump);
    }

    public void torchOn(float factor)
    {
        source.volume = 0.1f * factor;
        source.PlayOneShot(clip_TorchOn);
    }

    public void torchOff(float factor)
    {
        source.volume = 0.1f * factor;
        source.PlayOneShot(clip_TorchOff);
    }
}
