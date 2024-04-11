#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;

public class Editor_ConnectionEditor : EditorWindow {

    [MenuItem("Tools/Map Connection Manager")]
    public static void openConnectionManager() {
        EditorWindow.GetWindow(typeof(Editor_ConnectionEditor));
    }

    MatchManager matchManager;

    bool addToContinentFoldout = false;

    void OnGUI() {
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        MapManager mapManager = matchManager.map.GetComponent<MapManager>();

        if (GUILayout.Button("Create Connection")) {
            Debug.Log("creating connection...");
            CreateConnection();
        }
        if (GUILayout.Button("Backup connections")) {
            Debug.Log("backing up connections...");
            BackupConnections();
        }
        if (GUILayout.Button("Restore backed up connections")) {
            Debug.Log("Restoring connections...");
            RestoreBackedupConnections();
        }

        addToContinentFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(addToContinentFoldout, "add to continent...");
        //addToContinentFoldout = EditorGUILayout.Foldout(addToContinentFoldout, "add to continent...");
        if (addToContinentFoldout) {
            foreach (MapManager.Continent continent in mapManager.Continents) {
                if (GUILayout.Button("Add to " + continent.Name)) {
                    Debug.Log("Adding territories to continent '" + continent.Name + "'.");

                    foreach (GameObject selectedGameObject in Selection.gameObjects) {
                        TerritoryManager manager = selectedGameObject.GetComponent<TerritoryManager>();
                        if (manager) {
                            continent.Territories.Add(manager);
                        }
                    }
                }
            }
            EditorGUILayout.Space();
        }
        //EditorGUILayout.EndFoldoutHeaderGroup();

        //myString = EditorGUI.MultiPropertyField("Text Field", backupList);
        if (GUILayout.Button("Randomize Colors")) {
            Debug.Log("recoloring terrain...");
            RandomizeAllColors();
        }
        if (GUILayout.Button("Randomize randomSeed")) {
            Debug.Log("randomizing seed...");
            RandomizeAllRandomSeeds();
        }
        if (GUILayout.Button("Update connection visualization")) {
            Debug.Log("updating visualization...");
            UpdateConnectionVisualization();
        }

        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        //myString = EditorGUILayout.TextField("Text Field", myString);

        //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        //EditorGUILayout.EndToggleGroup();
    }
    void Start() {
        //GUIUtility.systemCopyBuffer = "dicklol";
        //Debug.Log(GUIUtility.systemCopyBuffer);
    }
    void Update() {
        UpdateConnectionVisualization();
    }

    void CreateConnection() {
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        MapManager mapManager = matchManager.map.GetComponent<MapManager>();

        GameObject territoryA = null;
        GameObject territoryB = null;
        foreach (GameObject territory in Selection.gameObjects) {
            TerritoryManager manager = territory.GetComponent<TerritoryManager>();
            if (manager) {
                if (territoryA == null) {
                    territoryA = territory;
                } else if (territoryB == null) {
                    territoryB = territory;
                }
            }
        }
        if (territoryA != null && territoryB != null) {
            mapManager.Connections.Add(new MapManager.Connection(territoryA, territoryB) );
            Debug.Log("Successfully added connection between '" + territoryA.name + "' to '" + territoryB.name + "'.");
        } else {
            Debug.LogWarning("Please select two GameObjects to connect.");
        }
    }
    List<MapManager.BackupConnection> BackupConnections() {
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        MapManager mapManager = matchManager.map.GetComponent<MapManager>();

        List<MapManager.BackupConnection> returnList = new List<MapManager.BackupConnection>();
        mapManager.BackedupConnections = new List<MapManager.BackupConnection>();

        foreach (MapManager.Connection connection in mapManager.Connections) {
            mapManager.BackedupConnections.Add(new MapManager.BackupConnection(connection));
        }

        //mapManager.BackedupConnections = returnList;
        return returnList;
    }
    void RestoreBackedupConnections() {
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        MapManager mapManager = matchManager.map.GetComponent<MapManager>();

        mapManager.Connections = new List<MapManager.Connection>();

        foreach (MapManager.BackupConnection backup in mapManager.BackedupConnections) {
            Transform From = mapManager.transform.Find(backup.PointA);
            Transform To = mapManager.transform.Find(backup.PointB);
            if (From != null && To != null) {
                MapManager.Connection newConneciton = new MapManager.Connection(From.gameObject, To.gameObject);
                mapManager.Connections.Add(newConneciton);
            }
        }
    }
    List<MapManager.Connection> LoadConnections(string loadedData) {
        List<MapManager.Connection> ReturnList = new List<MapManager.Connection>();

        

        return ReturnList;
    }
    Color GetRandomColor() {
        return new Color(((float)Random.Range(0, 255)) / 255f, ((float)Random.Range(0, 255)) / 255f, ((float)Random.Range(0, 255)) / 255f);
    }
    void RandomizeAllColors() {
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        MapManager mapManager = matchManager.map.GetComponent<MapManager>();

        foreach (GameObject territory in Selection.gameObjects) {
            if (territory.GetComponent<TerritoryManager>()) {
                territory.GetComponent<Renderer>().material.SetColor("Color_3C554333", GetRandomColor());
                territory.GetComponent<Renderer>().material.color = GetRandomColor();
            }
        }
    }
    void RandomizeAllRandomSeeds() {
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        MapManager mapManager = matchManager.map.GetComponent<MapManager>();

        foreach (GameObject territory in Selection.gameObjects) {
            if (territory.GetComponent<TerritoryManager>()) {
                territory.GetComponent<Renderer>().material.SetFloat("Vector1_EBAE1FE8", Random.Range(0, 91236790));
                territory.GetComponent<Renderer>().material.SetFloat("Vector1_4264E57F", (float)Random.Range(500, 1000)/1000f);
            }
        }
    }
    void UpdateConnectionVisualization() {
        if (!matchManager) { matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>(); }
        MapManager mapManager = matchManager.map.GetComponent<MapManager>();

        Vector3 offset = new Vector3(0, 1.5f, 0);

        foreach (MapManager.Connection connection in mapManager.Connections) {
            Vector3 pointA = connection.PointA.transform.position + offset;
            Vector3 pointB = connection.PointB.transform.position + offset;
            Debug.DrawLine(pointA, pointB, Color.black);
        }
    }

}
#endif