using System;
using TMPro;
using UnityEngine;

public class CarEntry : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform vehicle;
    public Transform carDoor;
    public FPVCam CameraController;
    public Transform driverSeat;

    [Header("UI")]
    public TextMeshProUGUI carEntry;

    private bool isInVehicle = false;
    private Rigidbody playerRb;

    void Start()
    {
        carEntry.gameObject.SetActive(false);
        playerRb = player.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isInVehicle)
        {
            HandlePlayerEntry();
        }
        else
        {
            HandlePlayerExit();
        }
    }

    private void HandlePlayerEntry()
    {
        if (Vector3.Distance(player.position, carDoor.position) < 3f)
        {
            carEntry.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.F))
            {
                carEntry.gameObject.SetActive(false);
                EnterVehicle();
            }
        }
        else
        {
            carEntry.gameObject.SetActive(false);
        }
    }

    private void HandlePlayerExit()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ExitVehicle();
        }
    }

    private void EnterVehicle()
    {
        isInVehicle = true;

        // Disable player movement
        player.GetComponent<PlayerCont>().enabled = false;
        playerRb.detectCollisions = false;

        // Parent player to driver seat
        player.SetParent(driverSeat);
        player.localPosition = Vector3.zero;
        player.localRotation = Quaternion.identity;

        // Lock physics while in seat
        playerRb.isKinematic = true;
        playerRb.constraints = RigidbodyConstraints.FreezeAll;

        // Enable vehicle controls
        vehicle.GetComponent<CarController>().enabled = true;
    }

    private void ExitVehicle()
    {
        isInVehicle = false;
        player.GetComponent<PlayerCont>().enabled = true;
        playerRb.detectCollisions = true;

        // Unparent player
        player.SetParent(null);

        // Unlock physics and movement
        playerRb.isKinematic = false;
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        player.GetComponent<PlayerCont>().enabled = true;
        vehicle.GetComponent<CarController>().enabled = false;

        // Move player slightly outside the vehicle
        player.position = vehicle.position + vehicle.transform.right * 2f;

    }
}
