using UnityEngine;

public class BallCollisionHandler : MonoBehaviour
{
    [SerializeField] private LaunchSystem controller;
    [SerializeField] private CameraController cam;
    [SerializeField] private ManagerUI managerUI;

    public bool isBackboard = false;
    public bool isBasket = false;
    private bool isPerfectBasket = true;

    private void OnCollisionEnter(Collision other)
    {
        string tag = other.gameObject.tag;

        if (tag == "Floor")
        {
            cam.isTrigger = false;
            controller.SetSpherePosition();
        }
        else if ((tag == "Backboard" && isBackboard) || (tag == "Hoop" && isBasket))
        {
            if (tag == "Backboard") isBackboard = false;
            if (tag == "Hoop") isBasket = false;

            controller.currentTarget = controller.GetHoop;
            controller.PerfectShot();
            isPerfectBasket = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        string tag = other.gameObject.tag;

        if (tag == "TriggerCamera")
        {
            cam.isTrigger = true;
        }
        else if (tag == "TriggerBasket")
        {
            managerUI.AddPoints(isPerfectBasket ? 3 : 2);
            isPerfectBasket = true;
        }
    }
}