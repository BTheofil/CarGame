using System;
using UnityEngine;

[Serializable]
public class BotSettings {
    public float lookahead = 12f;
    public float steerSensitivity = 35f;
    public float brakePower = 18f;
    public float brakingRange = 45f;
    public float sampleStep = 5f;
    public float brakeMargin = 4f;
}

public readonly struct DriveCommand {
    public float Steer { get; }
    public float Throttle { get; }
    public float Brake { get; }

    public DriveCommand(float steer, float throttle, float brake) {
        Steer = steer;
        Throttle = throttle;
        Brake = brake;
    }
}

public class BotDriver {

    private readonly ITrackPath path;
    private readonly BotSettings settings;
    private readonly float maxSpeed;
    private readonly float lateralGrip;

    public BotDriver(ITrackPath path, BotSettings settings, float maxSpeed, float lateralGrip) {
        this.path = path;
        this.settings = settings;
        this.maxSpeed = maxSpeed;
        this.lateralGrip = lateralGrip;
    }

    public DriveCommand Drive(Vector3 position, Vector3 forward, float speed, float distance) {
        float error = TargetSpeed(distance) - speed;
        return new DriveCommand(
            SteerTowards(position, forward, distance),
            Mathf.Clamp01(error),
            Mathf.Clamp01(-error / settings.brakeMargin));
    }

    private float SteerTowards(Vector3 position, Vector3 forward, float distance) {
        Vector3 toTarget = Flat(path.PositionAt(distance + settings.lookahead) - position);
        float angle = Vector3.SignedAngle(Flat(forward), toTarget, Vector3.up);
        return Mathf.Clamp(angle / settings.steerSensitivity, -1f, 1f);
    }

    private float TargetSpeed(float distance) {
        float limit = maxSpeed;
        for (float ahead = 0f; ahead <= settings.brakingRange; ahead += settings.sampleStep) {
            float corner = CornerSpeed(distance + ahead);
            limit = Mathf.Min(limit, Mathf.Sqrt(corner * corner + 2f * settings.brakePower * ahead));
        }
        return limit;
    }

    private float CornerSpeed(float distance) {
        float radius = Radius(
            path.PositionAt(distance - settings.sampleStep),
            path.PositionAt(distance),
            path.PositionAt(distance + settings.sampleStep));
        return Mathf.Min(maxSpeed, Mathf.Sqrt(lateralGrip * radius));
    }

    private static float Radius(Vector3 a, Vector3 b, Vector3 c) {
        float doubleArea = Vector3.Cross(b - a, c - a).magnitude;
        if (doubleArea < 0.0001f) return float.MaxValue;
        return Vector3.Distance(a, b) * Vector3.Distance(b, c) * Vector3.Distance(c, a) / (2f * doubleArea);
    }

    private static Vector3 Flat(Vector3 value) => new Vector3(value.x, 0f, value.z);
}
