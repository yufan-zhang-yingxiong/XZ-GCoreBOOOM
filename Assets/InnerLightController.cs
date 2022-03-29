using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.Universal;//introduce Light2D access
using UnityEngine;

public class InnerLightController : MonoBehaviour
{
    private SpriteRenderer spr;
    private float lerpAmp = 0;
    private float tgtAmp = 0;

    public Light2D circleLight;
    public Color idleColor = Color.black;
    public Color GlowColor = Color.blue;    

    // Start is called before the first frame update
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    public void Glow(bool bGlow)
    {
        if(bGlow)
        {
            tgtAmp = 1;
        }
        else
        {
            tgtAmp = 0;
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        lerpAmp += (tgtAmp - lerpAmp) * .2f;
        spr.color = Color.Lerp(idleColor, GlowColor, lerpAmp);
        circleLight.intensity = lerpAmp;
    }
}
