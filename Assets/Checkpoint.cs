using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex; // Unique index for the checkpoint, set in Unity Inspector
    private bool isActive = false;

    // Activate or deactivate the checkpoint (e.g., turn on/off a visual indicator)
    public void Activate(bool active)
    {
        isActive = active;

        // You can implement any visual or functional logic for activation here.
        if (isActive)
        {
            Debug.Log("Checkpoint " + checkpointIndex + " activated!");
        }
        else
        {
            Debug.Log("Checkpoint " + checkpointIndex + " deactivated!");
        }
    }

    public bool IsActive()
    {
        return isActive;
    }
}
