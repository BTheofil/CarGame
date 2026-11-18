using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour {

    [Header("Motor")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 32f;
    [SerializeField] private float maxReverseSpeed = 10f;
    [SerializeField] private float brakeDeceleration = 26f;
    [SerializeField] private float coastDeceleration = 0.8f;

    [Header("Steer")]
    [SerializeField] private float steerRate = 80f;
    [SerializeField] private float steerFullSpeed = 8f;

    [Header("Grip")]
    [SerializeField] private float grip = 12f;
    [SerializeField] private float slideAllowance = 1.8f;
    [SerializeField] private float spinThreshold = 10f;
    [SerializeField] private float spinRate = 4f;

    [Header("Impact")]
    [SerializeField] private float impactDecay = 4f;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask = 1 << 10;
    [SerializeField] private float groundCheckDistance = 0.8f;

    private Rigidbody rb;
    private ICarInputProvider input;
    private float forwardSpeed;
    private Vector3 appliedVelocity;
    private Vector3 impact;
    public float SpeedRatio => Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);
    public float MaxSpeed => maxSpeed;
    public float LateralGrip => grip * slideAllowance;
    public float ImpactSpeed { get; private set; }
    private bool IsGrounded { get; set; }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        input = GetComponent<ICarInputProvider>();
    }

    private void FixedUpdate() {
        Vector3 velocity = rb.linearVelocity;
        Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);

        Vector3 injected = planarVelocity - appliedVelocity;
        ImpactSpeed = injected.magnitude;

        IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, groundCheckDistance, groundMask);
        if (!IsGrounded) {
            appliedVelocity = planarVelocity;
            return;
        }

        float deltaTime = Time.fixedDeltaTime;
        impact = Vector3.MoveTowards(impact + injected, Vector3.zero, impactDecay * deltaTime);
        planarVelocity -= impact;

        Vector3 heading = Steer(planarVelocity, deltaTime);
        float alongHeading = Vector3.Dot(planarVelocity, heading);
        Vector3 slide = Vector3.MoveTowards(planarVelocity - heading * alongHeading, Vector3.zero, grip * deltaTime);

        forwardSpeed = Drive(alongHeading, deltaTime);

        Vector3 result = heading * forwardSpeed + slide + impact;
        appliedVelocity = result;
        rb.linearVelocity = new Vector3(result.x, velocity.y, result.z);
    }

    private Vector3 Steer(Vector3 planarVelocity, float deltaTime) {
        float speed = planarVelocity.magnitude;
        float corneringLimit = speed > 0.1f ? grip * slideAllowance * Mathf.Rad2Deg / speed : steerRate;
        float traction = Mathf.Clamp01(speed / steerFullSpeed);
        float reverseSign = forwardSpeed < -0.5f && speed < maxReverseSpeed ? -1f : 1f;
        float lateralSpeed = Vector3.Dot(planarVelocity, transform.right);

        float spinYaw = -Mathf.MoveTowards(lateralSpeed, 0f, spinThreshold) * spinRate;
        float steerLimit = input.Steer * lateralSpeed > 0f ? corneringLimit + Mathf.Abs(spinYaw) : corneringLimit;
        float steerYaw = Mathf.Clamp(input.Steer * steerRate, -steerLimit, steerLimit) * traction * reverseSign;

        Quaternion rotation = rb.rotation * Quaternion.Euler(0f, (steerYaw + spinYaw) * deltaTime, 0f);
        rb.MoveRotation(rotation);
        return rotation * Vector3.forward;
    }

    private float Drive(float speed, float deltaTime) {
        if (input.Throttle > 0f) {
            float headroom = 1f - Mathf.Clamp01(speed / maxSpeed);
            return Mathf.MoveTowards(speed, maxSpeed, acceleration * input.Throttle * headroom * deltaTime);
        }
        if (input.Brake > 0f) {
            float power = speed > 0f ? brakeDeceleration : acceleration;
            return Mathf.MoveTowards(speed, -maxReverseSpeed, power * input.Brake * deltaTime);
        }
        return Mathf.MoveTowards(speed, 0f, coastDeceleration * deltaTime);
    }
}
