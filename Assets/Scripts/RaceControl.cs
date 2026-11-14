using UnityEngine;

public class RaceControl : MonoBehaviour {
    public static RaceControl Instance { get; private set; }
    private int remainCheckpoints = 4;

    private void Awake() {
        Instance = this;
    }

    public void CheckpointReached() {
        remainCheckpoints--;
        Debug.Log($"Checkpoint reached!");
        if (remainCheckpoints == 0) {
            Debug.Log("Race finished!");
        }
    }
}