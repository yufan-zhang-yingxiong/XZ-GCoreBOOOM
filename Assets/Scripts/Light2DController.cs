using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;//introduce Light2D access
using UnityEngine.InputSystem;

public class Light2DController : MonoBehaviour
{
    private new Light2D light;
    private bool bFocus = false;
    private bool bJump = false;
    private Vector2 dir = Vector2.zero;
    private float rotAngle = 0f;
    private CameraController camCtrl;
    private float jumpBlink = 0;
    private Vector2 playerPrevVel = Vector2.zero;

    [SerializeField]
    [Header("Camera")]
    public Camera cam;
    public float focusDistance = 2;
    public float blurAng = 30;

    [Header("Audio")]
    public AudioController audioCtrl;

    [Header("Player")]
    public Rigidbody2D player;
    public float accMark = 1.5f;

    [Header("Torch")]
    public float scaleFactor = 0.2f;

    public float blinkIts = 0.8f;

    public float idleAng = 360f;
    public float idleIts = 0.5f;
    public float idleRadOut = 3;
    public float idleRadIn = 0.1f;

    public float focusAng = 30;
    public float focusIts = 0.8f;
    public float focusRadOut = 15;
    public float focusRadIn = 1;

    void Start()
    {
        light = GetComponent<Light2D>();
        camCtrl = cam.GetComponent<CameraController>();
    }

    

    public void Focus(InputAction.CallbackContext context)
    {
        audioCtrl.torchOn(1);
        if (context.performed && !bJump)
        {
            bJump = true;
            jumpBlink += 0.8f;
        }
        if (context.performed)
        {
            bFocus = !bFocus;
            //Vector3 tmp = Input.mousePosition;
            //dir = new Vector2(tmp.x, tmp.y);
            //setRotAngle(dir);
        }
    }

    public void UnFocus(InputAction.CallbackContext context)
    {
        audioCtrl.torchOff(1);
        if (context.performed)
        {
            bFocus = !bFocus;
        }
    }

    public void SetDir(InputAction.CallbackContext context)
    {
        if(bFocus)
        {
            dir = context.ReadValue<Vector2>();
            setRotAngle(dir);
        }
    }

    public void Blink(InputAction.CallbackContext context)
    {
        if (context.performed && !bJump)
        {
            audioCtrl.jump(1);
            TriggerBlink(1);
        }
    }

    public void TriggerBlink(float factor)
    {
        bJump = true;
        jumpBlink += blinkIts * factor;
    }

    private void setRotAngle(Vector2 tgtPos)
    {
        Vector2 mosPos = tgtPos;
        Vector3 curPos = transform.position;
        Vector2 scnPos = cam.WorldToScreenPoint(curPos);       
        Vector2 dir = mosPos - scnPos;
        dir = dir.normalized;
        rotAngle = Mathf.Atan2(dir.y, dir.x) * 180 / 3.1415f - 90;
    }

    private void focusTorch(bool focus)
    {
        if (focus)
        {
            light.pointLightInnerAngle -= (light.pointLightInnerAngle - focusAng) * scaleFactor;
            light.pointLightOuterAngle -= (light.pointLightOuterAngle - (focusAng + blurAng)) * scaleFactor;
            light.pointLightInnerRadius += (focusRadIn - light.pointLightInnerRadius) * scaleFactor;
            light.pointLightOuterRadius += (focusRadOut - light.pointLightOuterRadius) * scaleFactor;
            light.intensity += (focusIts - jumpBlink - light.intensity) * scaleFactor + jumpBlink;
        }
        else if (!focus && !bJump)
        {
            light.pointLightInnerAngle += (idleAng - light.pointLightInnerAngle) * scaleFactor;
            light.pointLightOuterAngle += (idleAng - light.pointLightOuterAngle) * scaleFactor;
            light.pointLightInnerRadius -= (light.pointLightInnerRadius - idleRadIn) * scaleFactor;
            light.pointLightOuterRadius -= (light.pointLightOuterRadius - idleRadOut) * scaleFactor;
            light.intensity -= (light.intensity - idleIts) * scaleFactor;
        }
        else
        {
            light.intensity = idleIts + jumpBlink;
        }
    }

    private void FixedUpdate()
    {
        Vector2 playerCurVel = player.velocity;
        Vector2 playerAcc = playerCurVel - playerPrevVel;
        playerPrevVel = playerCurVel;
        float accAmp = playerAcc.magnitude;
        Debug.Log(accAmp);
        if (accAmp > accMark && !bJump)
        {
            audioCtrl.hit(accAmp - accMark);
            TriggerBlink(0.5f);
        }

        Vector3 tmpRot = transform.eulerAngles;
        tmpRot.z = rotAngle;
        transform.eulerAngles = tmpRot;

        if (bJump)
        {
            jumpBlink -= jumpBlink * scaleFactor;
            if (jumpBlink <= 0.1)
            {
                bJump = false;
                jumpBlink = 0;
            }
        }        

        if (bFocus)
        {
            setRotAngle(dir);//用旧的坐标更新手电指向，用于当鼠标不动、Player移动而camera还没有跟上时的手电方向更新
            

            //force镜头移动到对应方向一定距离外
            Vector3 curPos = transform.position;
            Vector2 scnPos = cam.WorldToScreenPoint(curPos);
            Vector2 camDir = dir - scnPos;
            camCtrl.ForceTgtPos(new Vector2(curPos.x, curPos.y) + focusDistance * camDir.normalized);
        }
        else
        {
            camCtrl.ReleaseTgtPos();
            //dir = Vector2.zero;
        }
        focusTorch(bFocus);
    }
}
