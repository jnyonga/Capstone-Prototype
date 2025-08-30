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
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticle;
        public Axel axel;
    }

    [Header("Performance")]
    public float maxAcceleration = 30.0f;
    public float maxSpeed = 44.704f; // m/s
    //public float brakeAcceleration = 35.0f;

    [Header("Steering")]
    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    [Header("Drifting")]
    public float driftSteerMultiplier = 2.0f;
    public float driftGripReduction = 0.3f;
    public float driftSpeedBoost = 1.2f;
    public float driftAccelerationBoost = 1.5f;
    public bool isDriftActive = false;

    [Header("Drift Angle")]
    public float maxDriftAngle = 45f;
    public float driftAngleCorrection = 2f;
    public float idealDriftAngle = 25f;

    [Header("Drift Info (Read Only)")]
    public float currentDriftAngle;
    private WheelFrictionCurve[] originalSidewaysFriction;
    private WheelFrictionCurve[] originalForwardFriction;

    [Header("Jump")]
    public float jumpForce = 15000.0f;
    public KeyCode jumpKey = KeyCode.F;
    public float jumpCooldown = 3.0f;

    [Header("Air Control")]
    public float airSteerForce = 300f;
    public float airAcceleration = 10f;
    public bool enableAirAcceleration = true;

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

        StoreFrictionCurves();
    }
    void Update()
    {
        GetInputs();
        UpdateSpeedInfo();
        CheckGrounded();
        Animatewheels();
        WheelEffects();
        Jump();
        HandleDriftToggle();
    }

    void FixedUpdate()
    {
        Move();
        Steer();
        //Brake();
        AirControl();
        ManageDriftAngle();

    }

    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        //isHoldingBrake = Input.GetKey(KeyCode.Space);
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
        float currentMaxSpeed = isDriftActive ? maxSpeed * driftSpeedBoost : maxSpeed;
        float currentAcceleration = isDriftActive ? maxAcceleration * driftAccelerationBoost : maxAcceleration;

        foreach (var wheel in wheels)
        {
            if (moveInput > 0)
            {
                if (currentSpeed < maxSpeed)
                {
                    wheel.wheelCollider.motorTorque = moveInput * maxAcceleration * 100;
                }
                else
                {
                    wheel.wheelCollider.motorTorque = moveInput * maxAcceleration * 20;
                }
            }
            else if (moveInput < 0)
            {
                wheel.wheelCollider.motorTorque = moveInput * maxAcceleration * 100;
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0;
            }
        }

        //add speed while drifting to stay fast
        if (isDriftActive && moveInput > 0 && currentSpeed > 5f)
        {
            Vector3 driftBoostForce = transform.forward * currentAcceleration * 0.5f;
            carRb.AddForce(driftBoostForce, ForceMode.Acceleration);
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                float steerMultiplier = isDriftActive ? driftSteerMultiplier : 1.0f;
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle * steerMultiplier;

                float lerpSpeed = isDriftActive ? 0.8f : 0.6f;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, lerpSpeed);
            }
        }
    }

    /*void Brake()
    {
        // Only brake when holding spacebar (not just toggling drift)
        if (isHoldingBrake && wasBrakePressed) // Only brake while holding after initial press
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 100 * brakeAcceleration;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }
    */
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

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
           
            bool shouldShowEffects = isDriftActive && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded && carRb.linearVelocity.magnitude >= 10f;

            if (shouldShowEffects)
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                wheel.smokeParticle.Emit(1);
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }

    void AirControl()
    {
        if(!isGrounded)
        {
            if(Mathf.Abs(steerInput) > 0.1f)
            {
                Vector3 strafeForce = transform.right * steerInput * airAcceleration;
                carRb.AddForce(strafeForce);
            }
            float rollInput = 0f;
            if (Input.GetKey(KeyCode.E))
            {
                rollInput = -1f; // Roll left
            }
            if (Input.GetKey(KeyCode.Q))
            {
                rollInput = 1f; // Roll right
            }

            if (Mathf.Abs(rollInput) > 0.1f)
            {
                Vector3 rollTorque = transform.forward * rollInput * airSteerForce;
                carRb.AddTorque(rollTorque);
            }
        }
    }

    void StoreFrictionCurves()
    {
        originalSidewaysFriction = new WheelFrictionCurve[wheels.Count];
        originalForwardFriction = new WheelFrictionCurve[wheels.Count];

        for (int i = 0; i < wheels.Count; i++)
        {
            originalSidewaysFriction[i] = wheels[i].wheelCollider.sidewaysFriction;
            originalForwardFriction[i] = wheels[i].wheelCollider.forwardFriction;
        }
    }

    void HandleDriftToggle()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDriftActive = !isDriftActive;
            ApplyDriftPhysics();

            Debug.Log("Drift mode: " + (isDriftActive ? "ON" : "OFF"));
        }
    }

    //this was made with ai sorry :(
    void ApplyDriftPhysics()
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            if (isDriftActive)
            {
                // Reduce grip for drifting - this only affects sliding, NOT steering or acceleration
                WheelFrictionCurve sidewaysFriction = originalSidewaysFriction[i];
                WheelFrictionCurve forwardFriction = originalForwardFriction[i];

                // Reduce rear wheel grip more than front wheels for better drift feel
                float gripMultiplier = wheels[i].axel == Axel.Rear ?
                    driftGripReduction * 0.5f : driftGripReduction;

                sidewaysFriction.extremumValue *= gripMultiplier;
                sidewaysFriction.asymptoteValue *= gripMultiplier;

                // Reduce forward grip less to maintain acceleration ability
                forwardFriction.extremumValue *= (gripMultiplier + 0.3f); // Keep some forward grip
                forwardFriction.asymptoteValue *= (gripMultiplier + 0.3f);

                wheels[i].wheelCollider.sidewaysFriction = sidewaysFriction;
                wheels[i].wheelCollider.forwardFriction = forwardFriction;
            }
            else
            {
                // Restore original grip
                wheels[i].wheelCollider.sidewaysFriction = originalSidewaysFriction[i];
                wheels[i].wheelCollider.forwardFriction = originalForwardFriction[i];
            }
        }

        
    }
    //This is AI, TEMPORARY
    void ManageDriftAngle()
    {
        if (!isDriftActive || !isGrounded) return;

        // Calculate current drift angle
        Vector3 velocityDirection = carRb.linearVelocity.normalized;
        Vector3 forwardDirection = transform.forward;

        currentDriftAngle = Vector3.SignedAngle(forwardDirection, velocityDirection, transform.up);
        currentDriftAngle = Mathf.Abs(currentDriftAngle); // Use absolute value

        // Apply correction forces if angle is too extreme
        if (currentDriftAngle > maxDriftAngle)
        {
            // Calculate correction force to bring angle back under control
            Vector3 correctionDirection = Vector3.Cross(transform.up, velocityDirection).normalized;
            float correctionStrength = (currentDriftAngle - maxDriftAngle) / maxDriftAngle;

            Vector3 correctionForce = correctionDirection * correctionStrength * driftAngleCorrection * currentSpeed;
            carRb.AddForce(correctionForce, ForceMode.Acceleration);

            // Also apply counter-rotation
            float rotationCorrection = correctionStrength * driftAngleCorrection * 50f;
            carRb.AddTorque(-transform.up * rotationCorrection * Mathf.Sign(currentDriftAngle), ForceMode.Acceleration);
        }
    }
}
