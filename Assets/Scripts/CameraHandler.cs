using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour {

    //references
    public PlayerHandler localPlayer;
    MatchManager matchManager;
    GameObject map;

    //dynamic
    public Vector3 targetPosition;
    float horizontalAspect = -1;
    float targetFOV = 60;
    float hFOV = 60;

    //values
    [Tooltip("measured it percent of the distance between targetPosition and currentPosition per second. 1 = 100%, 0 = 0%. Can go greater than 100% per second.")]
    public float lerpSpeed = 7.5f;

    void Start() {
        localPlayer = GetComponentInParent<PlayerHandler>();
        targetFOV = GetComponent<Camera>().fieldOfView;
        hFOV = targetFOV * 1.5f;
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        map = matchManager.map;

        //for some reason this value is set to it's default when the game starts. I think it's because it's tagged as public but hideInInspector.
        //The inspector still sets a default value to it even if it's not visible.
        //Putting this value change in the PlayerHandler's start function doesn't fix the problem.
        //localPlayer.CurrentManeuver = null;
        //localPlayer.testMan = null;
    }
    void Update() {
        MatchManager.Team currentTurnTeam = matchManager.GetTeamFromTeamId(matchManager.TurnByTeamId);
        List<TerritoryManager> teamOccupiedTerritories = matchManager.GetTeamOccupiedTerritories(currentTurnTeam);
        List<TerritoryManager> territoriesToView = new List<TerritoryManager>();
        Vector3 calculatedPosition = Vector3.zero;
        Vector3 farthest_Left = Vector3.zero;
        Vector3 farthest_Right = Vector3.zero;
        Vector3 farthest_Up = Vector3.zero;
        Vector3 farthest_Down = Vector3.zero;

        //print("testMan: " + localPlayer.testMan);
        //print(localPlayer.CurrentManeuver == null);
        //print(localPlayer.CurrentManeuver);
        if (localPlayer.CurrentManeuver != null) {
            territoriesToView.Add(localPlayer.CurrentManeuver.From);
            territoriesToView.Add(localPlayer.CurrentManeuver.To);
        } else if (matchManager.currentPhase == MatchManager.Phase.Deploy || matchManager.currentPhase == MatchManager.Phase.Maneuver) {
            if (matchManager.GetTeamOccupiedTerritoryCount(currentTurnTeam) > 0) {
                territoriesToView = teamOccupiedTerritories;
            } else {
                territoriesToView = matchManager.AllTerritories;
            }
        } else if (matchManager.currentPhase == MatchManager.Phase.Attack) {
            territoriesToView = new List<TerritoryManager>();
            foreach (TerritoryManager territory in teamOccupiedTerritories) {
                territoriesToView.Add(territory);
                foreach (GameObject connectedTerritory in territory.ConnectedTerritories) {
                    TerritoryManager connectedMngr = connectedTerritory.GetComponent<TerritoryManager>();
                    territoriesToView.Add(connectedMngr);
                }
            }
        }
        if (horizontalAspect == -1) {
            territoriesToView = matchManager.AllTerritories;
        }

        foreach (TerritoryManager territory in territoriesToView) {
            Vector3 territoryPosition = territory.gameObject.transform.position;
            Vector3 territoryExtents = territory.GetComponent<Renderer>().bounds.size;
            //calculatedPosition += territoryPosition;
            if (territoryPosition.z + territoryExtents.z > farthest_Up.z) {
                farthest_Up = territoryPosition + territoryExtents;
            } if (territoryPosition.z - territoryExtents.z < farthest_Down.z) {
                farthest_Down = territoryPosition - territoryExtents;
            } if (territoryPosition.x + territoryExtents.x > farthest_Right.x) {
                farthest_Right = territoryPosition + territoryExtents;
            } if (territoryPosition.x - territoryExtents.x < farthest_Left.x) {
                farthest_Left = territoryPosition - territoryExtents;
            }
        }
        //calculatedPosition /= teamOccupiedTerritories.Count;

        Debug.DrawLine(farthest_Left, farthest_Right, Color.blue);
        Debug.DrawLine(farthest_Up, farthest_Down, Color.blue);
        Debug.DrawLine(new Vector3(farthest_Left.x, farthest_Left.y, farthest_Up.z), new Vector3(farthest_Left.x, farthest_Left.y, farthest_Down.z), Color.cyan);
        Debug.DrawLine(new Vector3(farthest_Right.x, farthest_Right.y, farthest_Up.z), new Vector3(farthest_Right.x, farthest_Right.y, farthest_Down.z), Color.cyan);

        Camera cam = GetComponent<Camera>();
        float ratio = 1; //(float)(cam.pixelWidth / cam.pixelHeight);
        float inverseRatio = 1; // (float)cam.pixelHeight / cam.pixelWidth;

        float selectionRatio = Mathf.Abs(farthest_Right.x - farthest_Left.x) / Mathf.Abs(farthest_Down.z - farthest_Up.z);
        if (horizontalAspect == -1) {
            horizontalAspect = selectionRatio;
        }
        hFOV = targetFOV * horizontalAspect; //1.77777777778f;

        calculatedPosition = (farthest_Down + farthest_Up + farthest_Right + farthest_Left) / 4;
        calculatedPosition = new Vector3( (farthest_Right.x + farthest_Left.x)/2, calculatedPosition.y, (farthest_Down.z + farthest_Up.z)/2 );
        float distance = new Vector2(Mathf.Abs(farthest_Down.z - farthest_Up.z) * (ratio), Mathf.Abs(farthest_Right.x - farthest_Left.x) * inverseRatio).magnitude / 2;
        //float distance = Mathf.Max(Mathf.Abs(farthest_Down.z - farthest_Up.z) * (ratio), Mathf.Abs(farthest_Right.x - farthest_Left.x) * inverseRatio) / 2;

        Debug.DrawLine(transform.position, calculatedPosition, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + (transform.forward * (distance) ), Color.red);

        targetPosition = calculatedPosition + (transform.forward * -(distance) + (transform.up * -(distance * .075f)) );

    }
    void LateUpdate() {
        transform.position = Vector3.LerpUnclamped(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }

    void FixedUpdate() {
        Camera camera = GetComponent<Camera>();
        float inverseRatio = (float)camera.pixelHeight / camera.pixelWidth;

        if (hFOV * inverseRatio > targetFOV) {//camera.aspect < 1) {
            float hFOVrad = hFOV * Mathf.Deg2Rad;
            float camH = Mathf.Tan(hFOVrad * .5f) / camera.aspect;
            float vFOVrad = Mathf.Atan(camH) * 2;
            camera.fieldOfView = vFOVrad * Mathf.Rad2Deg;
        } else {
            camera.fieldOfView = targetFOV;
        }
    }
}
