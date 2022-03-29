using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CircleController : MonoBehaviour
{
    public Rigidbody2D Rb;
    private float _mass;
    private void Awake()
    {
        _mass = Rb.mass;
    }
    public void SetMass(float targetAmount, float duration)
    {
        StartCoroutine(SetMassCoroutine(targetAmount, duration));
    }
    private IEnumerator SetMassCoroutine(float targetAmount, float duration)
    {
        Rb.mass = targetAmount;
        yield return new WaitForSeconds(duration);
        Rb.mass = _mass;
    }
}
