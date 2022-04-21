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

    private List<SpawnOptionController> _activeSpawnOptions;

    private SpawnOptionConfig _selectedSpawnOption;

    [SerializeField]
    private SpawnOptionConfig[] _enemySpawnOptions;

    private Dictionary<int, bool> _enemySpawnOptionsSpawnable;

    [SerializeField]
    [Range(0, 1)]
    private float _enemySpawnChance = 0.5f;

    public static float Score { get; private set; }

    private bool _isGameOver;

    [SerializeField]
    private float _time;

    private float _countdown;

    private void Start()
    {
        // work around for lagging when spawning the first troop
        Destroy(GetComponentInChildren<TroopController>().gameObject);

        Score = 0;
        _isGameOver = false;
        _countdown = _time;

        _activeSpawnOptions = new List<SpawnOptionController>();

        foreach (var spawnOption in _spawnOptions)
        {
            SpawnOptionController spawnOptionController = _uiManager.SetSpawnOptions(spawnOption);
            spawnOptionController.OnClicked += SetSelectedTroop;
            _activeSpawnOptions.Add(spawnOptionController);
        }

        _enemySpawnOptionsSpawnable = new Dictionary<int, bool>();
        for (var i = 0; i < _enemySpawnOptions.Length; i++)
        {
            _enemySpawnOptionsSpawnable.Add(i, false);
        }

        Invoke("BadCellSpawnTroop", Random.Range(0f, 3f));
    }

    private void Update()
    {
        _uiManager.SetPointsText(_goodCellsGroup.Points);

        foreach (var spawnOption in _activeSpawnOptions)
        {
            spawnOption.SetSpawnable(_goodCellsGroup.Points >= spawnOption.GetPrice());
        }

        for (var i = 0; i < _enemySpawnOptions.Length; i++)
        {
            _enemySpawnOptionsSpawnable[i] = _badCellsGroup.Points >= _enemySpawnOptions[i].price;
        }

        if (!_isGameOver)
        {
            _countdown -= Time.deltaTime;

            if (_countdown <= 0)
            {
                _isGameOver = true;
                _uiManager.ShowGameLostPanel();
            } else {
                _uiManager.SetTimeText(_countdown);
            }

            if (_goodCellsGroup.Cells.Count == 0)
            {
                _isGameOver = true;
                _uiManager.ShowGameLostPanel();
            }

            if (_badCellsGroup.Cells.Count == 0)
            {
                _isGameOver = true;
                SaveManager.SetUnlockedLevel(SaveManager.CurrentLevel);
                SaveManager.SetLevelHighscore(SaveManager.CurrentLevel, Score);
                _uiManager.SetScoreText(Score);
                _uiManager.ShowGameWonPanel();
            }
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
        if (_isGameOver)
        {
            return;
        }

        List<int> spawnableTroopIndex = new List<int>();

        foreach (var item in _enemySpawnOptionsSpawnable)
        {
            if (item.Value) {
                spawnableTroopIndex.Add(item.Key);
            }
        }

        if (spawnableTroopIndex.Count == 0)
        {
            return;
        }

        int troopIndex = spawnableTroopIndex[Random.Range(0, spawnableTroopIndex.Count)];

        if (Random.value < _enemySpawnChance)
        {
            _badCellsGroup.AddPoints(-_enemySpawnOptions[troopIndex].price);
            CellController cell = _badCellsGroup.Cells[Random.Range(0, _badCellsGroup.Cells.Count)];
            _badCellsGroup.SpawnTroopFromCell(troopIndex, cell);
        }

        Invoke("BadCellSpawnTroop", Random.Range(3f, 6f));
    }

    public static void AddScore(int score)
    {
        Score += score;
    }
}

[System.Serializable]
public struct SpawnOptionConfig
{
    public GameObject prefab;
    public float price;
    public Color bgColor;
}
