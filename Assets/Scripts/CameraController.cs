using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Rigidbody2D Player;
    public float zDepth = -5;
    public float maxLen = 10;
    public float mass = 10;
    public float damping = 0.95f;
    public float dftFollowFactor = 0.02f;
    public float longFollowFactor = 0.5f;

    private Vector2 acc = Vector2.zero;
    private Vector2 vel = Vector2.zero;
    

    private void FixedUpdate()
    {
        Vector2 tgtPos = Player.position;
        Vector2 curPos = transform.position;
        Vector2 delta = tgtPos - curPos;
        float dLen = delta.magnitude;
        float followFactor = dftFollowFactor;
        if (dLen >= maxLen)
            followFactor = longFollowFactor;

        Vector2 tmpForce = delta * followFactor;
        acc.x = tmpForce.x / mass;
        acc.y = tmpForce.y / mass;
        vel += acc;
        curPos += vel;
        curPos *= damping;
        transform.position = new Vector3(curPos.x, curPos.y, zDepth);



    }
}
