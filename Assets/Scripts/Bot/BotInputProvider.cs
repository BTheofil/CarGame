using UnityEngine;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Rigidbody))]
public class BotInputProvider : MonoBehaviour, ICarInputProvider {

    [SerializeField] private SplineTrackPath track;
    [SerializeField] private BotSettings settings = new BotSettings();
    [SerializeField] private int lapsToDrive = 1;

    public float Steer { get; private set; }
    public float Throttle { get; private set; }
    public float Brake { get; private set; }

    private Rigidbody rb;
    private ITrackPath path;
    private BotDriver driver;
    private LapTracker laps;
    private bool finished;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        CarController car = GetComponent<CarController>();

        path = track;
        driver = new BotDriver(path, settings, car.MaxSpeed, car.LateralGrip);
        laps = new LapTracker(path.Length, path.DistanceOf(transform.position));
    }

    private void FixedUpdate() {
        float distance = path.DistanceOf(transform.position);
        laps.Advance(distance);

        if (laps.Laps >= lapsToDrive) {
            if (!finished) {
                finished = true;
                Debug.Log($"{name} finished {laps.Laps} lap(s)");
            }
            Steer = Throttle = Brake = 0f;
            return;
        }

        DriveCommand command = driver.Drive(
            transform.position,
            transform.forward,
            Vector3.Dot(rb.linearVelocity, transform.forward),
            distance);

        Steer = command.Steer;
        Throttle = command.Throttle;
        Brake = command.Brake;
    }
}
