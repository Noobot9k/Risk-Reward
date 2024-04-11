using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFOVHandler : MonoBehaviour {

    public float TargetFOV = -1;
    Camera camera;

    void Start() {
        camera = GetComponent<Camera>();
        if (TargetFOV == -1) {
            TargetFOV = camera.fieldOfView;
        }
    }

    void FixedUpdate() {
        
        float inverseRatio = (float)camera.pixelHeight / camera.pixelWidth;
        float hFOV = TargetFOV;

        var radAngle = camera.fieldOfView * Mathf.Deg2Rad;
        var radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * camera.aspect);
        var currentHFOV = Mathf.Rad2Deg * radHFOV;

        if (camera.aspect < 1) {//camera.aspect < 1) {
            float hFOVrad = hFOV * Mathf.Deg2Rad;
            float camH = Mathf.Tan(hFOVrad * .5f) / camera.aspect;
            float vFOVrad = Mathf.Atan(camH) * 2;
            camera.fieldOfView = vFOVrad * Mathf.Rad2Deg;
        } else {
            camera.fieldOfView = TargetFOV;
        }
    }

}
