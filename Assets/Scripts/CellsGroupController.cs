using System.Collections.Generic;
using UnityEngine;

public class CellsGroupController : MonoBehaviour
{
    [SerializeField]
    private float _points = 50f;

    public float Points
    {
        get { return _points; }
    }

    [SerializeField]
    private float _addPoints = 5f;

    public List<CellController> Cells { get; private set; }

    [SerializeField]
    private GameObject[] _troopsPrefab;

    public GameObject[] TroopsPrefab
    {
        get { return _troopsPrefab; }
    }

    private Dictionary<string, List<TroopController>> _troopsPool;

    public List<TroopController> Troops { get; private set; }

    [SerializeField]
    private int _spawnCellRandomly = 3;

    [SerializeField]
    private GameObject _cellPrefab;

    private BoxCollider2D _boxCollider;
    private Bounds _cellBounds;

    private EnvironmentManager _environmentManager;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _cellBounds = _cellPrefab.GetComponent<SpriteRenderer>().bounds;
        _environmentManager = GetComponentInParent<EnvironmentManager>();
        Cells = new List<CellController>();
        Troops = new List<TroopController>();

        _troopsPool = new Dictionary<string, List<TroopController>>();
        foreach (GameObject troopPrefab in _troopsPrefab)
        {
            _troopsPool.Add(troopPrefab.name, new List<TroopController>());
        }

        if (_environmentManager == null || !_environmentManager.isTraining)
        {
            SpawnCellRandomly();
            GetAllChildCells();
        }
    }

    private void FixedUpdate()
    {
        _points += _addPoints * Time.fixedDeltaTime;
    }

    public void AddPoints(float points)
    {
        _points += points;
    }

    private void GetAllChildCells()
    {
        foreach (Transform child in transform)
        {
            if (gameObject.CompareTag("GoodSpawn"))
                child.tag = "GoodCell";
            else
                child.tag = "BadCell";


            child.gameObject.layer = LayerMask.NameToLayer("Cell");

            CellController cell = child.GetComponent<CellController>();
            if (cell != null)
            {
                Cells.Add(cell);
                cell.OnCellDestroyed += RemoveCell;
            }
        }
    }

    private void RemoveCell(CellController cell)
    {
        Cells.Remove(cell);
    }

    public void SpawnCellRandomly()
    {
        Bounds bounds = _boxCollider.bounds;

        for (int i = 0; i < _spawnCellRandomly; i++)
        {
            Vector3 randomPosition;
            Vector2 size = Vector2.Scale(_cellBounds.size, _cellPrefab.transform.localScale);

            do
            {
                randomPosition = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    0
                );
            } while (Physics2D.OverlapBox(randomPosition, size, 0) != null);


            Instantiate(_cellPrefab, randomPosition, Quaternion.identity, transform);
        }
    }

    private TroopController GetFromPool(int troopIndex)
    {
        TroopController troop;
        string name = _troopsPrefab[troopIndex].name;

        if (_troopsPool[name].Count == 0)
        {
            GameObject newTroop = Instantiate(_troopsPrefab[troopIndex]);
            newTroop.name = newTroop.name.Replace("(Clone)", "");
            newTroop.layer = LayerMask.NameToLayer("Troop");

            if (gameObject.CompareTag("GoodSpawn"))
                newTroop.tag = "GoodTroop";
            else
                newTroop.tag = "BadTroop";

            troop = newTroop.GetComponent<TroopController>();
            troop.OnTroopDeath += ReturnToPool;
        }
        else
        {
            troop = _troopsPool[name][0];
            _troopsPool[name].RemoveAt(0);
        }

        troop.gameObject.SetActive(true);
        Troops.Add(troop);

        return troop;
    }

    private void ReturnToPool(TroopController troop)
    {
        troop.gameObject.SetActive(false);
        Troops.Remove(troop);
        _troopsPool[troop.name].Add(troop);
    }

    public TroopController SpawnTroopFromCell(int troopIndex, CellController cell)
    {
        TroopController troop = GetFromPool(troopIndex);
        troop.transform.position = cell.transform.GetChild(0).position;

        if (_environmentManager != null)
        {
            troop.transform.parent = _environmentManager.transform;
        }

        troop.InitTroop();

        return troop;
    }

    public TroopController SpawnTroopRandomly()
    {
        int troopIndex = Random.Range(0, _troopsPrefab.Length);

        int cellIndex = Random.Range(0, Cells.Count);

        return SpawnTroopFromCell(troopIndex, Cells[cellIndex]);
    }

    public void RespawnCell()
    {
        while (Cells.Count > 0)
        {
            if (Cells[0] != null)
                DestroyImmediate(Cells[0].gameObject);

            Cells.RemoveAt(0);
        }

        Debug.Log("Respawning cell " + gameObject.name);
        Debug.Log("Before Cells count: " + Cells.Count);

        SpawnCellRandomly();
        GetAllChildCells();

        Debug.Log("After Cells count: " + Cells.Count);
    }

    public void RespawnTroops(int count)
    {
        while (Troops.Count > 0)
        {
            ReturnToPool(Troops[0]);
        }

        for (int i = 0; i < count; i++)
        {
            SpawnTroopRandomly();
        }
    }

    public void ReturnAllTroopsToPool()
    {
        foreach (TroopController troop in Troops)
        {
            ReturnToPool(troop);
        }
    }
}
