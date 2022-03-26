using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;//introduce Light2D access
using UnityEngine.InputSystem;

public class Light2DController : MonoBehaviour
{
    private new Light2D light;
    private bool bFocus = false;
    private Vector2 dir = Vector2.zero;
    private float rotAngle = 0f;
    private CameraController camCtrl;

    public Camera cam;
    public float focusDistance = 2;

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
        if (context.performed)
            bFocus = !bFocus;
    }

    public void UnFocus(InputAction.CallbackContext context)
    {
        if (context.performed)
            bFocus = !bFocus;
    }

    public void SetDir(InputAction.CallbackContext context)
    {
        if(bFocus)
        {
            dir = context.ReadValue<Vector2>();
            setRotAngle(dir);
        }
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
        if(focus)
        {
            light.pointLightInnerAngle -= (light.pointLightInnerAngle - focusAng) * 0.2f;
            light.pointLightOuterAngle -= (light.pointLightOuterAngle - focusAng) * 0.2f;
            light.pointLightInnerRadius += (focusRadIn - light.pointLightInnerRadius) * 0.2f;
            light.pointLightOuterRadius += (focusRadOut - light.pointLightOuterRadius) * 0.2f;
            light.intensity += (focusIts - light.intensity) * 0.2f;
        }
        else
        {
            light.pointLightInnerAngle += (idleAng - light.pointLightInnerAngle) * 0.2f;
            light.pointLightOuterAngle += (idleAng - light.pointLightOuterAngle) * 0.2f;
            light.pointLightInnerRadius -= (light.pointLightInnerRadius - idleRadIn) * 0.2f;
            light.pointLightOuterRadius -= (light.pointLightOuterRadius - idleRadOut) * 0.2f;
            light.intensity -= (light.intensity - idleIts) * 0.2f;
        }
    }

    private void FixedUpdate()
    {
        Vector3 tmpRot = transform.eulerAngles;
        tmpRot.z = rotAngle;
        transform.eulerAngles = tmpRot;
        if (bFocus)
        {
            setRotAngle(dir);//用旧的坐标更新手电指向，用于当鼠标不动、Player移动而camera还没有跟上时的手电方向更新
            focusTorch(true);

            //force镜头移动到对应方向一定距离外
            Vector3 curPos = transform.position;
            Vector2 scnPos = cam.WorldToScreenPoint(curPos);
            Vector2 camDir = dir - scnPos;
            camCtrl.ForceTgtPos(new Vector2(curPos.x, curPos.y) + focusDistance * camDir.normalized);
        }
        else
        {
            focusTorch(false);
            camCtrl.ReleaseTgtPos();
            dir = Vector2.zero;
        }
    }
}
