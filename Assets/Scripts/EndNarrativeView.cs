using UnityEngine;

public class ExitNarrativeHallTrigger : MonoBehaviour
{
    [SerializeField] private Player player; // Reference to the player script

    private void Start()
    {
        // Automatically find the player object by tag and get the Player script component
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player object not found. Please ensure the player is tagged as 'Player'.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            // Disable Narrative View by setting isInNarrativeHall to false
            player.SetNarrativeHallMode(false);
        }
    }
}