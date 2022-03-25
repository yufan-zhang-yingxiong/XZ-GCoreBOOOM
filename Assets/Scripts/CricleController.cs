using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CricleController : MonoBehaviour
{

    public float distance = 4f;
    public Rigidbody2D Sqaure;
    public SpringJoint2D joint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector2 pos = transform.position;
        float distToSquare = (Sqaure.position - pos).magnitude;
        if (distToSquare <= distance + 0.01)
        {
            joint.enabled = false;
        }
        else
        {
            joint.enabled = true;
        }
    }
}
