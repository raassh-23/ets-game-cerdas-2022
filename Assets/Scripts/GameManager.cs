using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private CellsGroupController _goodCellsGroup;

    [SerializeField]
    private CellsGroupController _badCellsGroup;

    [SerializeField]
    private UIManager _uiManager;

    [SerializeField]
    private SpawnOptionConfig[] _spawnOptions;

    private void Start() {
        _uiManager.SetSpawnOptions(_spawnOptions);
    }

    private void Update() {
        _uiManager.SetPointsText(_goodCellsGroup.Points);
    }

}

[System.Serializable]
public struct SpawnOptionConfig
{
    public string name;
    public GameObject prefab;
    public float price;
}
