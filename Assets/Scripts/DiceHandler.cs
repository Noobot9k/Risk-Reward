using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceHandler : MonoBehaviour {

    public GameObject FlashText;
    [SerializeField] TextMesh InstructionText;
    [SerializeField] TextMesh SelectedNumberDisplayText;
    [SerializeField] Text ResultText;

    public bool physicsEnabled = false;
    public bool launched = false;
    public int CurrentOutput = 0;

    public float timeSinceSleep = -1;
    float SleepStartTick = -1;
    bool lastPhysicsEnabled = false;
    bool lastLaunched = false;

    Rigidbody rigidbody;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();

        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        rigidbody.maxAngularVelocity = 50;

        transform.rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
    }
    void Update() {
        if (lastPhysicsEnabled != physicsEnabled) {
            if (physicsEnabled) {
                rigidbody.constraints = RigidbodyConstraints.None;
                InstructionText.gameObject.SetActive(false);
            } else {
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            lastPhysicsEnabled = physicsEnabled;
        }
        if (lastLaunched != launched) {
            //SelectedNumberDisplayText.gameObject.SetActive(true);
            lastLaunched = launched;
        }


        int sides = 0;
        if (Vector3.Dot(Vector3.up, transform.forward) > .666) {
            sides = 6;
        } else if (Vector3.Dot(Vector3.up, transform.forward) < -.666) {
            sides = 1;
        } else if (Vector3.Dot(Vector3.up, transform.up) > .666) {
            sides = 3;
        } else if (Vector3.Dot(Vector3.up, transform.up) < -.666) {
            sides = 4;
        } else if (Vector3.Dot(Vector3.up, transform.right) > .666) {
            sides = 2;
        } else if (Vector3.Dot(Vector3.up, transform.right) < -.666) {
            sides = 5;
        }
        CurrentOutput = sides;
        SelectedNumberDisplayText.text = sides.ToString() + "!"; //"...";

        if (rigidbody.IsSleeping() ) {
            SelectedNumberDisplayText.gameObject.SetActive(false);
            ResultText.text = sides.ToString() + "!";
            //ResultText.gameObject.SetActive(true);
            SelectedNumberDisplayText.gameObject.SetActive(true);

            if (SleepStartTick == -1 && launched == true) {
                SleepStartTick = Time.time;
            }
        }
        if (SleepStartTick != -1) {
            timeSinceSleep = Time.time - SleepStartTick;
        }

    }
}
