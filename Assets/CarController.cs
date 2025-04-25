using UnityEngine;
using TMPro;
public class FastSmoothCarController : MonoBehaviour
{
    [Header("Car Setup")]
    public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    [Header("Car Parameters")]
    public float maxMotorTorque = 80000f; // Increased for quick acceleration
    public float maxSteeringAngle = 30f;  // Sharper turns
    public float currentMaxSteeringAngle = 0f;
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

    public TextMeshProUGUI speedText;
    
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

    speedText.text = $"Speed: {(currentSpeed/ 3.14f):F1} km/h";
    //Debug.Log($"Spee: {(currentSpeed/ 3.14f):F1} km/h | Torque: {tordque:F0}");
}


   private void ApplySteering()
{
    float baseAngle = maxSteeringAngle; // max at 0 km/h
    float minAngle = 2f;   // minimum possible steering at high speed
    float exponent = 35.3f; // tweak this for how *fast* it drops (try 3.5–4.5)

    float speedFactor = Mathf.Clamp01(currentSpeed / 245f); // normalized based on 200 km/h

    // Exponential dropoff — fast reduction
    float steeringAngle = Mathf.Lerp(baseAngle, minAngle, Mathf.Pow(speedFactor, exponent));
    float adjustedSteering = steeringInput * steeringAngle;
    currentMaxSteeringAngle = steeringAngle;
    if (isDrifting)
        adjustedSteering *= 0.85f;

    frontLeftWheel.steerAngle = adjustedSteering;
    frontRightWheel.steerAngle = adjustedSteering;

    //Debug.Log($"Speed: {(currentSpeed / 3.14f):F0} km/h | Steering Angle: {adjustedSteering:F2}");
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
