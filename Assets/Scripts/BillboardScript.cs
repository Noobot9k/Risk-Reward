using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardScript : MonoBehaviour {

    [System.Serializable]
    public enum BillboardTypeEnum { Align, Point }

    [Tooltip("Align means the billboarded object will align its rotation with that of the camera. Point means the billboarded object will face toward the camera which can cause it to have some roll applied to it.")]
    public BillboardTypeEnum billboardType = BillboardTypeEnum.Point;
    public Vector3 rotationalOffset = new Vector3();
    public Vector3 positionalOffset = new Vector3(0,.5f,0);

    void Start() {

    }

    void LateUpdate() {
        Camera cam = Camera.main;
        if (cam) {
            if (billboardType == BillboardTypeEnum.Align) {
                transform.rotation = cam.transform.rotation * Quaternion.Euler(0, 180, 0);
            } else if (billboardType == BillboardTypeEnum.Point) {
                transform.LookAt(cam.transform);
            }
            transform.position = transform.parent.position + transform.rotation * positionalOffset;
            transform.rotation = transform.rotation * Quaternion.Euler(rotationalOffset);
        }
    }
}
