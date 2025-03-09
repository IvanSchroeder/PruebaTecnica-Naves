using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using Codice.Client.BaseCommands;
using UnityEngine.InputSystem;

public class CameraTarget : MonoBehaviour {
    // [field: SerializeField] public Transform CameraCenter { get; private set; }
    // [field: SerializeField] public Vector3 TargetPosition { get; private set; }
    // [field: SerializeField] public bool Lock { get; private set; }
    // [field: SerializeField] public bool InstantSnapOverride { get; private set; }
    // [field: SerializeField] public bool IsLerping { get; private set; }
    // [field: SerializeField] public float LerpSpeed { get; private set; }
    // [field: SerializeField] public float LerpDelay { get; private set; }
    // [field: SerializeField] public float ElapsedTime { get; private set; }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float xThreshold;
    [SerializeField] private float yThreshold;

    private void Update () {
        AimLogic();
    }

    private void AimLogic() {
        Vector3 mousePos = mainCamera.ScreenToWorld(Input.mousePosition);
        Vector3 targetPos = (player.position + mousePos) / 2;

        targetPos.x = Mathf.Clamp(targetPos.x, -xThreshold + player.position.x, xThreshold + player.position.x);
        targetPos.y = Mathf.Clamp(targetPos.y, -yThreshold + player.position.y, yThreshold + player.position.y);

        this.transform.position = targetPos;
    }

    // private void Awake() {
    //     if (CameraCenter == null) CameraCenter = this.transform.parent;
    //     IsLerping = false;
    // }

    // private void Update() {
    //     if (Lock) return;
        
    //     if (!InstantSnapOverride && transform.position != TargetPosition) {
    //         transform.position = Vector3.MoveTowards(transform.position, TargetPosition, LerpSpeed * Time.deltaTime);

    //         ElapsedTime += Time.deltaTime;

    //         if (transform.position == TargetPosition) { 
    //             transform.localPosition = Vector3.zero;
    //             IsLerping = false;
    //         }
    //     }
    //     else if (InstantSnapOverride || transform.position == TargetPosition) {
    //         IsLerping = false;
    //         transform.localPosition = Vector3.zero;
    //     }
    // }

    // public void OffsetTargetPosition(Vector3 direction, float distance) {
    //     Vector3 offset = direction * distance;
    //     TargetPosition = CameraCenter.position + offset;
    // }

    // public void SetTargetPosition(Vector3 position, float distance = 0f, bool offsetOffCenter = false, bool instantSnap = false) {
    //     if (offsetOffCenter) {
    //         Vector3 offset = position * distance;
    //         TargetPosition = CameraCenter.position + offset;
    //     }
    //     else TargetPosition = position;

    //     if (instantSnap || InstantSnapOverride) {
    //         transform.position = position;
    //     }
    //     else {
    //         ElapsedTime = 0f;
    //         IsLerping = true;
    //     }
    // }

    // public void ResetTargetPosition() {
    //     SetTargetPosition(CameraCenter.position);
    // }

    // public bool CheckIfReachedTarget() {
    //     bool target = (transform.position == TargetPosition);
    //     return target;
    // }
}
