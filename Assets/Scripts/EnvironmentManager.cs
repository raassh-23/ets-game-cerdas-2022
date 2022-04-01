using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.MLAgents;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{

    [SerializeField]
    private CellsGroupController[] _cellsGroups = new CellsGroupController[2];

    [SerializeField]
    private int _troopSpawnCount = 9;

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
    private int _resetTimer;

    private float _cellDestroyedReward;
    private float _troopDestroyedReward;

    public bool isOneSideDestroyed = false;

    private SimpleMultiAgentGroup _group1;
    private SimpleMultiAgentGroup _group2;

    void Start()
    {
        _group1 = new SimpleMultiAgentGroup();
        _group2 = new SimpleMultiAgentGroup();

        InitScene();
    }

    private void FixedUpdate()
    {
        _resetTimer++;
        if (_resetTimer >= MaxEnvironmentSteps)
        {
            EndEpisode(true);
        }

        CheckTroop();
        CheckCell();
    }

    public void InitScene()
    {
        Debug.Log("InitScene");
        _resetTimer = 0;
        RespawnObjects();
        RegisterAgent();
        _cellDestroyedReward = 1f / _cellsGroups[0].Cells.Count;
        _troopDestroyedReward = 0.3f / _cellsGroups[0].Troops.Count;
        isOneSideDestroyed = false;
    }

    public void RespawnObjects()
    {
        foreach (var cellsGroup in _cellsGroups)
        {
            cellsGroup.RespawnCell();
            cellsGroup.RespawnTroops(_troopSpawnCount);

            foreach (var cell in cellsGroup.Cells)
            {
                cell.OnCellDestroyed += CellDestroyed;
            }

            foreach (var troop in cellsGroup.Troops)
            {
                troop.OnTroopDeath += TroopDestroyed;
            }
        }
    }

    public void RegisterAgent()
    {
        Debug.Log("register agent");

        foreach (var troop in _cellsGroups[0].Troops)
        {
            if (troop != null)
            {
                _group1.RegisterAgent(troop);
            }
        }

        foreach (var troop in _cellsGroups[1].Troops)
        {
            if (troop != null)
            {
                _group2.RegisterAgent(troop);
            }
        }
    }

    public void UnregisterAgent()
    {
        Debug.Log("unregister agent");

        foreach (var troop in _cellsGroups[0].Troops)
        {
            if (troop != null)
            {
                _group1.UnregisterAgent(troop);
            }
        }

        foreach (var troop in _cellsGroups[1].Troops)
        {
            if (troop != null)
            {
                _group2.UnregisterAgent(troop);
            }
        }
    }

    private void CellDestroyed(CellController cell)
    {
        if (cell.gameObject.CompareTag("GoodCell"))
        {
            _group2.AddGroupReward(_cellDestroyedReward);
            Debug.Log("GoodCell Destroyed");
        }
        else if (cell.gameObject.CompareTag("BadCell"))
        {
            _group1.AddGroupReward(_cellDestroyedReward);
            Debug.Log("BadCell Destroyed");
        }
    }

    private void TroopDestroyed(TroopController troop)
    {
        if (troop.gameObject.CompareTag("GoodTroop"))
        {
            _group2.AddGroupReward(_troopDestroyedReward);
            Debug.Log("GoodTroop Destroyed");
        }
        else if (troop.gameObject.CompareTag("BadTroop"))
        {
            _group1.AddGroupReward(_troopDestroyedReward);
            Debug.Log("BadTroop Destroyed");
        }
    }

    private void CheckTroop()
    {
        if (_cellsGroups[0].Troops.Count == 0 && _cellsGroups[1].Troops.Count == 0)
        {
            EndEpisode(true);
        }
        else if (_cellsGroups[0].Troops.Count == 0 || _cellsGroups[1].Troops.Count == 0)
        {
            isOneSideDestroyed = true;
        }
    }

    private void CheckCell()
    {
        if (_cellsGroups[0].Cells.Count == 0 || _cellsGroups[1].Cells.Count == 0)
        {
            Debug.Log("GoodCell or BadCell all destroyed");
            EndEpisode(false);
        }
    }

    private void EndEpisode(bool timeout)
    {
        if (_cellsGroups[0].Cells.Count > _cellsGroups[1].Cells.Count)
        {
            _group1.AddGroupReward(1f);
            _group2.AddGroupReward(-1f);
            Debug.Log("Good wins");
        }
        else if (_cellsGroups[1].Cells.Count > _cellsGroups[0].Cells.Count)
        {
            _group1.AddGroupReward(-1f);
            _group2.AddGroupReward(1f);
            Debug.Log("Bad wins");
        }
        else
        {
            _group1.AddGroupReward(0f);
            _group2.AddGroupReward(0f);
            Debug.Log("Draws");
        }

        UnregisterAgent();

        if (timeout) {
            _group1.GroupEpisodeInterrupted();
            _group2.GroupEpisodeInterrupted();
        } else {
            _group1.EndGroupEpisode();
            _group2.EndGroupEpisode();
        }

        InitScene();
    }
}
