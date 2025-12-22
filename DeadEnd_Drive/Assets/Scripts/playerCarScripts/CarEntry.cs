using System;
using TMPro;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Playables;

public class CarEntry : MonoBehaviour
{
    [Header("Settings")]
    public bool startInsideCar = false;
    public bool isOutOfFuel = false;

    [Header("Ride Settings")]

    public float rideDuration = 20f;

    [Header("References")]
    public Transform player;
    public Transform vehicle;
    public Transform carDoor;
    public FPVCam CameraController;
    public Transform driverSeat;
    public PlayableDirector carTimeline;

    [Header("UI")]
    public TextMeshProUGUI carEntry;
    public TextMeshProUGUI gasEmptyText;

    private bool isInVehicle = false;
    private bool hasRideFinished = false;
    private Rigidbody playerRb;
    private PlayerCont playerController;

    [Header("Sound")]
    private AudioSource src;
    public AudioClip startEngine;
    public AudioClip driveEngine;

    void Start()
    {
        if (gasEmptyText != null) gasEmptyText.gameObject.SetActive(false);
        if (carEntry != null) carEntry.gameObject.SetActive(false);

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            playerController = player.GetComponent<PlayerCont>();
        }

        src = gameObject.GetComponent<AudioSource>();

        if (startInsideCar)
        {
            EnterVehicle();

            StartCoroutine(StartEngineRoutine());
        }
    }

    void Update()
    {
        if (isInVehicle)
        {
            HandlePlayerExit();
        }
        else
        {
            HandlePlayerEntry();
        }
    }

    private void HandlePlayerEntry()
    {
        if (hasRideFinished) return;
        if (player == null || carDoor == null) return;

        if (Vector3.Distance(player.position, carDoor.position) < 3f)
        {
            if (carEntry != null)
            {
                carEntry.text = "Press F to Enter";
                carEntry.gameObject.SetActive(true);
            }


            if (Input.GetKeyDown(KeyCode.F))
            {
                carEntry.gameObject.SetActive(false);
                EnterVehicle();
                StartCoroutine(StartEngineRoutine());
            }
        }
        else
        {
            if (carEntry != null)
            {
                carEntry.gameObject.SetActive(false);    
            }
            
        }
    }

    private IEnumerator StartEngineRoutine()
    {

        if (src == null || startEngine == null) yield break;

        src.loop = false;
        src.PlayOneShot(startEngine);

        yield return new WaitForSeconds(startEngine.length);

        if (driveEngine != null)
        {
            src.clip = driveEngine;
            src.loop = true;
            src.Play();
        }
    }


    private void HandlePlayerExit()
    {
        if (carTimeline != null && carTimeline.state == PlayState.Playing)
        {
            if (carEntry != null) carEntry.gameObject.SetActive(false);
            return;
        }

        if (carEntry != null)
        {
            carEntry.text = "Press F to Exit";
            carEntry.gameObject.SetActive(true);
        }

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

        isInVehicle = true;

        if (playerController != null) playerController.enabled = false;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = true;
            playerRb.detectCollisions = false;
        }

        player.SetParent(driverSeat);
        player.localPosition = Vector3.zero;
        player.localRotation = Quaternion.identity;

        if (carTimeline != null)
        {
            carTimeline.Play();
        }
    }

    private void ExitVehicle()
    {
        if (src != null) src.Stop();

        isInVehicle = false;
        hasRideFinished = true;
        if (carEntry != null)
        {
            carEntry.text = "Press F to Enter";
            carEntry.gameObject.SetActive(false);
        }

        player.SetParent(null);

        if (carDoor != null)
        {
            player.position = carDoor.position;
            player.rotation = carDoor.rotation;
        }
        else
        {
            player.position = vehicle.position + vehicle.transform.right * -2f;
        }

        if (playerRb != null)
        {
            playerRb.detectCollisions = true;
            playerRb.isKinematic = false;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (isOutOfFuel)
        {
            StartCoroutine(ShowDialogueRoutine());
        }
    }

    private IEnumerator ShowDialogueRoutine()
    {
        if (gasEmptyText != null)
        {
            gasEmptyText.text = "Dammit... Gas is empty. Maybe there's something in that house.";
            gasEmptyText.gameObject.SetActive(true);

            yield return new WaitForSeconds(4f);

            gasEmptyText.gameObject.SetActive(false);
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

