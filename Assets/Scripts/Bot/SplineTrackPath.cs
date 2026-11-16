using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineTrackPath : MonoBehaviour, ITrackPath {

    private const int Oversampling = 8;

    [SerializeField, Min(0.25f)] private float sampleSpacing = 1f;
    [SerializeField, Min(1f)] private float searchRange = 8f;

    private TrackSamples samples;

    public float Length => samples.Length;

    private void Awake() {
        Vector3[] fine = SampleByCurveTime(GetComponent<SplineContainer>());
        samples = ResampleByDistance(fine, DistancesAlong(fine));
    }

    public float DistanceOf(Vector3 position) => samples.Nearest(position);

    public float DistanceNear(Vector3 position, float near) => samples.NearestAround(position, near, searchRange);

    public Vector3 PositionAt(float distance) => samples.PositionAt(distance);

    public Vector3 DirectionAt(float distance) => samples.DirectionAt(distance);

    public Vector3 PointBeside(float distance, float offset) => samples.PointBeside(distance, offset);

    private Vector3[] SampleByCurveTime(SplineContainer container) {
        int steps = Mathf.Max(64, Mathf.RoundToInt(container.Spline.GetLength() / sampleSpacing) * Oversampling);
        Vector3[] fine = new Vector3[steps + 1];

        for (int i = 0; i <= steps; i++) {
            fine[i] = container.EvaluatePosition((float)i / steps);
        }
        return fine;
    }

    private static float[] DistancesAlong(Vector3[] fine) {
        float[] distances = new float[fine.Length];

        for (int i = 1; i < fine.Length; i++) {
            distances[i] = distances[i - 1] + Vector3.Distance(fine[i - 1], fine[i]);
        }
        return distances;
    }

    private TrackSamples ResampleByDistance(Vector3[] fine, float[] distances) {
        int count = Mathf.Max(3, Mathf.RoundToInt(distances[distances.Length - 1] / sampleSpacing));
        float spacing = distances[distances.Length - 1] / count;
        Vector3[] even = new Vector3[count];
        int cursor = 0;

        for (int i = 0; i < count; i++) {
            float target = i * spacing;
            while (cursor < fine.Length - 2 && distances[cursor + 1] < target) cursor++;
            even[i] = Vector3.Lerp(fine[cursor], fine[cursor + 1], Mathf.InverseLerp(distances[cursor], distances[cursor + 1], target));
        }
        return new TrackSamples(even, spacing);
    }
}
