using UnityEngine;

public class PlayerInputProvider : MonoBehaviour, ICarInputProvider {

    [SerializeField] private float steerDeadZone = 0.08f;

    private InputSystemActions actions;

    public float Steer { get; private set; }
    public float Throttle { get; private set; }
    public float Brake { get; private set; }

    private void Awake() {
        actions = new InputSystemActions();
        actions.CarMovement.Enable();
    }

    private void OnDestroy() {
        actions.CarMovement.Disable();
        actions.Dispose();
    }

    private void Update() {
        Vector2 move = actions.CarMovement.Move.ReadValue<Vector2>();

        Steer = Mathf.Abs(move.x) < steerDeadZone ? 0f : move.x;
        Throttle = Mathf.Max(0f, move.y);
        Brake = Mathf.Max(0f, -move.y);
    }
}
