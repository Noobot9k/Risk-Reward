using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextHandler : MonoBehaviour {

    TextMesh textMesh;
    bool reversing = false;

    void Start() {
    }
    void OnEnable() {
        textMesh = GetComponent<TextMesh>();
        textMesh.lineSpacing = 0;
        textMesh.color -= new Color(0,0,0,1);
    }
    void Update() {
        if (!gameObject.active) { return; }
        if (!reversing) {
            textMesh.lineSpacing += .75f * Time.deltaTime;
            textMesh.color += new Color(0, 0, 0, 4 * Time.deltaTime);
            if (textMesh.color.a >= 1) {
                reversing = true;
            }
        } else {
            textMesh.lineSpacing += .3f * Time.deltaTime;
            textMesh.color -= new Color(0, 0, 0, .4f * Time.deltaTime);
            if (textMesh.color.a <= 0) {
                GameObject.Destroy(gameObject);
            }
        }
    }
}
