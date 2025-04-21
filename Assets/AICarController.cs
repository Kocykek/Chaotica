using UnityEngine;
using System.Collections.Generic;

public class AICarController : MonoBehaviour
{
    [Header("Waypoints & AI Settings")]
    public List<Transform> waypoints;  // List of waypoints
    public float waypointThreshold = 5f; // Distance to switch waypoints
    private int currentWaypointIndex = 0;

    [Header("Car Parameters")]
    public float maxMotorTorque = 20000f;
    public float maxSteeringAngle = 30f;
    public float brakeForce = 5000f;
    public float maxSpeed = 200f; // Max speed cap

    [Header("Rigidbody Settings")]
    public float carMass = 800f;
    public Vector3 centerOfMass = new Vector3(0, -0.1f, 0); // Lower CoM for stability

    [Header("Wheel Setup")]
    public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Setup Rigidbody
        SetupRigidbody();

        // Setup Suspension
        SetupSuspension();

        Debug.Log("🤖 AI Car Initialized!");
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;

        if (!IsGrounded())
        {
            Debug.LogWarning("🚨 AI Car is in the air!");
            return;
        }

        DriveTowardsWaypoint();
        UpdateWheelVisuals();
    }

    // Setup Rigidbody for better physics behavior
    private void SetupRigidbody()
    {
        rb.mass = carMass;
        rb.drag = 0.02f;
        rb.angularDrag = 2f;
        rb.centerOfMass = centerOfMass;
    }

    // Setup car suspension for smooth handling
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

    // AI follows waypoints for smooth racing
    private void DriveTowardsWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        Vector3 targetPosition = targetWaypoint.position + randomOffset;
        // Direction towards waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        float steeringVariation = Random.Range(-5f, 5f); // Adjust the range as needed
        float steering = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        steering = Mathf.Clamp(steering + steeringVariation, -maxSteeringAngle, maxSteeringAngle);

        // Apply steering
        frontLeftWheel.steerAngle = steering;
        frontRightWheel.steerAngle = steering;

        float turnSharpness = Mathf.Abs(steering) / maxSteeringAngle; // 0 = straight, 1 = sharp turn
        float turnSpeedFactor = Mathf.Lerp(1f, 0.5f, turnSharpness); // Reduce speed up to 50% in sharp turns

        // Speed control
        float currentSpeed = rb.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
        float accelerationNoise = Random.Range(0.7f, 1.1f); // +/- 10% variation
        float accelerationFactor = Mathf.Pow(1f - speedFactor, 2f) * accelerationNoise;
        float torque = maxMotorTorque * accelerationFactor;

        // Apply motor torque to rear wheels
        rearLeftWheel.motorTorque = torque;
        rearRightWheel.motorTorque = torque;

        Debug.Log($"Speed: {currentSpeed:F2} | Acceleration: {accelerationFactor:F2} | Torque: {torque:F2}");

        // Switch to next waypoint when close
        if (Vector3.Distance(transform.position, targetWaypoint.position) < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count; // Loop waypoints
        }
    }

    // Check if the AI car is grounded
    private bool IsGrounded()
    {
        return rearLeftWheel.GetGroundHit(out _) && rearRightWheel.GetGroundHit(out _);
    }

    // Update wheel visuals for better realism
    private void UpdateWheelVisuals()
    {
        UpdateWheelPositionAndRotation(frontLeftWheel, frontLeftTransform);
        UpdateWheelPositionAndRotation(frontRightWheel, frontRightTransform);
        UpdateWheelPositionAndRotation(rearLeftWheel, rearLeftTransform);
        UpdateWheelPositionAndRotation(rearRightWheel, rearRightTransform);
    }

    private void UpdateWheelPositionAndRotation(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("💥 AI Car hit a wall!");
            Vector3 impactForce = collision.relativeVelocity * 0.2f; // Reduce impact force
            rb.velocity -= impactForce;
            rb.AddForce(-collision.contacts[0].normal * 200f, ForceMode.Impulse);
        }
    }
}
