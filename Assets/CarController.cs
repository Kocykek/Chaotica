using UnityEngine;

public class FastSmoothCarController : MonoBehaviour
{
    [Header("Car Setup")]
    public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    [Header("Car Parameters")]
    public float maxMotorTorque = 80000f; // Increased for quick acceleration
    public float maxSteeringAngle = 30f;  // Sharper turns
    public float brakeForce = 10000f;     // Strong brakes
    public float maxSpeed = 500f;
    public float jumpForce = 400f;
    public float groundCheckDistance = 0.5f;

    [Header("Rigidbody Settings")]
    public float carMass = 1200f;
    public Vector3 centerOfMass = new Vector3(0, -0.5f, 0); // Lowered for tighter handling

    [Header("Drift Settings")]
    public bool isDrifting = false;
    public ParticleSystem driftSmokeLeft;
    public ParticleSystem driftSmokeRight;
    public float driftFactor = 0.2f;   // More drift
    public float driftTorqueBoost = 2.2f;
    private ParticleSystem.EmissionModule leftEmission;
    private ParticleSystem.EmissionModule rightEmission;

    private Rigidbody rb;
    private float motorInput, steeringInput, brakeInput;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = carMass;
        rb.centerOfMass = centerOfMass;
        rb.drag = 0.02f;
        rb.angularDrag = 3f;

        SetUpWheelFriction(frontLeftWheel);
        SetUpWheelFriction(frontRightWheel);
        SetUpWheelFriction(rearLeftWheel);
        SetUpWheelFriction(rearRightWheel);

        leftEmission = driftSmokeLeft.emission;
        rightEmission = driftSmokeRight.emission;
    }

    void Update()
    {
        motorInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        isDrifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        AdjustDrift();
        UpdateWheelVisuals();
    }

    void FixedUpdate()
    {
        ApplyMotor();
        ApplySteering();
        ApplyBraking();

        if (Input.GetKeyDown(KeyCode.X) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (currentSpeed > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

void SetUpWheelFriction(WheelCollider wheel)
{
    WheelFrictionCurve forward = wheel.forwardFriction;
    WheelFrictionCurve sideways = wheel.sidewaysFriction;

    // Forward = Acceleration grip
    forward.extremumSlip = 0.4f;
    forward.extremumValue = 2f;
    forward.asymptoteSlip = 0.8f;
    forward.asymptoteValue = 1.5f;
    forward.stiffness = 2.5f;

    // Sideways = Turning grip + drift balance
    sideways.extremumSlip = 0.3f;
    sideways.extremumValue = 2.2f;
    sideways.asymptoteSlip = 0.6f;
    sideways.asymptoteValue = 1.8f;
    sideways.stiffness = 2.0f;

    wheel.forwardFriction = forward;
    wheel.sidewaysFriction = sideways;
}



    private void ApplyMotor()
{
    currentSpeed = rb.velocity.magnitude * 3.6f; // convert to km/h

    // More aggressive acceleration curve
    float accelerationFactor = Mathf.Pow(1f - (currentSpeed / maxSpeed), 1.2f); // smoother scaling
    float torque = motorInput * maxMotorTorque * accelerationFactor;

    if (isDrifting)
    {
        torque *= driftTorqueBoost;
    }

    rearLeftWheel.motorTorque = torque;
    rearRightWheel.motorTorque = torque;

    Debug.Log($"Speed: {currentSpeed:F1} km/h | Torque: {torque:F0}");
}


    private void ApplySteering()
{
    float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
    float adjustedSteering = steeringInput * Mathf.Lerp(maxSteeringAngle, maxSteeringAngle * 0.4f, speedFactor);

    if (isDrifting)
    {
        adjustedSteering *= 0.8f; // slight cut during drift
    }

    frontLeftWheel.steerAngle = adjustedSteering;
    frontRightWheel.steerAngle = adjustedSteering;

    Debug.Log($"Steering: {adjustedSteering:F1}");
}


    private void ApplyBraking()
    {
        float brake = brakeInput * brakeForce;

        frontLeftWheel.brakeTorque = brake;
        frontRightWheel.brakeTorque = brake;
        rearLeftWheel.brakeTorque = brake;
        rearRightWheel.brakeTorque = brake;
    }

    private void UpdateWheelVisuals()
    {
        UpdateWheelPositionAndRotation(frontLeftWheel, frontLeftTransform);
        UpdateWheelPositionAndRotation(frontRightWheel, frontRightTransform);
        UpdateWheelPositionAndRotation(rearLeftWheel, rearLeftTransform);
        UpdateWheelPositionAndRotation(rearRightWheel, rearRightTransform);
    }

    private void UpdateWheelPositionAndRotation(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    void AdjustDrift()
{
    float driftGrip = isDrifting ? driftFactor : 1.5f;

    WheelFrictionCurve sidewaysFriction = rearLeftWheel.sidewaysFriction;
    sidewaysFriction.stiffness = driftGrip;

    rearLeftWheel.sidewaysFriction = sidewaysFriction;
    rearRightWheel.sidewaysFriction = sidewaysFriction;

    leftEmission.enabled = isDrifting;
    rightEmission.enabled = isDrifting;
}


    private bool IsGrounded()
    {
        return rearLeftWheel.GetGroundHit(out _) && rearRightWheel.GetGroundHit(out _);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector3 impact = collision.relativeVelocity * 0.2f;
            rb.velocity -= impact;
            rb.AddForce(-collision.contacts[0].normal * 250f, ForceMode.Impulse);
        }
    }
}
