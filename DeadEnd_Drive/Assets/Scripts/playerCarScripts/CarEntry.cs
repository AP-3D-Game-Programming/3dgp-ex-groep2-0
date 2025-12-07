using System;
using TMPro;
using UnityEngine;
using System.Collections;

public class CarEntry : MonoBehaviour
{
    [Header("Settings")]
    public bool startInsideCar = false;
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

    [Header("Sound")]
    private AudioSource src;
    public AudioClip startEngine;
    public AudioClip driveEngine;

    void Start()
    {
        carEntry.gameObject.SetActive(false);
        playerRb = player.GetComponent<Rigidbody>();
        src = gameObject.GetComponent<AudioSource>();

        if (startInsideCar)
        {
            EnterVehicle();

            StartCoroutine(StartEngineRoutine());
        }
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
                StartCoroutine(StartEngineRoutine());
            }
        }
        else
        {
            carEntry.gameObject.SetActive(false);
        }
    }

    private IEnumerator StartEngineRoutine()
    {

        src.loop = false;

        src.PlayOneShot(startEngine);

        yield return new WaitForSeconds(startEngine.length);


        src.clip = driveEngine;

        src.loop = true;

        src.Play();
    }


    private void HandlePlayerExit()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ExitVehicle();
        }
    }

    public void EnterVehicle()
    {

        if (player == null || driverSeat == null || vehicle == null)
        {
            Debug.LogError("CarEntry references not assigned!");
            return;
        }

        if (playerRb == null)
            playerRb = player.GetComponent<Rigidbody>();
        isInVehicle = true;

        player.GetComponent<PlayerCont>().enabled = false;
        playerRb.detectCollisions = false;

        player.SetParent(driverSeat);
        player.localPosition = Vector3.zero;
        player.localRotation = Quaternion.identity;

        playerRb.isKinematic = true;
        playerRb.constraints = RigidbodyConstraints.FreezeAll;

        Rigidbody carRb = vehicle.GetComponent<Rigidbody>();
        if (carRb != null)
        {
            carRb.linearDamping = 0.1f;
            carRb.angularDamping = 0.1f;
        }

        // Enable vehicle controls
        vehicle.GetComponent<CarController>().enabled = true;
    }

    private void ExitVehicle()
    {
        src.Stop();
        isInVehicle = false;

        if (player.GetComponent<PlayerCont>() != null)
        {
            player.GetComponent<PlayerCont>().enabled = true;
        }

        playerRb.detectCollisions = true;
        // Unparent player
        player.SetParent(null);

        // Unlock physics and movement
        playerRb.isKinematic = false;
        playerRb.constraints = RigidbodyConstraints.FreezeRotation;

        // Disable the car controller script
        if (vehicle.GetComponent<CarController>() != null)
        {
            vehicle.GetComponent<CarController>().enabled = false;
        }

        // Move player slightly outside the vehicle
        player.position = vehicle.position + vehicle.transform.right * -2f;

        // Apply realistic braking to the car
        Rigidbody carRb = vehicle.GetComponent<Rigidbody>();
        if (carRb != null)
        {
            StartCoroutine(GradualStop(carRb));
        }
    }

    private IEnumerator GradualStop(Rigidbody carRb)
    {
        Vector3 initialVelocity = carRb.linearVelocity;
        float startSpeed = initialVelocity.magnitude;

        float decelerationRate = 10f;

        while (carRb.linearVelocity.magnitude > 0.1f)
        {
            // Reduce the current speed linearly
            float newSpeed = Mathf.MoveTowards(carRb.linearVelocity.magnitude, 0f, decelerationRate * Time.deltaTime);

            // Apply the new speed in the direction the car is currently moving
            carRb.linearVelocity = carRb.linearVelocity.normalized * newSpeed;

            yield return null;
        }

        carRb.linearVelocity = Vector3.zero;
        carRb.angularVelocity = Vector3.zero;

        // High drag to keep it parked
        carRb.linearDamping = 100f;
        carRb.angularDamping = 100f;

        // Force sleep to ensure it doesn't slide down hills
        carRb.Sleep();
    }
}

