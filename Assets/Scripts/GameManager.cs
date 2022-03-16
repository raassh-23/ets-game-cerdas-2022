using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                {
                    Debug.LogError("Error: GameManager not Found");
                }
            }

            return _instance;
        }
    }

    [SerializeField]
    private CellsGroupController[] _cellsGroups;

    private Dictionary<string, List<TroopController>> _spawnedTroops;

    private void Start()
    {
        _spawnedTroops = new Dictionary<string, List<TroopController>>();

        foreach (CellsGroupController cellsGroup in _cellsGroups)
        {
            _spawnedTroops.Add(cellsGroup.gameObject.tag, new List<TroopController>());
        }
    }

    public void addToSpawnedTroops(TroopController troop)
    {
        _spawnedTroops[troop.gameObject.tag].Add(troop);
    }

    public void removeFromSpawnedTroops(TroopController troop)
    {
        _spawnedTroops[troop.gameObject.tag].Remove(troop);
    }
}
