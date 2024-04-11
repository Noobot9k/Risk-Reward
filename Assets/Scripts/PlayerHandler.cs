using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerHandler : MonoBehaviour {

    [System.Serializable]
    public class TouchData {
        public int TouchId;
        public GameObject InitialTarget;
        public Vector2 InitialPosition;
        public GameObject CurrentTarget;
        public Vector2 CurrentPosition;
        public Vector3 CurrentPositionInWorld;
        public bool ModifiedThisFrame = true;
    }
    //[System.Serializable]
    public class ManeuverData {
        public TerritoryManager From;
        public TerritoryManager To;
        public GameObject ArrowIndicator;

        public ManeuverData(TerritoryManager f, TerritoryManager t) {
            From = f;
            To = t;
        }
    }

    public MatchManager.Team CurrentlyControlledTeam;
    public List<MatchManager.Team> ControlledTeams = new List<MatchManager.Team>();
    MatchManager matchManager;
    DiceManager diceManager;
    Camera mainCam; //= Camera.main;

    [SerializeField] float diceAcceleration = 1.5f;
    [SerializeField] float diceMaxSpeed = 5;
    [SerializeField] float diceHeight = 4;
    [SerializeField, Tooltip("the dice's velocity vector3 is applied as torque to give at least some sourse of spin on the dice. The dice can beging spinning too fast. If this happens, lower this value.")]
    float diceTorqueMultiplier = .1f;

    [SerializeField] GameObject ArrowIndicatorPrefab;
    [SerializeField] Canvas GUI;
    GameObject GUI_Main;
    [SerializeField] Button nextButton;
    [SerializeField] Text TeamTurnText;
    [SerializeField] Text PhaseText;
    [SerializeField] GameObject DeployableUnitsLeftGUI;
    [SerializeField] Text DeployableUnitsLeft_ContentText;
    [SerializeField] GameObject ManeuverGUI;
    [SerializeField] GameObject EndScreen;
    [SerializeField] GameObject SetupGUI;
    Button ManeuverGUI_Plus1Button;
    Button ManeuverGUI_PlusAllButton;
    Button ManeuverGUI_CloseButton;
    GameObject EndScreen_Background;
    Text EndScreen_TeamText;
    Button SetupGUI_Start;
    Slider SetupGUI_Slider;

    string defaultTeamTurnText;
    string defaultPhaseText;

    List<TouchData> TouchesList = new List<TouchData>();
    List<TerritoryManager> shownSelection = new List<TerritoryManager>();
    List<TerritoryManager> shownHover = new List<TerritoryManager>();

    public ManeuverData CurrentManeuver = null;

    public void EndGame(MatchManager.Team winningTeam) {
        CloseManeuver();
        GUI_Main.SetActive(false);

        EndScreen.SetActive(true);
        EndScreen_TeamText.text = winningTeam.Name;
        EndScreen_TeamText.color = winningTeam.Color;
    }
    public void RollDice(MatchManager.Team teamToRollFor, int numberOfDice = 1) {
        GUI_Main.SetActive(false);
        diceManager.RollDice(teamToRollFor, numberOfDice);
    }
    public void RollDiceFinished(MatchManager.Team teamPerformingAction, List<int> result) {
        GUI_Main.SetActive(true);
        matchManager.PlayerAction_ReportDiceRollResult(this, teamPerformingAction, result);
    }
    void NextButtonClicked() {
        matchManager.PlayerAction_ProgressToNextPhase(this);
        CloseManeuver();
    }
    void StartButtonClicked() {
        SetupGUI.SetActive(false);
        GUI_Main.SetActive(true);
        matchManager.PlayerAction_SetTeamCount((int)SetupGUI_Slider.value);
    }
    void Plus1ButtonClicked() {
        if (CurrentManeuver != null && matchManager.GetIsTeamsTurn(CurrentlyControlledTeam) ) {
            matchManager.PlayerAction_Maneuver(this, CurrentManeuver.From, CurrentManeuver.To, 1);
        }
    }
    void PlusAllButtonClicked() {
        if (CurrentManeuver != null && matchManager.GetIsTeamsTurn(CurrentlyControlledTeam) ) {
            matchManager.PlayerAction_Maneuver(this, CurrentManeuver.From, CurrentManeuver.To, CurrentManeuver.From.OccupantCount - 1);
            CloseManeuver();
        }
    }
    public void OpenManeuver(ManeuverData data) {
        print("opening maneuver...");
        if (CurrentManeuver != null) { CloseManeuver(); }
        CurrentManeuver = data;

        GameObject Arrow = Instantiate(ArrowIndicatorPrefab);
        Arrow.GetComponent<Renderer>().material.SetColor("Color_47C51489", CurrentManeuver.From.Occupant.Color);
        CurrentManeuver.ArrowIndicator = Arrow;
        Vector3 pos1 = CurrentManeuver.From.transform.position;
        Vector3 pos2 = CurrentManeuver.To.transform.position;

        Arrow.transform.position = pos1 + ((pos2 - pos1) / 2) + (Vector3.up * .1f);
        Arrow.transform.LookAt(pos2);
        Arrow.transform.localScale = new Vector3(Arrow.transform.localScale.x, Arrow.transform.localScale.y, (pos2 - pos1).magnitude * .1f);

        ManeuverGUI.SetActive(true);
        GUI_Main.SetActive(false);
    }
    void CloseManeuver() {
        if (CurrentManeuver != null) {
            GameObject.Destroy(CurrentManeuver.ArrowIndicator);
            GUI_Main.SetActive(true);
            ManeuverGUI.SetActive(false);
            CurrentManeuver = null;
            matchManager.PlayerAction_ReportFinishedCombatManeuver(this);
        }
    }

    void Start() {
        Input.simulateMouseWithTouches = false;
        mainCam = Camera.main;
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        diceManager = GetComponent<DiceManager>();

        GUI_Main = GUI.transform.Find("Main").gameObject;
        ManeuverGUI_Plus1Button = ManeuverGUI.transform.Find("+1Button").GetComponent<Button>();
        ManeuverGUI_PlusAllButton = ManeuverGUI.transform.Find("+AllButton").GetComponent<Button>();
        ManeuverGUI_CloseButton = ManeuverGUI.transform.Find("CloseButton").GetComponent<Button>();
        ManeuverGUI_Plus1Button.onClick.AddListener(Plus1ButtonClicked);
        ManeuverGUI_PlusAllButton.onClick.AddListener(PlusAllButtonClicked);
        ManeuverGUI_CloseButton.onClick.AddListener(CloseManeuver);
        EndScreen_Background = EndScreen.transform.Find("Background").gameObject;
        EndScreen_TeamText = EndScreen_Background.transform.Find("TeamText").GetComponent<Text>();
        SetupGUI_Start = SetupGUI.transform.Find("Start").GetComponent<Button>();
        SetupGUI_Slider = SetupGUI.transform.Find("Content").Find("Slider").GetComponent<Slider>();

        DeployableUnitsLeftGUI = GUI.transform.Find("DeployableUnitsLeft").gameObject;
        DeployableUnitsLeft_ContentText = DeployableUnitsLeftGUI.transform.Find("ContentText").GetComponent<Text>();

        nextButton.onClick.AddListener(NextButtonClicked);
        SetupGUI_Start.onClick.AddListener(StartButtonClicked);

        if (TeamTurnText) {
            defaultTeamTurnText = TeamTurnText.text;
        }
        if (PhaseText) {
            defaultPhaseText = PhaseText.text;
        }
    }
    void Update() {
        foreach (TerritoryManager shownSelectionElement in shownSelection) {
            shownSelectionElement.Selected = false;
        }
        shownSelection = new List<TerritoryManager>();
        foreach (TerritoryManager shownHoverElement in shownHover) {
            shownHoverElement.Hovered = false;
        }
        shownHover = new List<TerritoryManager>();

        if (ControlledTeams.Contains(matchManager.GetTeamFromTeamId(matchManager.TurnByTeamId))) {
            CurrentlyControlledTeam = matchManager.GetTeamFromTeamId(matchManager.TurnByTeamId);
        }

        //Collect and process touch data.
        foreach (TouchData touchData in TouchesList) {
            touchData.ModifiedThisFrame = false;
        }

        void ProccessInput(int touchId, Vector2 InputPixelScreenPos) {
            Ray touchRay;
            RaycastHit touchRayHit;
            Vector2 TouchPosition = InputPixelScreenPos;
            touchRay = Camera.main.ScreenPointToRay(TouchPosition);
            bool didTouchRayHit = Physics.Raycast(touchRay, out touchRayHit);

            //check for existing touch data
            TouchData existingData = null;
            foreach (TouchData touchData in TouchesList) {
                if (touchData.TouchId == touchId) {
                    existingData = touchData;
                }
            }

            //if no touch data exists, create.
            if (existingData == null) {
                existingData = new TouchData();
                existingData.TouchId = touchId;
                existingData.InitialPosition = InputPixelScreenPos;
                TouchesList.Add(existingData);
                existingData.InitialTarget = null;
                if (didTouchRayHit && !EventSystem.current.IsPointerOverGameObject()) {
                    existingData.InitialTarget = touchRayHit.collider.gameObject;
                }
            }

            //universal changes
            existingData.ModifiedThisFrame = true;
            existingData.CurrentPosition = InputPixelScreenPos;
            if (didTouchRayHit) {
                existingData.CurrentTarget = touchRayHit.collider.gameObject;
                existingData.CurrentPositionInWorld = touchRayHit.point;
            } else {
                existingData.CurrentTarget = null;
                existingData.CurrentPositionInWorld = Camera.main.transform.position + touchRay.direction * 4;
            }
        }

        if (Input.GetMouseButton(0) ) {
            ProccessInput(-1, Input.mousePosition);
            ////Get mouse target.
            //Ray touchRay;
            //RaycastHit touchRayHit;
            //Vector2 TouchPosition = Input.mousePosition; // / CurrentResolution;
            //touchRay = Camera.main.ScreenPointToRay(TouchPosition);
            //bool didTouchRayHit = Physics.Raycast(touchRay, out touchRayHit);

            //TouchData existingData = null;
            //foreach (TouchData touchData in TouchesList) {
            //    if (touchData.TouchId == -1) {
            //        existingData = touchData;
            //    }
            //}
            //if (existingData == null) {
            //    existingData = new TouchData();
            //    existingData.TouchId = -1;
            //    existingData.InitialPosition = Input.mousePosition;
            //    TouchesList.Add(existingData);
            //    if (didTouchRayHit && !EventSystem.current.IsPointerOverGameObject() ) {
            //        existingData.InitialTarget = touchRayHit.collider.gameObject;
            //    } else { existingData.InitialTarget = null; }
            //}
            //existingData.ModifiedThisFrame = true;
            //existingData.CurrentPosition = Input.mousePosition;
            //if (didTouchRayHit) {
            //    existingData.CurrentTarget = touchRayHit.collider.gameObject;
            //    existingData.CurrentPositionInWorld = touchRayHit.point;
            //} else {
            //    existingData.CurrentTarget = null;
            //    existingData.CurrentPositionInWorld = Camera.main.transform.position + touchRay.direction * 4;
            //}
        }
        foreach (Touch touch in Input.touches) {
            ProccessInput(touch.fingerId, touch.position);

            //TouchData existingData = null;
            //foreach (TouchData touchData in TouchesList) {
            //    if (touchData.TouchId == touch.fingerId) {
            //        existingData = touchData;
            //    }
            //}
            
            ////Get touch target.
            //Ray touchRay;
            //RaycastHit touchRayHit;
            //Vector2 TouchPosition = touch.position; // / CurrentResolution;
            //touchRay = Camera.main.ScreenPointToRay(TouchPosition);
            //bool didTouchRayHit = Physics.Raycast(touchRay, out touchRayHit);

            //if (existingData != null) {

            //} else {
            //    existingData = new TouchData();
            //    existingData.TouchId = touch.fingerId;
            //    existingData.InitialPosition = touch.position;
            //    TouchesList.Add(existingData);
            //    if (didTouchRayHit) {
            //        existingData.InitialTarget = touchRayHit.collider.gameObject;
            //        existingData.CurrentPositionInWorld = touchRayHit.point;
            //    } else {
            //        existingData.InitialTarget = null;
            //        existingData.CurrentPositionInWorld = Camera.main.transform.position + touchRay.direction * 4;
            //    }
            //}
            //existingData.CurrentPosition = touch.position;
            //existingData.ModifiedThisFrame = true;
            //if (didTouchRayHit && !EventSystem.current.IsPointerOverGameObject()) {
            //    existingData.CurrentTarget = touchRayHit.collider.gameObject;
            //} else { existingData.CurrentTarget = null; }
        }

        List<TouchData> toRemove = new List<TouchData>();
        foreach (TouchData touchData in TouchesList) {

            if (touchData.ModifiedThisFrame == false) { //input is released this frame.
                toRemove.Add(touchData);

                //perform phase based actions.
                if (touchData.InitialTarget && touchData.CurrentTarget) {
                    TerritoryManager initialManager = touchData.InitialTarget.GetComponent<TerritoryManager>();
                    TerritoryManager endManager = touchData.CurrentTarget.GetComponent<TerritoryManager>();
                    if (initialManager && endManager) {
                        if (matchManager.currentPhase == MatchManager.Phase.Deploy) { //DEPLOY
                            if (touchData.InitialTarget == touchData.CurrentTarget ) {
                                if (
                                        (
                                            matchManager.GetTeamOccupiedTerritoryCount(CurrentlyControlledTeam) > 0 && initialManager.Occupant.Name == CurrentlyControlledTeam.Name
                                        ) || (
                                            matchManager.GetTeamOccupiedTerritoryCount(CurrentlyControlledTeam) <= 0 && 
                                            (
                                                initialManager.Occupant == null || initialManager.Occupant.Name == ""
                                            )
                                        )
                                    )
                                //print("deploying at " + touchData.InitialTarget.name);
                                matchManager.PlayerAction_Deploy(this, initialManager, 1);
                            }
                        } else if (matchManager.currentPhase == MatchManager.Phase.Attack) { //ATTACK
                            if (initialManager.Occupant.Name == CurrentlyControlledTeam.Name && (endManager.Occupant == null || endManager.Occupant.Name != CurrentlyControlledTeam.Name) ) {

                                //Show visualization for attack options.
                                foreach (GameObject connectedTerritory in initialManager.ConnectedTerritories) {
                                    TerritoryManager connectedTerritoryManager = connectedTerritory.GetComponent<TerritoryManager>();
                                    shownSelection.Add(connectedTerritoryManager);
                                }

                                //perform the attack.
                                matchManager.PlayerAction_Attack(this, initialManager, endManager);
                            }
                        } else if (matchManager.currentPhase == MatchManager.Phase.Maneuver) { //MANEUVER
                            if (initialManager.Occupant.Name == CurrentlyControlledTeam.Name && endManager.Occupant.Name == CurrentlyControlledTeam.Name && initialManager != endManager && initialManager.OccupantCount > 1) {
                                if (CurrentManeuver == null) {
                                    matchManager.PlayerAction_Maneuver(this, initialManager, endManager, 1);
                                    if (initialManager.OccupantCount > 1) {
                                        OpenManeuver(new ManeuverData(initialManager, endManager));
                                    } else {
                                    }
                                    //CurrentManeuver = new ManeuverData(initialManager, endManager);
                                }
                                //matchManager.PlayerAction_Maneuver(this, initialManager, endManager, initialManager.OccupantCount - 1);
                            }
                        }
                    }
                }

                // old1

                //if target is a dice.
                if (touchData.InitialTarget && touchData.InitialTarget.layer == 9) {
                    GameObject dice = touchData.InitialTarget;
                    Rigidbody diceRigidbody = dice.GetComponent<Rigidbody>();
                    DiceHandler diceHandler = dice.GetComponent<DiceHandler>();

                    if (diceHandler) {
                        diceHandler.launched = true;
                        diceRigidbody.velocity *= 2f;
                    }

                }

            } else { // input is still being held this frame.

                //visualize current action.
                if (touchData.InitialTarget) {
                    TerritoryManager initialManager = touchData.InitialTarget.GetComponent<TerritoryManager>();
                    if (initialManager) {
                        shownHover.Add(initialManager);
                        if (touchData.CurrentTarget) {
                            TerritoryManager endManager = touchData.CurrentTarget.GetComponent<TerritoryManager>();
                            if (endManager) {
                                shownHover.Add(endManager);
                            }
                        }

                        if (matchManager.currentPhase == MatchManager.Phase.Deploy) {

                        } else if (matchManager.currentPhase == MatchManager.Phase.Attack) {
                            if (initialManager.Occupant != null && initialManager.Occupant.Name == CurrentlyControlledTeam.Name) {
                                //shownSelection.Add(initialManager);
                                foreach (GameObject territory in initialManager.ConnectedTerritories) {
                                    TerritoryManager manager = territory.GetComponent<TerritoryManager>();
                                    if (manager.Occupant == null || manager.Occupant.Name != CurrentlyControlledTeam.Name) {
                                        shownSelection.Add(manager);
                                    }
                                }
                            }
                        } else if (matchManager.currentPhase == MatchManager.Phase.Maneuver) {
                            if (initialManager.Occupant.Name == CurrentlyControlledTeam.Name) {
                                //shownSelection.Add(initialManager);
                                foreach (TerritoryManager territory in matchManager.GetTeamOccupiedTerritories(CurrentlyControlledTeam)) {
                                    if (territory != initialManager) {
                                        shownSelection.Add(territory);
                                    }
                                }
                            }
                        }
                    }
                }

                //if target is a dice.
                if (touchData.InitialTarget && touchData.InitialTarget.layer == 9) {
                    GameObject dice = touchData.InitialTarget;
                    Rigidbody diceRigidbody = dice.GetComponent<Rigidbody>();
                    DiceHandler diceHandler = dice.GetComponent<DiceHandler>();

                    if (diceHandler && diceHandler.physicsEnabled == false && diceHandler.launched == false) {
                        diceHandler.physicsEnabled = true;
                    }
                    if (diceHandler && diceHandler.launched == false) {
                        Vector3 diceTargetPosition = new Vector3(touchData.CurrentPositionInWorld.x, diceHeight, touchData.CurrentPositionInWorld.z);
                        Vector3 diceOffsetFromTarget = diceTargetPosition - dice.transform.position;
                        diceRigidbody.velocity = Vector3.MoveTowards(diceRigidbody.velocity, diceOffsetFromTarget * diceMaxSpeed, diceAcceleration);
                        diceRigidbody.AddTorque(diceOffsetFromTarget * diceTorqueMultiplier, ForceMode.VelocityChange);
                        //diceRigidbody.AddForce(Vector3.ClampMagnitude(diceOffsetFromTarget, 5)/5, ForceMode.Impulse);
                    }
                }
            }
        }
        //remove expired inputs from above loop.
        foreach (TouchData touchData in toRemove) {
            TouchesList.Remove(touchData);
        }

        //visualize options when no action is being taken yet.
        if (shownSelection.Count <= 0 && CurrentManeuver == null) {
            if (matchManager.currentPhase == MatchManager.Phase.Deploy) {
                if (matchManager.GetTeamOccupiedTerritoryCount(CurrentlyControlledTeam) > 0) {
                    foreach (TerritoryManager occupiedTerritory in matchManager.GetTeamOccupiedTerritories(CurrentlyControlledTeam)) {
                        shownSelection.Add(occupiedTerritory);
                    }
                } else {
                    foreach (TerritoryManager occupiedTerritory in matchManager.GetTeamOccupiedTerritories(null)) {
                        shownSelection.Add(occupiedTerritory);
                    }
                }
            } else if (matchManager.currentPhase == MatchManager.Phase.Attack) {
                foreach (TerritoryManager occupiedTerritory in matchManager.GetTeamOccupiedTerritories(CurrentlyControlledTeam)) {
                    shownSelection.Add(occupiedTerritory);
                }
            } else if (matchManager.currentPhase == MatchManager.Phase.Maneuver) {
                foreach (TerritoryManager occupiedTerritory in matchManager.GetTeamOccupiedTerritories(CurrentlyControlledTeam)) {
                    shownSelection.Add(occupiedTerritory);
                }
            }
        }

        //visualize proccessed options and actions.
        if (CurrentManeuver != null) {
            shownHover.Add(CurrentManeuver.From);
            shownHover.Add(CurrentManeuver.To);
        }
        foreach (TerritoryManager territoryToSelect in shownSelection) {
            territoryToSelect.Selected = true;
        }
        foreach (TerritoryManager territoryToHover in shownHover) {
            territoryToHover.Hovered = true;
        }

        MatchManager.Team turnTeam = matchManager.GetTeamFromTeamId(matchManager.TurnByTeamId);
        string teamHexColor = ColorUtility.ToHtmlStringRGB(turnTeam.Color);
        string teamColorPrefix = "<color=#" + teamHexColor + ">";
        string teamColorSuffix = "</color>";

        //show remaining undeployed units (or don't if not aplicable)
        DeployableUnitsLeftGUI.SetActive(matchManager.currentPhase == MatchManager.Phase.Deploy);
        if (matchManager.currentPhase == MatchManager.Phase.Deploy) {
            DeployableUnitsLeft_ContentText.text = teamColorPrefix + matchManager.GetTeamUndeployedUnits(matchManager.GetCurrentTurnTeam()).ToString() + teamColorSuffix;
        }
        //visualize which team is currently taking it's turn.
        if (TeamTurnText) {
            TeamTurnText.text = defaultTeamTurnText.Replace("TEAM_NAME", teamColorPrefix + turnTeam.Name + teamColorSuffix);
        }
        //visualize the current phase in the turn.
        if (PhaseText) {
            PhaseText.text = defaultPhaseText.Replace("PHASE_NAME", teamColorPrefix + matchManager.currentPhase.ToString() + teamColorSuffix );
        }
        //change color of the next button
        if (nextButton) {
            nextButton.GetComponent<Image>().color = turnTeam.Color;
        }

        //show or hide the 'next' button depending on if the team currently taking it's turn is controlled by the player.
        if (matchManager.TurnByTeamId == matchManager.GetTeamId(CurrentlyControlledTeam) ) {
            nextButton.gameObject.SetActive(true);
        } else { nextButton.gameObject.SetActive(false); }

    }
}
