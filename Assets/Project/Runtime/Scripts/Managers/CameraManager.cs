using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour {
    public static CameraManager instance;
    [field: SerializeField] public Camera MainCamera { get; private set; }
    [field: SerializeField] public CinemachineCamera VirtualCamera { get; private set; }
    [field: SerializeField] public Transform CameraTarget { get; private set; }
    [field: SerializeField] public bool DisableCameraTargetOnDeath { get; private set; } = true;
    [field: SerializeField] public CameraTarget DefaultCameraTarget { get; private set; }
    [field: SerializeField] public CameraTarget CurrentCameraTarget { get; private set; }

    [Header("Snap Parameters")]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField] private IntSO pixelsPerUnit;
    [SerializeField] public Vector3 snappedCurrentPosition;

    private void OnEnable() {
        // Player.OnPlayerSpawned += SetVirtualCameraTarget;
        // Player.OnPlayerDeath += DisableCameraTarget;
    }

    private void OnDisable() {
        // Player.OnPlayerSpawned -= SetVirtualCameraTarget;
        // Player.OnPlayerDeath -= DisableCameraTarget;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        if (MainCamera == null) MainCamera = this.GetMainCamera();
        if (VirtualCamera == null) VirtualCamera = MainCamera.transform.GetComponentInChildren<CinemachineCamera>();
    }

    private void LateUpdate() {
        if (!snapToPixelGrid) return;

        Vector3 snappedTargetPosition = MainCamera.transform.position;
        snappedTargetPosition = GetSnappedPosition(MainCamera.transform.position, pixelsPerUnit.Value);

        MainCamera.transform.position = snappedTargetPosition;
        snappedCurrentPosition = MainCamera.transform.position;
    }

    private void SetVirtualCameraTarget(Player player) {
        if (CurrentCameraTarget == null) CameraTarget = DefaultCameraTarget.transform;
        VirtualCamera.LookAt = CameraTarget;
        VirtualCamera.Follow = CameraTarget;
    }

    private void DisableCameraTarget(Player player) {
        if (DisableCameraTargetOnDeath) CameraTarget.transform.SetParent(null);
    }

    private Vector3 GetSnappedPosition(Vector3 position, float snapPPU) {
        float pixelGridSize = 1f / snapPPU;
        // float x = ((position.x * snapValue).Round() / snapValue);
        // float y = ((position.y * snapValue).Round() / snapValue);
        float x = ((position.x / pixelGridSize).Round() * pixelGridSize);
        float y = ((position.y / pixelGridSize).Round() * pixelGridSize);
        return new Vector3(x, y, position.z);
    }
}
