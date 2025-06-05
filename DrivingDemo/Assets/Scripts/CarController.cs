using UnityEngine;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear
    }
    
    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticles;
        public Axel axel;
    }

    public float engineRPM;
    public float minRPM = 1000f;
    public float maxRPM = 7000f;

    public float downforceMultiplier = 50f;
    public float steerDampening = 0.7f;
    public float steerSpeedThreshold = 50f;

    public int currentGear = 1;
    public float[] gearRatios = { 2.66f, 1.78f, 1.30f, 1.00f, 0.74f };
    public float finalDriveRatio = 3.42f;

    public float wheelRadius = 0.34f;
    public float engineTorque = 250f;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    private CarLights carLights;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
        carLights = GetComponent<CarLights>();
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
    }

    void FixedUpdate()
    {
        Move();
        UpdateEngineRPM();
        AutoShift();
        Steer();
        Brake();
        WheelEffect();
        ApplyDownforce();
    }
    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void Move()
    {
        /*foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }*/
        float driveTorque = engineTorque * gearRatios[currentGear - 1] * finalDriveRatio;
        driveTorque *= moveInput;

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear)
            {
                WheelHit hit;
                wheel.wheelCollider.GetGroundHit(out hit);

                float slip = Mathf.Abs(hit.forwardSlip);
                float tractionControl = Mathf.Clamp01(1.0f - slip);

                wheel.wheelCollider.motorTorque = driveTorque * tractionControl;
            }
        }

    }

    void Steer()
    {
        float speed = carRb.linearVelocity.magnitude;
        float speedFactor = Mathf.Clamp01(speed / steerSpeedThreshold);
        float adjustedSteerInput = steerInput * (1f - speedFactor * steerDampening);

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
            }

            carLights.isBackLightOn = true;
            carLights.OperateBackLights();
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
            carLights.isBackLightOn = false;
            carLights.OperateBackLights();
        }
    }

    void ApplyDownforce()
    {
        float downforce = carRb.linearVelocity.magnitude * downforceMultiplier;
        carRb.AddForce(-transform.up * downforce);
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffect()
    {
        foreach (var wheel in wheels)
        {
            if(Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded == true && carRb.linearVelocity.magnitude >= 10.0f)
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                wheel.smokeParticles.Emit(1);
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }

    void UpdateEngineRPM()
    {
        float averageWheelRPM = 0f;
        int drivenWheels = 0;

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear)
            {
                averageWheelRPM += wheel.wheelCollider.rpm;
                drivenWheels++;
            }

            if (drivenWheels > 0)
            {
                averageWheelRPM /= drivenWheels;
            }

            engineRPM = Mathf.Clamp(averageWheelRPM * gearRatios[currentGear - 1] * finalDriveRatio, minRPM, maxRPM);
        }
    }

    void AutoShift()
    {
        if (engineRPM > maxRPM - 500 && currentGear < gearRatios.Length)
        {
            currentGear++;
        }
        else if (engineRPM < minRPM + 500 && currentGear > 1)
        {
            currentGear--;
        }
    }
}
