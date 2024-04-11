using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class DiceManager : MonoBehaviour {

    [SerializeField] GameObject DiceRollerPrefab;
    [SerializeField] GameObject DicePrefab;

    PlayerHandler playerHandler;
    Camera mainCam; //= Camera.main;
    MatchManager.Team CurrentlyRollingForTeam = null;
    GameObject diceRoller;
    List<GameObject> diceToRoll;

    public void RollDice(MatchManager.Team teamToRollFor, int numberOfDice = 1) {
        mainCam.enabled = false;
        GUI.enabled = false;
        diceToRoll = new List<GameObject>();

        CurrentlyRollingForTeam = teamToRollFor;
        diceRoller = Instantiate(DiceRollerPrefab, Vector3.up * 1, Quaternion.identity);
        //GameObject dice = diceRoller.transform.Find("Dice").gameObject;
        for (int i = 0; i < numberOfDice; i++) {
            GameObject dice = Instantiate(DicePrefab);
            dice.name = "Dice";
            dice.transform.SetParent(diceRoller.transform);
            Rigidbody diceRB = dice.GetComponent<Rigidbody>();
            DiceHandler handler = dice.GetComponent<DiceHandler>();
            GameObject flashText = handler.FlashText;
            if (flashText != null) {
                Text titleText = flashText.transform.Find("TitleText").GetComponent<Text>();
                string defaultText = titleText.text;
                string hexColor = ColorUtility.ToHtmlStringRGB(teamToRollFor.Color);
                titleText.text = defaultText.Replace("TEAMA_NAME", "<color=#" + hexColor + ">" + teamToRollFor.Name + "</color>");
            } else { Debug.LogWarning("dice's FlashText could not be found."); }

            dice.GetComponent<Renderer>().material.color = teamToRollFor.Color;

            diceToRoll.Add(dice);
        }


        //while (diceRB.IsSleeping() == false || handler.launched == false) {
        //    yield return null;
        //}
        //yield return new WaitForSeconds(3);
    }
    void RollDiceFinished() {
        if (CurrentlyRollingForTeam == null) { return; }

        List<int> returnList = new List<int>();

        foreach (GameObject dice in diceToRoll) {
            //GameObject dice = diceRoller.transform.Find("Dice").gameObject;
            Rigidbody diceRB = dice.GetComponent<Rigidbody>();
            DiceHandler handler = dice.GetComponent<DiceHandler>();
            int result = handler.CurrentOutput;
            returnList.Add(result);
        }

        GameObject.Destroy(diceRoller);
        GUI.enabled = true;
        mainCam.enabled = true;

        //if (result <= 0) { RollDice(); return; }
        MatchManager.Team TEMP_CurrentlyRollingForTeam = CurrentlyRollingForTeam; //this is needed to make sure this.CurrentlyRollingForTeam is null in case matchManager.PlayerAction_ReportDiceRollResult immediately calls this.RollDice again.
        CurrentlyRollingForTeam = null;
        playerHandler.RollDiceFinished(TEMP_CurrentlyRollingForTeam, returnList);
    }

    // Start is called before the first frame update
    void Start() {
        mainCam = Camera.main;
        playerHandler = GetComponent<PlayerHandler>();
    }

    // Update is called once per frame
    void Update() {
        if (diceRoller) {
            int diceAsleep = 0;
            foreach (GameObject dice in diceToRoll) {
                //GameObject dice = diceRoller.transform.Find("Dice").gameObject;
                Rigidbody diceRB = dice.GetComponent<Rigidbody>();
                DiceHandler handler = dice.GetComponent<DiceHandler>();

                if (handler.timeSinceSleep > 1) {
                    diceAsleep++;
                }
            }
            if (diceAsleep >= diceToRoll.Count) {
                RollDiceFinished();
            }
        }
    }
}
