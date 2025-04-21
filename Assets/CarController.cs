using UnityEngine;

public class RealisticCarController : MonoBehaviour
{
    [Header("Car Setup")]
    public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    [Header("Car Parameters")]
    public float maxMotorTorque = 20000f;
    public float maxSteeringAngle = 30f;
    public float brakeForce = 5000f;
    public float maxSpeed = 200f; // Maximum speed to cap the car
    public float jumpForce = 50055f;
    public float groundCheckDistance = 0.5f;

    [Header("Rigidbody Settings")]
    public float carMass = 800f;
    public Vector3 centerOfMass = new Vector3(0, -0.1f, 0); // Lower center of mass for stability

    [Header("Drift Settings")]
    public bool isDrifting = false;
    public ParticleSystem driftSmokeLeft;
    public ParticleSystem driftSmokeRight;
    public float driftFactor = 0.5f; // Lower = More Drift, Higher = More Grip
    public float driftTorqueBoost = 1.2f; // Extra power when drifting
    private ParticleSystem.EmissionModule leftEmission;
    private ParticleSystem.EmissionModule rightEmission;
    

    private Rigidbody rb;
    public Transform groundCheck;
    public LayerMask groundLayer;
    private float motorInput, steeringInput, brakeInput;

    void Start()
    {
        AdjustWheelFriction(frontLeftWheel);
        AdjustWheelFriction(frontRightWheel);
        AdjustWheelFriction(rearLeftWheel);
        AdjustWheelFriction(rearRightWheel);
        rb = GetComponent<Rigidbody>();

        // Setup Rigidbody
        SetupRigidbody();

        // Setup Suspension
        SetupSuspension();
    leftEmission = driftSmokeLeft.emission;
    rightEmission = driftSmokeRight.emission;
        Debug.Log("🚗 Car Initialized!");
    }

    void Update()
    {
        // Get Player Input
        motorInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;

        isDrifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        AdjustDrift();
        // Update Wheel Transforms
        UpdateWheelVisuals();
        // Press space to jump

    }

    void FixedUpdate()
    {
        if (!IsGrounded())
        {
            Debug.LogWarning("🚨 Car is in the air!");
            return;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.X))
        {
            rb.AddForce(Vector3.up * jumpForce);
            Debug.LogWarning("🚨 Car is in the air!");
        }
        }
        // Apply Car Systems
        ApplyMotor();
        ApplySteering();
        ApplyBraking();
        
    }

    // Setup Rigidbody for the car's physics
    private void SetupRigidbody()
    {
        rb.mass = carMass;
        rb.drag = 0.02f;
        rb.angularDrag = 2f;
        rb.centerOfMass = centerOfMass;
    }

    // Setup car suspension for better handling
    private void SetupSuspension()
    {
        JointSpring suspension = new JointSpring
        {
            spring = 40000f,
            damper = 3000f,
            targetPosition = 0.5f
        };

        frontLeftWheel.suspensionSpring = suspension;
        frontRightWheel.suspensionSpring = suspension;
        rearLeftWheel.suspensionSpring = suspension;
        rearRightWheel.suspensionSpring = suspension;
    }

    void AdjustWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        forwardFriction.stiffness = 3.5f;  // Increase traction (Default ~1.0)
        wheel.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.stiffness = 2.0f; // Prevents excessive sliding
        wheel.sidewaysFriction = sidewaysFriction;
    }

    // Apply motor torque for car acceleration
    private void ApplyMotor()
{
    float currentSpeed = rb.velocity.magnitude;
    float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);

    // 🚀 Improved Acceleration Formula
    float accelerationFactor = Mathf.Pow(1f - speedFactor, 2f); // Quadratic curve for smooth transition
    float torque = motorInput * maxMotorTorque * accelerationFactor;

    if (isDrifting)
    {
        torque *= driftTorqueBoost;
    }
    // Apply Torque to Rear Wheels
    rearLeftWheel.motorTorque = torque;
    rearRightWheel.motorTorque = torque;

    Debug.Log($"Speed: {currentSpeed:F2} | Acceleration Factor: {accelerationFactor:F2} | Torque: {torque:F2}");
}


    // Apply steering to the front wheels
    private void ApplySteering()
    {
        float steering = steeringInput * maxSteeringAngle;

        frontLeftWheel.steerAngle = steering;
        frontRightWheel.steerAngle = steering;

        Debug.Log($"Steering Angle: {steering}");
    }

    // Apply braking force
    private void ApplyBraking()
    {
        float brake = brakeInput * brakeForce;

        rearLeftWheel.brakeTorque = brake;
        rearRightWheel.brakeTorque = brake;
        frontLeftWheel.brakeTorque = brake;
        frontRightWheel.brakeTorque = brake;

        Debug.Log($"Braking Force: {brake}");
    }

    // Update the wheel visuals for better visualization of the car movement
    private void UpdateWheelVisuals()
    {
        UpdateWheelPositionAndRotation(frontLeftWheel, frontLeftTransform);
        UpdateWheelPositionAndRotation(frontRightWheel, frontRightTransform);
        UpdateWheelPositionAndRotation(rearLeftWheel, rearLeftTransform);
        UpdateWheelPositionAndRotation(rearRightWheel, rearRightTransform);
    }

    // Update the position and rotation of each wheel collider
    private void UpdateWheelPositionAndRotation(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }


    void AdjustDrift()
    {
        WheelFrictionCurve sidewaysFriction = rearLeftWheel.sidewaysFriction;
    
        if (isDrifting)
        {
            sidewaysFriction.stiffness = driftFactor; // Lower grip for drifting
        }
        else
        {
            sidewaysFriction.stiffness = 2.0f; // Restore normal grip
        }

        rearLeftWheel.sidewaysFriction = sidewaysFriction;
        rearRightWheel.sidewaysFriction = sidewaysFriction;
        leftEmission.enabled = isDrifting;
        rightEmission.enabled = isDrifting;
    }


    // Check if the car is grounded
    private bool IsGrounded()
    {
        return rearLeftWheel.GetGroundHit(out _) && rearRightWheel.GetGroundHit(out _);
    }

    void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Wall"))
    {
        Debug.Log("💥 Car hit a wall!");

        Vector3 impactForce = collision.relativeVelocity * 0.2f; // Absorb 80% of impact
        rb.velocity -= impactForce; // Reduce speed after impact

        rb.AddForce(-collision.contacts[0].normal * 200f, ForceMode.Impulse); // Push car away slightly
    }
}


}
