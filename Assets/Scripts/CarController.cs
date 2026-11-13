using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour {

    [Header("Motor")]
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float maxSpeed = 32f;
    [SerializeField] private float maxReverseSpeed = 10f;
    [SerializeField] private float brakeDeceleration = 26f;
    [SerializeField] private float coastDeceleration = 1f;

    [Header("Kormányzás")]
    [SerializeField] private float steerRate = 130f;
    [SerializeField] private float steerFullSpeed = 6f;

    [Header("Tapadás")]
    [SerializeField] private float grip = 12f;
    [SerializeField] private float slideAllowance = 1.8f;
    [SerializeField] private float spinThreshold = 6f;
    [SerializeField] private float spinRate = 6f;

    [Header("Talaj")]
    [SerializeField] private LayerMask groundMask = 1 << 10;
    [SerializeField] private float groundCheckDistance = 0.8f;

    private Rigidbody rb;
    private ICarInputProvider input;
    private float forwardSpeed;

    public float SpeedKmh => forwardSpeed * 3.6f;
    public float SpeedRatio => Mathf.Clamp01(Mathf.Abs(forwardSpeed) / maxSpeed);
    public float SlipSpeed { get; private set; }
    public bool IsGrounded { get; private set; }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        input = GetComponent<ICarInputProvider>();
        GetComponent<Collider>().sharedMaterial = new PhysicsMaterial {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum
        };
    }

    private void FixedUpdate() {
        IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, groundCheckDistance, groundMask);
        if (!IsGrounded) return;

        float deltaTime = Time.fixedDeltaTime;
        Vector3 velocity = rb.linearVelocity;
        Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);

        Vector3 heading = Steer(planarVelocity, deltaTime);
        float alongHeading = Vector3.Dot(planarVelocity, heading);
        Vector3 slide = Vector3.MoveTowards(planarVelocity - heading * alongHeading, Vector3.zero, grip * deltaTime);

        forwardSpeed = Drive(alongHeading, deltaTime);
        SlipSpeed = slide.magnitude;

        Vector3 result = heading * forwardSpeed + slide;
        rb.linearVelocity = new Vector3(result.x, velocity.y, result.z);
    }

    private Vector3 Steer(Vector3 planarVelocity, float deltaTime) {
        float speed = planarVelocity.magnitude;
        float corneringLimit = speed > 0.1f ? grip * slideAllowance * Mathf.Rad2Deg / speed : steerRate;
        float traction = Mathf.Clamp01(speed / steerFullSpeed);
        float reverseSign = forwardSpeed < -0.5f ? -1f : 1f;
        float lateralSpeed = Vector3.Dot(planarVelocity, transform.right);

        float steerYaw = Mathf.Clamp(input.Steer * steerRate, -corneringLimit, corneringLimit) * traction * reverseSign;
        float spinYaw = -Mathf.MoveTowards(lateralSpeed, 0f, spinThreshold) * spinRate;

        Quaternion rotation = rb.rotation * Quaternion.Euler(0f, (steerYaw + spinYaw) * deltaTime, 0f);
        rb.MoveRotation(rotation);
        return rotation * Vector3.forward;
    }

    private float Drive(float speed, float deltaTime) {
        if (input.Throttle > 0f) {
            return Mathf.MoveTowards(speed, maxSpeed, acceleration * input.Throttle * deltaTime);
        }
        if (input.Brake > 0f) {
            float power = speed > 0f ? brakeDeceleration : acceleration;
            return Mathf.MoveTowards(speed, -maxReverseSpeed, power * input.Brake * deltaTime);
        }
        return Mathf.MoveTowards(speed, 0f, coastDeceleration * deltaTime);
    }
}
