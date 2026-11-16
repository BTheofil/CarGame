using UnityEngine;

public interface ITrackPath {
    float Length { get; }
    float DistanceOf(Vector3 position);
    float DistanceNear(Vector3 position, float near);
    Vector3 PositionAt(float distance);
    Vector3 DirectionAt(float distance);
    Vector3 PointBeside(float distance, float offset);
}
