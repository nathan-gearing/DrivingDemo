using UnityEngine;
using UnityEngine.Assertions.Must;
using System.Collections;
using System.Collections.Generic;

public class CarSounds : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;
    private float currentSpeed;

    private Rigidbody carRb;
    private AudioSource carEngine;

    public float minPitch;
    public float maxPitch;
    private float pitchFromCar;

    void Start()
    {
        carEngine = GetComponent<AudioSource>();
        carRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        EngineSound();
    }

    
    void EngineSound()
    {
        currentSpeed = carRb.linearVelocity.magnitude;
        pitchFromCar = carRb.linearVelocity.magnitude / 60f;

        if (currentSpeed < minSpeed)
        {
            carEngine.pitch = minPitch;
        }

        if (currentSpeed > minSpeed && currentSpeed < maxSpeed)
        {
            carEngine.pitch = minPitch + pitchFromCar;
        }

        if (currentSpeed > maxSpeed)
        {
            carEngine.pitch = maxPitch;
        }
    }

}
