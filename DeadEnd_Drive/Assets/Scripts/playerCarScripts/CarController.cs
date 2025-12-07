using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections; // for IEnumerator

public class CarController : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear
    }

    [System.Serializable]
    public struct Wheel
    {
        public GameObject WheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    public float breakAcceleration = 50.0f;
    public float maxSpeed = 50f;
    public float motorTorque = 50f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;

    float steerInput;

    private Rigidbody carRb;
    public bool isStoppedByTrigger = false;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
    }

    void Update()
    {
        GetInputs();
        AnimatedWheels();
    }

    void LateUpdate()
    {
        HandleBrakingAndReverse();
        Move();
        Steer();
    }

    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }
    void Move()
    {
        if (isStoppedByTrigger) return;
        float speedFactor = Mathf.Clamp01((maxSpeed - carRb.linearVelocity.magnitude) / maxSpeed);
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * motorTorque * speedFactor;
        }

    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void HandleBrakingAndReverse()
    {
        if (isStoppedByTrigger) // stop all input
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = 0f;
                wheel.wheelCollider.brakeTorque = 600f; // keep brakes applied
            }
            return;
        }
        float forwardSpeed = Vector3.Dot(carRb.linearVelocity, transform.forward);
        float speedFactor = Mathf.Clamp01((maxSpeed - carRb.linearVelocity.magnitude) / maxSpeed);

        // 1. Handbrake (SPACE)
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 600 * breakAcceleration;
                wheel.wheelCollider.motorTorque = 0f;
            }
            return;
        }

        // 2. Reverse / Brake (S key)
        if (Input.GetKey(KeyCode.S))
        {
            foreach (var wheel in wheels)
            {
                if (forwardSpeed > 0.1f)
                {
                    // moving forward → brake first
                    wheel.wheelCollider.brakeTorque = 400 * breakAcceleration;
                    wheel.wheelCollider.motorTorque = 0f;
                }
                else
                {
                    // stopped or rolling backward → reverse
                    wheel.wheelCollider.brakeTorque = 0f;
                    wheel.wheelCollider.motorTorque = -speedFactor * motorTorque;
                }
            }
            return;
        }

        // 3. No braking or reversing
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.brakeTorque = 0f;
        }
    }


    void AnimatedWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;

            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.WheelModel.transform.position = pos;
            wheel.WheelModel.transform.rotation = rot;
        }
    }
    //--force stop for fuel empty
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StopCarTrigger"))
        {
            StopCarSmooth();

            CarEntry entryScript = other.GetComponent<CarEntry>();
            if (entryScript != null)
            {
                entryScript.isOutOfFuel = true;
            }
        }
    }

    public void StopCarSmooth(float stopDuration = 0.5f)
    {
        StartCoroutine(GradualStop(stopDuration));
    }

    private IEnumerator GradualStop(float duration)
    {
        isStoppedByTrigger = true;

        float elapsed = 0f;

        float targetBrakeTorque = 600f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = 0f;
                wheel.wheelCollider.brakeTorque = Mathf.Lerp(0f, targetBrakeTorque, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = 0f;
            wheel.wheelCollider.brakeTorque = targetBrakeTorque;
        }
    }

}
