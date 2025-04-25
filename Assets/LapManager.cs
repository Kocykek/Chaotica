using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using TMPro;

public class LapManager : MonoBehaviour
{
    public Checkpoint[] checkpoints; // Array of checkpoints you can drag and drop in Unity Inspector
    private int currentCheckpoint = 0; // Start at checkpoint 0
    private int lapCount = 0; // Track the number of laps completed
    public int lapToFinish = 1;
    private bool lapStarted = false; // Flag to track if the lap has started
    public Button restartButton; // Reference to the restart button

    public TextMeshProUGUI currentLapTimeText;
    public TextMeshProUGUI bestLapTimeText;
    public TextMeshProUGUI lapCountText;

    private float currentLapTime = 0f;
    private float bestLapTime = Mathf.Infinity;
    private bool isTimingLap = false;
    private void Start()
    {
        // Initialize checkpoints: only the first one should be active at the start
        Debug.Log("Starting LapManager...");
        for (int i = 0; i < checkpoints.Length; i++)
        {
            bool isFirstCheckpoint = i == 0;
            checkpoints[i].Activate(isFirstCheckpoint);
            Debug.Log("Checkpoint " + i + " initialized. Active: " + isFirstCheckpoint);
        }
        lapStarted = true;
        isTimingLap = true;
        Debug.Log("Lap started immediately at game start.");
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(RestartGame); // Attach restart functionality
        }
    }

    private void Update()
    {
        if (isTimingLap)
        {
            currentLapTime += Time.deltaTime;
            currentLapTimeText.text = $"Current Lap: {currentLapTime:F2}s";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("CURRENT: " + currentCheckpoint);
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();
        
        // Debug log to check what object triggered the collider
        Debug.Log("Trigger hit by: " + other.gameObject.name);

        if (checkpoint != null)
        {
            Debug.Log("Trigger hit by checkpoint with index: " + checkpoint.checkpointIndex);

            // If the checkpoint is passed in the correct order
            if (checkpoint.checkpointIndex == currentCheckpoint)
            {
                // If it's the first checkpoint and the lap has not started, start lap tracking
                // if (!lapStarted && currentCheckpoint == 0)
                // {
                //     lapStarted = true; // Set lap started flag
                //     isTimingLap = true;
                //     Debug.Log("Lap tracking started!");
                // }

                // Log when passing a checkpoint
                Debug.Log("Checkpoint " + currentCheckpoint + " passed!");

                // Deactivate the current checkpoint
                Debug.Log("Deactivating checkpoint " + currentCheckpoint);
                checkpoints[currentCheckpoint].Activate(false);

                // Move to the next checkpoint
                currentCheckpoint++;
                Debug.Log("Moved to next checkpoint. Current checkpoint: " + currentCheckpoint);

                // If all checkpoints are passed, complete the lap
                if (currentCheckpoint >= checkpoints.Length)
                {
                    lapCount++;
                    lapCountText.text = $"Lap: {lapCount}/{lapToFinish}";
                    Debug.Log("Lap Complete! Total Laps: " + lapCount);
                    currentCheckpoint = 0; // Reset to start a new lap
                    lapStarted = false; // Reset lapStarted to allow new lap to start
                    isTimingLap = false;
                    // Reactivate the first checkpoint after completing a lap
                    Debug.Log("Reactivating checkpoint 0");
                    checkpoints[currentCheckpoint].Activate(true);

                    if (currentLapTime < bestLapTime)
{
    bestLapTime = currentLapTime;
    bestLapTimeText.text = $"Best Lap: {bestLapTime:F2}s";
}
                    isTimingLap = true;
                    if (lapCount >= lapToFinish)
                    {
                        // Show restart button when the target lap count is reached
                        if (restartButton != null)
                        {
                            restartButton.gameObject.SetActive(true);
                            Debug.Log("Lap goal reached. Displaying Restart button.");
                        }
                    }
                    currentLapTime = 0f;


                }
                else
                {
                    // Activate the next checkpoint
                    Debug.Log("Activating checkpoint " + currentCheckpoint);
                    checkpoints[currentCheckpoint].Activate(true);
                }
            }
            else
            {
                // Debug log for wrong checkpoint order
                Debug.Log("Wrong checkpoint! You cannot go backward. Expected checkpoint: " + currentCheckpoint);
            }
        }
        else
        {
            // Debug log when the collider hit does not belong to a checkpoint
            Debug.Log("Collider hit is not a checkpoint.");
        }
        Debug.Log("CURRENTEND: " + currentCheckpoint);
    }

    private void RestartGame()
{
    // Reload the current scene to restart the game
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

}
