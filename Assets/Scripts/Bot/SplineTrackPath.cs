using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class SplineTrackPath : MonoBehaviour, ITrackPath {

    [SerializeField, Min(0.1f)] private float sampleSpacing = 1f;

    private Vector3[] points;

    public float Length { get; private set; }

    private float Step => Length / points.Length;

    private void Awake() {
        SplineContainer container = GetComponent<SplineContainer>();
        Length = container.Spline.GetLength();
        points = new Vector3[Mathf.CeilToInt(Length / sampleSpacing)];

        for (int i = 0; i < points.Length; i++) {
            float t = container.Spline.ConvertIndexUnit(i * Step, PathIndexUnit.Distance, PathIndexUnit.Normalized);
            points[i] = container.EvaluatePosition(t);
        }
    }

    public float DistanceOf(Vector3 position) {
        int nearest = 0;
        for (int i = 1; i < points.Length; i++) {
            if ((points[i] - position).sqrMagnitude < (points[nearest] - position).sqrMagnitude) nearest = i;
        }
        return nearest * Step;
    }

    public Vector3 PositionAt(float distance) {
        float index = Mathf.Repeat(distance, Length) / Step;
        int i = (int)index;
        return Vector3.Lerp(points[i % points.Length], points[(i + 1) % points.Length], index - i);
    }
}
