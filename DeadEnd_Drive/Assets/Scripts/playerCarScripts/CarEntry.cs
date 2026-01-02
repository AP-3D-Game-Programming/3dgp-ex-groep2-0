using System;
using TMPro;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Playables;

public class CarEntry : MonoBehaviour
{
    [Header("Settings")]
    public bool isLocked = true;
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
    public int text = 1;


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
            isLocked = false;
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
                if (isLocked)
                {
                    carEntry.text = "Locked, there must be a key nearby";
                }
                else
                {
                    carEntry.text = "Press F to Enter";

                    carEntry.gameObject.SetActive(true);
                }
            }


            if (!isLocked && Input.GetKeyDown(KeyCode.F))
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

    public void UnlockCar()
    {
        isLocked = false;
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
        if (carEntry != null) carEntry.gameObject.SetActive(false);
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
        if (carTimeline != null) carTimeline.Play();
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
            if (text == 1)
            gasEmptyText.text = "Dammit... Gas is empty. Maybe there's something in that house.";
            if (text == 2)
                gasEmptyText.text = "Maybe I should go back in that house? I don't think I have any other choice.";
            gasEmptyText.gameObject.SetActive(true);

            yield return new WaitForSeconds(4f);

            gasEmptyText.gameObject.SetActive(false);
        }
    }
}