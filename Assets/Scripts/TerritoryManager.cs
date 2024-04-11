using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryManager : MonoBehaviour {

    //References
    MapManager mapManager;
    MatchManager matchManager;

    //Game
    public bool Selected = false;
    public bool Hovered = false;
    public List<GameObject> ConnectedTerritories = new List<GameObject>();
    public MatchManager.Team Occupant;
    public int OccupantCount = 0;
    //[HideInInspector]
    public int lastOccupantCount = 0;
    [SerializeField] GameObject TerritoryBillboardDataPrefabReference;
    [SerializeField] GameObject TerritoryBillboardData = null;
    [SerializeField] TextMesh UnitCountTextReference = null;
    [SerializeField] TextMesh TerritoryNameTextReference = null;
    [SerializeField] TextMesh UnitGainTextReferencePrefab;
    [SerializeField] TextMesh UnitLossTextReferencePrefab;


    public void ForceUpdateFloatingGainLoss() {
        int difference = OccupantCount - lastOccupantCount;
        if (difference > 0) {
            TextMesh UnitGainTextReference = Instantiate(UnitGainTextReferencePrefab);
            UnitGainTextReference.transform.SetParent(TerritoryBillboardData.transform);
            UnitGainTextReference.transform.localPosition = Vector3.zero;
            UnitGainTextReference.transform.localRotation = Quaternion.Euler(Vector3.zero);
            UnitGainTextReference.gameObject.SetActive(true);

            UnitGainTextReference.text = "  +" + difference.ToString() + "\n";
        } else if (difference < 0) {
            TextMesh UnitLossTextReference = Instantiate(UnitLossTextReferencePrefab);
            UnitLossTextReference.transform.SetParent(TerritoryBillboardData.transform);
            UnitLossTextReference.transform.localPosition = Vector3.zero;
            UnitLossTextReference.transform.localRotation = Quaternion.Euler(Vector3.zero);
            UnitLossTextReferencePrefab.gameObject.SetActive(true);

            UnitLossTextReferencePrefab.text = "" + difference.ToString() + "  \n";
        }

        lastOccupantCount = OccupantCount;
    }

    void Start() {
        //transform.localPosition = transform.localPosition * 1.05f;

        if (TerritoryBillboardData == null) {
            TerritoryBillboardData = Instantiate(TerritoryBillboardDataPrefabReference);
            TerritoryBillboardData.transform.SetParent(transform);
            UnitCountTextReference = TerritoryBillboardData.transform.Find("UnitCountText").GetComponent<TextMesh>();
            TerritoryNameTextReference = TerritoryBillboardData.transform.Find("TerritoryNameText").GetComponent<TextMesh>();
        }
        //UnitGainTextReferencePrefab = TerritoryBillboardData.transform.Find("UnitGainText").GetComponent<TextMesh>();
        //UnitLossTextReferencePrefab = TerritoryBillboardData.transform.Find("UnitLossText").GetComponent<TextMesh>();

        mapManager = GetComponentInParent<MapManager>();
        matchManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();

        foreach (MapManager.Connection connection in mapManager.Connections) {
            if (connection.PointA == gameObject) {
                ConnectedTerritories.Add(connection.PointB);
            } else if (connection.PointB == gameObject) {
                ConnectedTerritories.Add(connection.PointA);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        int selectedInt = 0;
        if (Selected) { selectedInt = 1; }
        int hoveredInt = 0;
        if (Hovered) { hoveredInt = 1; }

        GetComponent<Renderer>().material.SetInt("Boolean_43D02678", selectedInt);
        GetComponent<Renderer>().material.SetInt("Boolean_BCADC87", hoveredInt);
        if (Occupant != null && Occupant.Name != "") {
            GetComponent<MeshRenderer>().material.color = Occupant.Color;
            GetComponent<Renderer>().material.SetColor("Color_3C554333", Occupant.Color);
        } else {
            GetComponent<MeshRenderer>().material.color = Color.gray;
            GetComponent<Renderer>().material.SetColor("Color_3C554333", Color.gray);
        }

        if (UnitCountTextReference != null) {
            if (OccupantCount > 0) {
                UnitCountTextReference.gameObject.SetActive(true);
                UnitCountTextReference.text = OccupantCount.ToString();
            } else {
                UnitCountTextReference.gameObject.SetActive(false);
            }
        }
        if (TerritoryNameTextReference != null) {
            TerritoryNameTextReference.gameObject.SetActive(Hovered);
            TerritoryNameTextReference.text = gameObject.name + "\n";
        }
        if (lastOccupantCount != OccupantCount) {
            ForceUpdateFloatingGainLoss();
        }
    }
}
