using UnityEngine;

public class CarCameraFollow : MonoBehaviour
{
    public Transform target;  // The car
    public Vector3 offset = new Vector3(0, 2.35f, -6.5f);  // Camera's offset behind the car
    public Vector3 rotationOffset = new Vector3(0, 0f, 0);  // Rotation offset (adjust the Y-axis rotation angle)
    public float positionSmoothSpeed = 5f;  // Smoothness for position following
    public float rotationSmoothSpeed = 5f;  // Smoothness for rotation following
    public Camera orbitCamera;  // The orbit camera (main camera)
    public Camera topCamera;    // The top camera (second camera)
    public Vector3 cameraOffset = new Vector3(5f, 25f, -5f); 
    public Vector3 specialOffset = new Vector3(0, 2.35f, 0);
    private bool useTopCamera = false;  // Whether to use the top-down camera
    private float topCameraHeight = 10f;  // Height above the car for top camera
    private float topCameraRotationSpeed = 3f; // Speed at which top-down camera rotates
    private float rotationX = 0f;  // X axis rotation for top-down camera
    private float rotationY = 0f;  // Y axis rotation for top-down camera
    private float currentMouseX = 0f;
    private float currentMouseY = 0f;

    private Vector3 previousPosition;


    void Start()
    {
        // Disable the top camera initially
        if (topCamera != null)
        {
            topCamera.gameObject.SetActive(false);  // Disable top-down camera
        }

        previousPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Toggle between orbit mode and top-down camera using the 'T' key
        if (Input.GetKeyDown(KeyCode.T))
        {
            useTopCamera = !useTopCamera;

            if (useTopCamera)
            {
                orbitCamera.gameObject.SetActive(false);  // Disable orbit camera
                if (topCamera != null) topCamera.gameObject.SetActive(true);  // Enable top camera
            }
            else
            {
                topCamera.gameObject.SetActive(false);  // Disable top camera
                orbitCamera.gameObject.SetActive(true);  // Enable orbit camera
                positionSmoothSpeed = 255;
                rotationSmoothSpeed = 255;
                
            }
        }

        if (useTopCamera)
        {
            // rotationSmoothSpeed = 2f;
            // positionSmoothSpeed = 353f;
            // Top-Down Camera Mode (like orbit mode):
            // Get mouse input for camera rotation
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Accumulate mouse movement for rotation (for smoother experience)
            currentMouseX += mouseX * topCameraRotationSpeed;
            currentMouseY -= mouseY * topCameraRotationSpeed;

            // Clamp the vertical rotation to avoid flipping
            currentMouseY = Mathf.Clamp(currentMouseY, -80f, 80f);

            // Calculate the desired position based on the rotation and height
            Vector3 direction = new Vector3(Mathf.Sin(currentMouseX * Mathf.Deg2Rad), Mathf.Sin(currentMouseY * Mathf.Deg2Rad), Mathf.Cos(currentMouseX * Mathf.Deg2Rad));
            Vector3 orbitPosition = target.position + direction * topCameraHeight;

            // Offset to keep the car in view — feel free to tweak these values

            // Apply the offset in the rotated space so it moves with camera direction
            Vector3 desiredPosition = orbitPosition + topCamera.transform.rotation * cameraOffset;

            // Smoothly move the camera to the desired position
            topCamera.transform.position = target.position + new Vector3(0, 3.5f, 0);

            // Make the camera always look at the car
            topCamera.transform.rotation = Quaternion.Euler(currentMouseY, currentMouseX, 0);
        }
        else
        {
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothSpeed * Time.deltaTime);

        // Apply rotation offset to the car's forward direction
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        desiredRotation *= Quaternion.Euler(rotationOffset);  // Apply the rotation offset
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
        rotationSmoothSpeed = 5f;
        positionSmoothSpeed = 5f;
        }
    }
}
