using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellsGroupController : MonoBehaviour
{
    [SerializeField]
    private float _points = 50f;

    public float Points { 
        get { return _points; }
    }

    [SerializeField]
    private float _addPoints = 5f;

    private List<CellController> _cells;

    [SerializeField]
    private GameObject[] _troopsPrefab;

    private Dictionary<string, List<TroopController>> _troopsPool;

    [SerializeField]
    private int _spawnCellRandomly = 3;

    [SerializeField]
    private GameObject _cellPrefab;

    private BoxCollider2D _boxCollider;
    private Bounds _cellBounds;

    private void Start() {
        _boxCollider = GetComponent<BoxCollider2D>();
        _cellBounds = _cellPrefab.GetComponent<SpriteRenderer>().bounds;

        _troopsPool = new Dictionary<string, List<TroopController>>();
        foreach (GameObject troopPrefab in _troopsPrefab) {
            _troopsPool.Add(troopPrefab.name, new List<TroopController>());
        }

        SpawnCellRandomly();
        GetAllChildCells();

        // placeholder just to test spawning
        // InvokeRepeating("SpawnTroopRandomly", 2f, 2f);
        SpawnTroopRandomly();
    }

    private void GetAllChildCells() {
        _cells = new List<CellController>();
        foreach (Transform child in transform) {
            child.tag = tag;
            child.gameObject.layer = LayerMask.NameToLayer("Cell");

            CellController cell = child.GetComponent<CellController>();
            if (cell != null) {
                _cells.Add(cell);
            }
        }
    }

    private void SpawnCellRandomly() {
        Bounds bounds = _boxCollider.bounds;

        for (int i = 0; i < _spawnCellRandomly; i++)
        {
            Vector3 randomPosition;

            do
            {
                randomPosition = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x), 
                    Random.Range(bounds.min.y, bounds.max.y), 
                    0
                ); 
            } while (Physics2D.OverlapBox(randomPosition, Vector2.Scale(_cellBounds.size, _cellPrefab.transform.localScale), 0) != null);

            
            Instantiate(_cellPrefab, randomPosition, Quaternion.identity, transform);
        }
    }

    private TroopController GetFromPool(int troopIndex) {
        TroopController troop;
        string name = _troopsPrefab[troopIndex].name;
        
        if (_troopsPool[name].Count == 0) {
            GameObject newTroop = Instantiate(_troopsPrefab[troopIndex]);
            newTroop.name = newTroop.name.Replace("(Clone)", "");
            newTroop.tag = tag;
            newTroop.layer = LayerMask.NameToLayer("Troop");

            troop = newTroop.GetComponent<TroopController>();
            troop.OnTroopDeath += ReturnToPool;
        } else {
            troop = _troopsPool[name][0];
            _troopsPool[name].RemoveAt(0);
        }

        GameManager.Instance.addToSpawnedTroops(troop);
        troop.gameObject.SetActive(true);

        return troop;
    }

    private void ReturnToPool(TroopController troop) {
        troop.gameObject.SetActive(false);
        GameManager.Instance.removeFromSpawnedTroops(troop);
        _troopsPool[troop.name].Add(troop);
    }

    private void SpawnTroopFromCell(int troopIndex, CellController cell) {
        TroopController troop = GetFromPool(troopIndex);
        troop.transform.position = cell.transform.GetChild(0).position;
    }

    // placeholder just to test spawning
    private void SpawnTroopRandomly() {
        int troopIndex = Random.Range(0, _troopsPrefab.Length);

        int cellIndex = Random.Range(0, _cells.Count);
        CellController cell = _cells[cellIndex];

        SpawnTroopFromCell(troopIndex, cell);
    }
}
