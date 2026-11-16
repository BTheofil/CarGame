using UnityEngine;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Rigidbody))]
public class BotInputProvider : MonoBehaviour, ICarInputProvider {

    [SerializeField] private SplineTrackPath track;
    [SerializeField] private BotSettings settings = new BotSettings();
    [SerializeField] private int lapsToDrive = 1;
    [SerializeField, Range(0.5f, 1f)] private float skill = 1f;
    [SerializeField] private float laneOffset;

    public float Steer { get; private set; }
    public float Throttle { get; private set; }
    public float Brake { get; private set; }

    private Rigidbody rb;
    private ITrackPath path;
    private BotDriver driver;
    private LapTracker laps;
    private bool finished;
    private float distance;

    public void Configure(SplineTrackPath track, float skill, float laneOffset) {
        this.track = track;
        this.skill = skill;
        this.laneOffset = laneOffset;
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        CarController car = GetComponent<CarController>();

        path = track;
        driver = new BotDriver(path, settings, car.MaxSpeed * skill, car.LateralGrip * skill, laneOffset);
        distance = path.DistanceOf(transform.position);
        laps = new LapTracker(path.Length, distance);
    }

    private void FixedUpdate() {
        distance = path.DistanceNear(transform.position, distance);
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
