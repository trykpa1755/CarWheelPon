using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    DrivingScript driveScript;

    float lastTimeMoving = 0;
    CheckPointController checkPointController;

    void Start()
    {
        driveScript = GetComponent<DrivingScript>();
        checkPointController = driveScript.rb.GetComponent<CheckPointController>();
    }

    void Update()
    {
        if (checkPointController.lap == RaceController.totalLaps + 1) return;

        float accel = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");
        float brake = Input.GetAxis("Jump");

        if (driveScript.rb.velocity.magnitude > 1 || !RaceController.racing) lastTimeMoving = Time.time;
        if (Time.time > lastTimeMoving + 4 || driveScript.rb.gameObject.transform.position.y < -5)
        {
            
            driveScript.rb.transform.position = checkPointController.lastPoint.transform.position + Vector3.up * 2;
            driveScript.rb.transform.rotation = checkPointController.lastPoint.transform.rotation;
            driveScript.rb.gameObject.layer = 6;
            Invoke("ResetLayer", 3);
        }


        if (!RaceController.racing) accel = 0;
        driveScript.Drive(accel, brake, steer);
        ////
        driveScript.EngineSound();
    }

    void ResetLayer()
    {
        driveScript.rb.gameObject.layer = 0;
    }
}
