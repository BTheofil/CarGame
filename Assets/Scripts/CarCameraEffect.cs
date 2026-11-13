using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineFollow))]
public class CarCameraEffect : MonoBehaviour {

    [SerializeField] private CarController car;
    [SerializeField] private float extraDistance = 2.5f;
    [SerializeField] private float extraFieldOfView = 12f;
    [SerializeField] private float responsiveness = 3f;

    private CinemachineCamera cam;
    private CinemachineFollow follow;
    private Vector3 restOffset;
    private float restFieldOfView;
    private float speedRatio;

    private void Awake() {
        cam = GetComponent<CinemachineCamera>();
        follow = GetComponent<CinemachineFollow>();
        restOffset = follow.FollowOffset;
        restFieldOfView = cam.Lens.FieldOfView;
    }

    private void Update() {
        if (car == null) return;

        speedRatio = Mathf.Lerp(speedRatio, car.SpeedRatio, responsiveness * Time.deltaTime);
        follow.FollowOffset = restOffset - Vector3.forward * (extraDistance * speedRatio);
        cam.Lens.FieldOfView = restFieldOfView + extraFieldOfView * speedRatio;
    }
}
