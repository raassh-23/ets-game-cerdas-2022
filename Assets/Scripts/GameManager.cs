using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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

    private List<SpawnOptionController> _activeSpawnOptions;

    private SpawnOptionConfig _selectedSpawnOption;

    public static float Score { get; private set; }

    private void Start()
    {
        // work around for lagging when spawning the first troop
        Destroy(GetComponentInChildren<TroopController>().gameObject);

        Score = 0;

        _activeSpawnOptions = new List<SpawnOptionController>();

        foreach (var spawnOption in _spawnOptions)
        {
            SpawnOptionController spawnOptionController = _uiManager.SetSpawnOptions(spawnOption);
            spawnOptionController.OnClicked += SetSelectedTroop;
            _activeSpawnOptions.Add(spawnOptionController);
        }

        Invoke("BadCellSpawnTroop", Random.Range(0f, 5f));
    }

    private void Update()
    {
        _uiManager.SetPointsText(_goodCellsGroup.Points);

        foreach (var spawnOption in _activeSpawnOptions)
        {
            spawnOption.SetSpawnable(_goodCellsGroup.Points >= spawnOption.GetPrice());
        }

        if (_goodCellsGroup.Cells.Count == 0)
        {
            DestroyAllTroops();
            _uiManager.ShowGameOverPanel();
        }

        if (_badCellsGroup.Cells.Count == 0)
        {
            DestroyAllTroops();
            SaveManager.SetUnlockedLevel(SaveManager.CurrentLevel);
            SaveManager.SetLevelHighscore(SaveManager.CurrentLevel, Score);
            _uiManager.SetScoreText(Score);
            _uiManager.ShowGameWonPanel();
        }
    }

    private void SetSelectedTroop(SpawnOptionConfig config)
    {
        if (_goodCellsGroup.Points >= config.price)
        {
            _selectedSpawnOption = config;
            _uiManager.SetSpawnUI(true);

            foreach (var cell in _goodCellsGroup.Cells)
            {
                cell.StartBlinking();
                cell.OnCellClicked += SpawnTroop;
            }
        }
    }

    public void CancelSpawn()
    {
        _uiManager.SetSpawnUI(false);

        foreach (var cell in _goodCellsGroup.Cells)
        {
            cell.StopBlinking();
            cell.OnCellClicked -= SpawnTroop;
        }
    }

    private void SpawnTroop(CellController cell)
    {
        Debug.Log("Spawning troop");
        int troopIndex = -1;

        for (var i = 0; i < _goodCellsGroup.TroopsPrefab.Length; i++)
        {
            if (_goodCellsGroup.TroopsPrefab[i].name == _selectedSpawnOption.prefab.name)
            {
                troopIndex = i;
                break;
            }
        }

        if (troopIndex == -1)
        {
            Debug.LogError("No troop found");
            return;
        }

        _goodCellsGroup.SpawnTroopFromCell(troopIndex, cell);
        _goodCellsGroup.AddPoints(-_selectedSpawnOption.price);
        CancelSpawn();
    }

    private void BadCellSpawnTroop()
    {
        _badCellsGroup.SpawnTroopRandomly();

        Invoke("BadCellSpawnTroop", Random.Range(5f, 15f));
    }

    public static void AddScore(int score)
    {
        Score += score;
    }

    public void DestroyAllTroops()
    {
        _goodCellsGroup.ReturnAllTroopsToPool();
        _badCellsGroup.ReturnAllTroopsToPool();
    }
}

[System.Serializable]
public struct SpawnOptionConfig
{
    public GameObject prefab;
    public float price;
    public Color bgColor;
}
