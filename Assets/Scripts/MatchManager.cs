using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour {

    [System.Serializable]
    public class Team {
        public string Name;
        public Color Color;
        public PlayerHandler Owner;
    }
    [System.Serializable]
    public class TeamTurnData {
        public int UndeployedUnits = 0;

    }
    public class CombatInstance {
        public Team teamInCombatA;
        public Team teamInCombatB;
        public List<int> teamInCombatResultA = null;
        public List<int> teamInCombatResultB = null;
        public TerritoryManager territoryInCombatA;
        public TerritoryManager territoryInCombatB;
        [Tooltip("If this value is true, combat has finished and CurrentCombatInstance isn't null so the user can maneuver troops to the new territory and will be changed after they're finished.")]
        public bool Maneuvering = false;
    }
    public enum Phase { Deploy, Attack, Maneuver };

    public GameObject map;
    public List<TerritoryManager> AllTerritories = new List<TerritoryManager>();
    [Tooltip("The order of teams in this list determines their turn order and TeamId.")]

    public List<Team> teams = new List<Team>();
    Dictionary<Team, TeamTurnData> teamsTurnData = new Dictionary<Team, TeamTurnData>();

    public Phase currentPhase;
    public bool initialRound = true;
    public bool waitingForTeamsCount = true;
    [Tooltip("TeamId = Team's position in the 'teams' list.")]
    public int TurnByTeamId = 0;
    CombatInstance CurrentCombatInstance = null;

    //Information functions
    public Team GetCurrentTurnTeam() {
        return GetTeamFromTeamId(TurnByTeamId);
    }
    public int GetTeamId(Team team) {
        int i = 0;
        Team foundTeam = null;
        int foundTeamId = -1;
        foreach (Team teamToCheck in teams) {
            if (teamToCheck.Name == team.Name) {
                foundTeam = teamToCheck;
                foundTeamId = i;
            }
            i++;
        }
        if (foundTeam != null) {
            return foundTeamId;
        } else {
            Debug.LogError("Unable to find team '" + team.Name + "' in list of teams.");
            return -1;
        }
    }
    public bool GetIsTeamsTurn(Team team) {
        return TurnByTeamId == GetTeamId(team);
    }
    public List<TerritoryManager> GetTeamOccupiedTerritories(Team team) {
        List<TerritoryManager> result = new List<TerritoryManager>();
        foreach (TerritoryManager territory in AllTerritories) {
            //print(territory.Occupant.Name + " " + team.Name + " " + (territory.Occupant.Name == team.Name) );
            if ((team == null && (territory.Occupant == null || territory.Occupant.Name == "")) || (team != null && territory.Occupant != null && territory.Occupant.Name == team.Name) ) {
                result.Add(territory);
            }
        }
        return result;
    }
    public int GetTeamOccupiedTerritoryCount(Team team) {
        return GetTeamOccupiedTerritories(team).Count;
    }
    public int GetTeamMaxDeployableUnits(Team team) {
        if (initialRound) { //GetTeamOccupiedTerritoryCount(team) > 0) {
            return (8 - 3); //3 is how many units they will be given at the start of the next round.
        } else {
            return (int)Mathf.Max(3, Mathf.Floor(GetTeamOccupiedTerritoryCount(team) / 3));
        }
    }
    public int GetTeamUndeployedUnits(Team team) {
        return teamsTurnData[team].UndeployedUnits;
    }
    public Team GetTeamFromTeamId(int teamId) {
        Team foundTeam = teams[teamId];
        if (foundTeam != null) {
            return foundTeam;
        } else {
            Debug.LogError("Unable to find team from TeamId '" + teamId + "'.");
            return null;
        }
    }

    //Back-end functions
    void EndGame(Team winner) {
        CurrentCombatInstance = null;

        List<PlayerHandler> players = new List<PlayerHandler>();
        foreach (Team team in teams) {
            if (team.Owner != null && !players.Contains(team.Owner)) {
                players.Add(team.Owner);
            }
        }
        foreach (PlayerHandler player in players) {
            player.EndGame(winner);
        }

        StartCoroutine("reload");
        //Delay.Create(reload, 10);

    }
    IEnumerator reload() {
        yield return new WaitForSeconds(10);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void ChangeUnitCount(TerritoryManager target, int unitCountModifier, bool silent = false) {
        target.OccupantCount = Mathf.Max(target.OccupantCount + (unitCountModifier), 0);
        if (silent) {
            target.lastOccupantCount = Mathf.Max(target.lastOccupantCount + (unitCountModifier), 0);
        }
        if (target.OccupantCount <= 0) {
            target.Occupant = null;
        }
    }
    void StartTeamTurn(Team team) {
        TeamTurnData turnData = teamsTurnData[team];
        if (turnData != null) {
            turnData.UndeployedUnits = GetTeamMaxDeployableUnits(team);     //Decide how many units to give the player;
        } else {
            Debug.LogError("Team '" + team.Name + "' does not have an associated TeamTurnData.");
        }
    }
    void StartAllTeamsTurns() {
        foreach (Team team in teams) {
            StartTeamTurn(team);
        }
    }

    //Player Actions
    public void PlayerAction_SetTeamCount(int numOfTeams) {
        numOfTeams = Mathf.Clamp(numOfTeams, 2, 6);
        if (initialRound && TurnByTeamId == 0) {
            waitingForTeamsCount = false;
            while (teams.Count > numOfTeams) teams.RemoveAt(teams.Count - 1);
            //teams.Capacity = (int)numOfTeams;
        }
    }
    public void PlayerAction_Deploy(PlayerHandler playerPerformingAction, TerritoryManager territory, int unitCount) {
        if (waitingForTeamsCount) { return; }
        Team teamPerformingAction = GetTeamFromTeamId(TurnByTeamId);
        if (GetTeamFromTeamId(TurnByTeamId).Owner == playerPerformingAction) { } else { return; }
        if (currentPhase == Phase.Deploy) { } else { return; }

        int occupiedTerritories = GetTeamOccupiedTerritoryCount(teamPerformingAction);

        if ((occupiedTerritories > 0 && territory.Occupant != null && territory.Occupant.Name == teamPerformingAction.Name) || (occupiedTerritories <= 0 && (territory.Occupant == null || territory.Occupant.Name == "") ) ) {
            unitCount = Mathf.Clamp(unitCount, 0, GetTeamUndeployedUnits(teamPerformingAction));
            teamsTurnData[teamPerformingAction].UndeployedUnits -= unitCount;
            territory.Occupant = teamPerformingAction;
            territory.OccupantCount += unitCount;
        }
    }
    public void PlayerAction_Attack(PlayerHandler playerPerformingAction, TerritoryManager from, TerritoryManager to) {
        Team teamPerformingAction = GetTeamFromTeamId(TurnByTeamId);
        if (GetTeamFromTeamId(TurnByTeamId).Owner == playerPerformingAction) { } else { return; }
        if (currentPhase == Phase.Attack) { } else {
            Debug.LogWarning("Team '" + teamPerformingAction.Name + "' tried to perform an action at the wrong time. "
            + GetTeamId(teamPerformingAction) + ", " + TurnByTeamId + ", "
            + "Attack, " + currentPhase);
            return;
        }

        if (from.ConnectedTerritories.Contains(to.gameObject)) {
            if (from.Occupant.Name == teamPerformingAction.Name && (to.Occupant == null || to.Occupant.Name != teamPerformingAction.Name)) {
                if (from.OccupantCount > 1) {

                    if (CurrentCombatInstance == null) {

                        CurrentCombatInstance = new CombatInstance();
                        CurrentCombatInstance.teamInCombatResultA = null;
                        CurrentCombatInstance.teamInCombatResultB = null;
                        CurrentCombatInstance.teamInCombatA = from.Occupant;
                        CurrentCombatInstance.teamInCombatB = to.Occupant;
                        CurrentCombatInstance.territoryInCombatA = from;
                        CurrentCombatInstance.territoryInCombatB = to;

                        if (to.Occupant == null || to.Occupant.Name == "") {
                            to.Occupant = teamPerformingAction;
                            to.OccupantCount = 1;
                            ChangeUnitCount(from, -1);
                            CurrentCombatInstance.Maneuvering = true;

                            if (from.OccupantCount > 1) {
                                from.Occupant.Owner.OpenManeuver(new PlayerHandler.ManeuverData(from, to));
                            } else {
                                CurrentCombatInstance = null;
                            }
                        } else {
                            playerPerformingAction.RollDice(teamPerformingAction, Mathf.Min(3, from.OccupantCount - 1));
                        }
                    }

                }
            }
        }
    }
    public void PlayerAction_Maneuver(PlayerHandler playerPerformingAction, TerritoryManager from, TerritoryManager to, int unitCount) {
        Team teamPerformingAction = GetTeamFromTeamId(TurnByTeamId);
        if (GetTeamFromTeamId(TurnByTeamId).Owner == playerPerformingAction) { } else { return; }
        if (currentPhase == Phase.Maneuver || currentPhase == Phase.Attack) { } else { return; }

        if (from.Occupant.Name == teamPerformingAction.Name && to.Occupant.Name == teamPerformingAction.Name && from != to) {
            if (currentPhase != Phase.Attack || (from == CurrentCombatInstance.territoryInCombatA && to == CurrentCombatInstance.territoryInCombatB)) {
                int WithdrawlAmmount = Mathf.Min(unitCount, (from.OccupantCount - 1));
                to.OccupantCount += WithdrawlAmmount;
                from.OccupantCount -= WithdrawlAmmount;
            }
        }
    }
    public void PlayerAction_ReportFinishedCombatManeuver(PlayerHandler playerPerformingAction) {
        if (GetTeamFromTeamId(TurnByTeamId).Owner == playerPerformingAction) { } else { return; }

        if (CurrentCombatInstance != null && CurrentCombatInstance.Maneuvering) {
            CurrentCombatInstance = null;
        }
    }
    public void PlayerAction_ProgressToNextPhase(PlayerHandler playerPerformingAction) {
        Team teamPerformingAction = GetTeamFromTeamId(TurnByTeamId);
        if (GetTeamFromTeamId(TurnByTeamId).Owner == playerPerformingAction) { } else { return; }

        CurrentCombatInstance = null;

        void nextTeamsTurn() {
            TurnByTeamId += 1;
            if (TurnByTeamId > teams.Count - 1) {
                TurnByTeamId = 0;
                initialRound = false;
                StartAllTeamsTurns();
            }
            currentPhase = Phase.Deploy;
        }

        if (initialRound) {
            nextTeamsTurn();
        } else if (currentPhase == Phase.Deploy) {
            currentPhase = Phase.Attack;
        } else if (currentPhase == Phase.Attack) {
            currentPhase = Phase.Maneuver;
        } else if (currentPhase == Phase.Maneuver) {
            nextTeamsTurn();
        }
    }
    public void PlayerAction_ReportDiceRollResult(PlayerHandler playerPerformingAction, Team teamPerformingAction, List<int> result) {
        print("Reveived reported dice roll result. " + result + " from " + teamPerformingAction.Name );
        //Team teamPerformingAction = null;
        if (teamPerformingAction == null) {
            Debug.LogError("PlayerAction_ReportDiceRollResult: Paramiter 'teamPerformingAction' is required. Got '" + teamPerformingAction + "'.");
            return;
        }
        if (teamPerformingAction.Owner == playerPerformingAction) { } else {
            Debug.LogWarning("Player '" + playerPerformingAction.name + "' tried to perform an action as team '" + teamPerformingAction.Name + "', who they do not controll.");
            return;
        }

        if (CurrentCombatInstance != null) {
            if (CurrentCombatInstance.teamInCombatA == teamPerformingAction && CurrentCombatInstance.teamInCombatResultA == null) {
                if (result.Count > 3 || result.Count > CurrentCombatInstance.territoryInCombatA.OccupantCount - 1) { Debug.LogWarning("Revieved report with too many dice. number of dice: " + result.Count + ", team: " + teamPerformingAction.Name + ", Player: " + playerPerformingAction); return; }
                CurrentCombatInstance.teamInCombatResultA = result;
                CurrentCombatInstance.teamInCombatB.Owner.RollDice(CurrentCombatInstance.teamInCombatB, Mathf.Min(2, CurrentCombatInstance.territoryInCombatB.OccupantCount) );
            } else if (CurrentCombatInstance.teamInCombatB == teamPerformingAction && CurrentCombatInstance.teamInCombatResultB == null) {
                if (result.Count > 2 || result.Count > CurrentCombatInstance.territoryInCombatB.OccupantCount) { Debug.LogWarning("Revieved report with too many dice. number of dice: " + result.Count + ", team: " + teamPerformingAction.Name + ", Player: " + playerPerformingAction); return; }
                CurrentCombatInstance.teamInCombatResultB = result;
            } else {
                Debug.LogWarning("Received reported dice roll result from team not in combat or after already receiving one from said team. Team:" + teamPerformingAction.Name);
            }
        } else {
            Debug.LogWarning("Received reported dice roll result when no teams/territories are in combat.");
        }


    }


    void Start() {
        foreach(TerritoryManager territory in map.GetComponentsInChildren<TerritoryManager>() ) {
            AllTerritories.Add(territory);
        }
        foreach (Team team in teams) {
            TeamTurnData turnData = new TeamTurnData();
            teamsTurnData.Add(team, turnData);
            if (team.Owner != null) {
                team.Owner.ControlledTeams.Add(team);
            }
        }
        StartAllTeamsTurns();
    }
    void PrintContents(string preface, List<int> list) {
        string output = "";
        foreach (int value in list) {
            output += value.ToString() + ", ";
        }
        print(preface + output);
    }
    void PrintContents(List<int> list) {
        string output = "";
        foreach (int value in list) {
            output += value.ToString() + ", ";
        }
        print(output);
    }
    void Update() {
        if (CurrentCombatInstance != null && CurrentCombatInstance.Maneuvering == false && CurrentCombatInstance.teamInCombatResultA != null && CurrentCombatInstance.teamInCombatResultB != null) {

            //sort dice result lists to list their highest first.
            PrintContents("ResultA: ", CurrentCombatInstance.teamInCombatResultA);
            PrintContents("ResultB: ", CurrentCombatInstance.teamInCombatResultB);
            CurrentCombatInstance.teamInCombatResultA.Sort((a, b) => b.CompareTo(a));
            CurrentCombatInstance.teamInCombatResultB.Sort((a, b) => b.CompareTo(a));
            PrintContents("Sorted ResultA: ", CurrentCombatInstance.teamInCombatResultA);
            PrintContents("Sorted ResultB: ", CurrentCombatInstance.teamInCombatResultB);

            //go through each dice roll that both teams have and deal damage acordingly.
            for (int i = 0; i < Mathf.Min(CurrentCombatInstance.teamInCombatResultA.Count, CurrentCombatInstance.teamInCombatResultB.Count); i++) {
                int rollA = CurrentCombatInstance.teamInCombatResultA[i];
                int rollB = CurrentCombatInstance.teamInCombatResultB[i];

                print(rollA + "/" + rollB);
                if (rollA > rollB) {
                    ChangeUnitCount(CurrentCombatInstance.territoryInCombatB, -1);
                } else {
                    ChangeUnitCount(CurrentCombatInstance.territoryInCombatA, -1);
                }
            }
            TerritoryManager from = CurrentCombatInstance.territoryInCombatA;
            TerritoryManager to = CurrentCombatInstance.territoryInCombatB;
            if (to.OccupantCount == 0) {
                to.Occupant = CurrentCombatInstance.teamInCombatA;
                to.ForceUpdateFloatingGainLoss();
                ChangeUnitCount(to, 1, true);
                //to.OccupantCount = 1;
                ChangeUnitCount(from, -1, true);
                if (from.OccupantCount > 1) {
                    CurrentCombatInstance.Maneuvering = true;
                    from.Occupant.Owner.OpenManeuver(new PlayerHandler.ManeuverData(from, to));
                } else {
                    CurrentCombatInstance = null;
                }
            } else {
                CurrentCombatInstance = null;
            }
            
        }

        foreach (Team team in teams) {
            if (GetTeamOccupiedTerritoryCount(team) >= AllTerritories.Count) {
                EndGame(team);
                break;
            }
        }
    }
}
