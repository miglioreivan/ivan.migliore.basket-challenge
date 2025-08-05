using System;
using UnityEngine;

public class BallCollisionHandler : MonoBehaviour
{
    [SerializeField] private LaunchSystem controller;
    [SerializeField] private CameraController cam;
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Floor"))
        {
            cam.isGoal = false;
            controller.SetSpherePosition();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hoop"))
        {
            cam.isGoal = true;
            Debug.Log("CANESTRO!!! 😊");
        }
    }
}