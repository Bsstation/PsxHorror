using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchBehaviour : MonoBehaviour
{
    public bool testing;

    public Transform target;
    public GameObject ball;

    public float height = 0.25f;

    public bool debugPath;

    Rigidbody currentBall;

    public void Update()
    {
        if (testing)
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (!currentBall)
                {
                    GameObject stone = Instantiate(ball, transform.position, transform.rotation);

                    MeshCollider mesh = stone.AddComponent<MeshCollider>();
                    mesh.convex = true;

                    Rigidbody rigid = stone.GetComponent<Rigidbody>();
                    rigid.isKinematic = true;

                    currentBall = rigid;
                }
                else
                {
                    Launch(currentBall, target);
                    currentBall.isKinematic = false;
                    Destroy(currentBall.gameObject, 5);
                    currentBall = null;
                }
            }
        }

        if (debugPath)
            DrawPath();
    }

    LaunchData CalculateLaunchData(Rigidbody ball, Transform target, float gravity, float height = 0.2f)
    {
        float displacementY = target.position.y - ball.position.y;
        Vector3 displacementXZ = new Vector3(target.position.x - ball.position.x, 0, target.position.z - ball.position.z);

        height = (height < displacementY) ? displacementY + height : height;

        float time = (Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity)) * 0.4f;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / time;

        return new LaunchData (velocityXZ + velocityY * -Mathf.Sign(gravity), time);
    }

    public void DrawPath()
    {
        if (!currentBall) return;

        LaunchData launchData = CalculateLaunchData(currentBall, target, Physics.gravity.y, height);
        Vector3 previousDrawPoint = currentBall.position;

        int resolution = 30;
        for(int i = 1; i < resolution; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * Physics.gravity.y * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = currentBall.position + displacement;

            Debug.DrawLine(previousDrawPoint, drawPoint, Color.red);
            previousDrawPoint = drawPoint;
        }
    }

    public void Launch(Rigidbody ball, Transform target)
    {
        ball.useGravity = true;
        ball.velocity = CalculateLaunchData(ball, target, Physics.gravity.y, height).initialVelocity;
    }

    struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData (Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }
    }
}
