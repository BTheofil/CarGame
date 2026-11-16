using UnityEngine;

public class TrackSamples {

    private readonly Vector3[] points;
    private readonly Vector3[] directions;
    private readonly float spacing;

    public float Length => spacing * points.Length;

    public TrackSamples(Vector3[] evenlySpacedPoints, float spacing) {
        points = evenlySpacedPoints;
        directions = new Vector3[points.Length];
        this.spacing = spacing;

        for (int i = 0; i < points.Length; i++) {
            directions[i] = (points[Wrap(i + 1)] - points[i]).normalized;
        }
    }

    public Vector3 PositionAt(float distance) {
        float index = IndexAt(distance);
        int whole = (int)index;
        return Vector3.Lerp(points[Wrap(whole)], points[Wrap(whole + 1)], index - whole);
    }

    public Vector3 DirectionAt(float distance) => directions[Wrap((int)IndexAt(distance))];

    public Vector3 PointBeside(float distance, float offset) =>
        PositionAt(distance) + Vector3.Cross(Vector3.up, DirectionAt(distance)) * offset;

    public float Nearest(Vector3 position) => NearestInRange(position, 0, points.Length);

    public float NearestAround(Vector3 position, float distance, float range) {
        int span = Mathf.CeilToInt(range / spacing);
        return NearestInRange(position, Wrap((int)IndexAt(distance)) - span, span * 2 + 1);
    }

    private float NearestInRange(Vector3 position, int start, int count) {
        int best = start;
        float bestSqrDistance = float.MaxValue;

        for (int offset = 0; offset < count; offset++) {
            int index = Wrap(start + offset);
            float sqrDistance = (points[index] - position).sqrMagnitude;

            if (sqrDistance < bestSqrDistance) {
                bestSqrDistance = sqrDistance;
                best = index;
            }
        }
        return best * spacing;
    }

    private float IndexAt(float distance) => Mathf.Repeat(distance, Length) / spacing;

    private int Wrap(int index) => (index % points.Length + points.Length) % points.Length;
}
