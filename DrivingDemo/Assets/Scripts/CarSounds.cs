using UnityEngine;
using UnityEngine.Assertions.Must;
using System.Collections;
using System.Collections.Generic;

public class CarSounds : MonoBehaviour
{
    
    public float minRPM = 800f;
    public float maxRPM = 7000f;

    
    private AudioSource carEngine;
    private CarController carController;

    public float minPitch;
    public float maxPitch;
    private float pitchFromCar;

    void Start()
    {
        carEngine = GetComponent<AudioSource>();
        carController = GetComponent<CarController>();
    }

    void Update()
    {
        EngineSound();
    }

    
    void EngineSound()
    {
        float rpmNormalized = Mathf.InverseLerp(minRPM, maxRPM, carController.engineRPM);
        carEngine.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);
    }

}
