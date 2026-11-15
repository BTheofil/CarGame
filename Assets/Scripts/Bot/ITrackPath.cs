using UnityEngine;

public interface ITrackPath {
    float Length { get; }
    float DistanceOf(Vector3 position);
    Vector3 PositionAt(float distance);
}
