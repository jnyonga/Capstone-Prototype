using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class CarController : MonoBehaviour
{
    /*source https://youtu.be/jr4eb4F9PSQ?si=7j5tSQQ-YjDYH4E8 */
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
        public Axel axel;
    }

    [Header("Performance")]
    public float maxAcceleration = 5.0f;
    public float maxSpeed = 25.0f; // m/s
    public float brakeAcceleration = 50.0f;

    [Header("Steering")]
    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    [Header("Jump")]
    public float jumpForce = 500.0f;
    public KeyCode jumpKey = KeyCode.E;
    public float jumpCooldown = 1.0f;

    [Header("Physics")]
    public Vector3 _centerOfMass;

    [Header("Wheels")]
    public List<Wheel> wheels;

    [Header("Speed Info (Read Only)")]
    public float currentSpeed; //in m/s
    public float currentSpeedMPH; //speed in mp/h for guage
    public TextMeshProUGUI speedGauge;

    [Header("Jump Info (Read Only)")]
    public bool isGrounded;
    public float lastJumpTime;

    float moveInput;
    float steerInput;

    private Rigidbody carRb;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
    }
    void Update()
    {
        GetInputs();
        UpdateSpeedInfo();
        CheckGrounded();
        Animatewheels();
        Jump();
    }

    void FixedUpdate()
    {
        Move();
        Steer();
        Brake();
        
    }

    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    void UpdateSpeedInfo()
    {
        currentSpeed = carRb.linearVelocity.magnitude;
        currentSpeedMPH = currentSpeed * 2.237f; //conversion
        speedGauge.text = Mathf.Round(currentSpeedMPH).ToString() + " MPH";
    }

    void CheckGrounded()
    {
        isGrounded = true;
        foreach(var wheel in wheels)
        {
            if(!wheel.wheelCollider.isGrounded)
            {
                isGrounded = false;
                break;
            }
        }
    }

    void Jump()
    {
        if(Input.GetKeyDown(jumpKey) && isGrounded && CanJump())
        {
            carRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
    }

    bool CanJump()
    {
        return Time.time - lastJumpTime > jumpCooldown;
    }
    void Move()
    {
        if(currentSpeed < maxSpeed)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = moveInput * maxAcceleration * 100;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = 0;
            }
        }
        
    }

    void Steer()
    {
        foreach(var wheel in wheels)
        {
            if(wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            foreach(var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 100 * brakeAcceleration;
            }
        }
        else
        {
            foreach(var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }

    void Animatewheels()
    {
        foreach(var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}
