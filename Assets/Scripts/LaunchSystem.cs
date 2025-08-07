using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaunchSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<Transform> positions;
    [SerializeField] private Rigidbody ball;
    [SerializeField] private Transform target;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image perfectZone;
    [SerializeField] private Image backboardZone;

    [Header("Settings")]
    [SerializeField] private float h = 2.8f;
    [SerializeField] private float gravity = -18f;

    // PERFECT ZONE VALUE
    private float minPerfect; // Minimum value on the slider
    private float maxPerfect; // Maximum value on the slider
    
    // BACKBOARD ZONE VALUE
    private float minBackboard; // Minimum value on the slider
    private float maxBackboard; // Maximum value on the slider
    
    private float distanceMagnitude; // Distance betweet ball and basket
    private int posIndex = 0; // Shooting position index
    private bool isMobile; // Device check

    public void InitLaunch()
    {
        SetSpherePosition();
    }

    public void SetZone(Transform pos)
    {
        Vector3 distance = new Vector3(pos.position.x - ball.position.x, 0, pos.position.z - ball.position.z);
        distanceMagnitude = distance.magnitude;
        distanceMagnitude = Mathf.Clamp01(distanceMagnitude / 10f);
        
        Debug.Log("Name: " + pos.name + "Basket distance: " + distanceMagnitude);
        
        // Set the anchor for the Perfect Zone
        minPerfect = Mathf.Clamp01(distanceMagnitude-0.07f);
        maxPerfect = Mathf.Clamp01(distanceMagnitude+0.07f);
        perfectZone.rectTransform.anchorMin = new Vector2(0f, minPerfect);
        perfectZone.rectTransform.anchorMax = new Vector2(1f, maxPerfect);
        perfectZone.rectTransform.sizeDelta = new Vector2(0f, 0f);

        // Set the anchor for the Backboard Zone
        minBackboard = (maxPerfect + 0.1f);
        maxBackboard = (minBackboard+0.1f);
        backboardZone.rectTransform.anchorMin = new Vector2(0f, minBackboard);
        backboardZone.rectTransform.anchorMax = new Vector2(1f, maxBackboard);
        backboardZone.rectTransform.sizeDelta = new Vector2(0f,0f);

    }

    public void CheckZone(float t)
    {
        if (t > minPerfect && t < maxPerfect)
        {
            if (t < 0.50f)
                h = 1.8f;
            else if (t > 0.50 && t < 0.60f)
                h = 2.4f;
            else if (t > 0.60f)
                h = 2.6f;
            PerfectShot();
        }
        else
        {
            if (t < minPerfect)
            {
                PerfectShot();
                ball.linearVelocity -= AddError(ball.linearVelocity.normalized);
            } else if (t > maxPerfect)
            {
                PerfectShot();
                ball.linearVelocity += AddError(ball.linearVelocity.normalized);
            }
            Debug.Log("a cojone ma come fai a manca er verde");
        }
    }

    private void PerfectShot()
    {
        Physics.gravity = Vector3.up * gravity;
        ball.useGravity = true;
        ball.linearVelocity = CalculateLaunchData().initialVelocity;
    }

    public Vector3 AddError(Vector3 dir)
    {
        float angleError = Random.Range(-Mathf.PI, Mathf.PI);
        Quaternion rotation = Quaternion.Euler(0, angleError, 0);
        return rotation * dir;
    }
    
    public void SetSpherePosition()
    {
        ball.useGravity = false;

        if (posIndex > positions.Count - 1)
        {
            posIndex = 0;
        }

        SetZone(positions[posIndex]);
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.position = positions[posIndex].position;
        posIndex++;
    }
    
    LaunchData CalculateLaunchData()
    {
        float displacementY = target.position.y - ball.position.y;
        Vector3 displacementXZ = new Vector3(target.position.x - ball.position.x, 0, target.position.z - ball.position.z);
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;
        Vector3 initialVelocity = velocityXZ + velocityY;
    
        return new LaunchData(initialVelocity * -Mathf.Sign(gravity), time);
    }
    
    public void DrawPath()
    {
        LaunchData launchData = CalculateLaunchData();
        Vector3 prevDrawPoint = ball.position;

        int resolution = 30;
        for (int i = 0; i <= resolution; i++)
        {
            float simulationTime = i / (float)resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * (gravity * simulationTime * simulationTime) / 2f;
            Vector3 drawPoint = ball.position + displacement;
            Debug.DrawLine(prevDrawPoint, drawPoint, Color.green);
            prevDrawPoint = drawPoint;
        }
    }
    
    struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }
    }
    
}
