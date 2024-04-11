using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    [System.Serializable]
    public class Continent {
        public string Name;
        public int UnitBonus;
        public List<TerritoryManager> Territories = new List<TerritoryManager>();

    }
    [System.Serializable]
    public class Connection {
        public GameObject PointA;
        public GameObject PointB;

        public Connection(GameObject ObjectA, GameObject ObjectB) {
            PointA = ObjectA;
            PointB = ObjectB;
        }
    }
    [System.Serializable]
    public class BackupConnection {
        public string PointA;
        public string PointB;

        public BackupConnection(GameObject ObjectA, GameObject ObjectB) {
            PointA = ObjectA.name;
            PointB = ObjectB.name;
        }
        public BackupConnection(MapManager.Connection connection) {
            PointA = connection.PointA.name;
            PointB = connection.PointB.name;
        }
    }

    public List<Connection> Connections = new List<Connection>();
    public List<BackupConnection> BackedupConnections = new List<BackupConnection>();
    public List<Continent> Continents;
    MatchManager matchManager;

    // Start is called before the first frame update
    void Start() {
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();

        foreach (TerritoryManager territory in transform.GetComponentsInChildren<TerritoryManager>()) {
            foreach (MatchManager.Team team in matchManager.teams) {
                if (territory.Occupant != null && territory.Occupant.Name == team.Name) {
                    territory.Occupant = team;
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
