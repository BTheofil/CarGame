using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Checkpoint : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        CarController car = other.GetComponentInParent<CarController>();
        if (car != null) {
            RaceControl.Instance.CheckpointReached();
        }
    }
}
