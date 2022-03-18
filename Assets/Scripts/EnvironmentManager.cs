using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.MLAgents;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{

    [SerializeField]
    private CellsGroupController[] _cellsGroups = new CellsGroupController[2];

    private List<CellController>[] _cells;
    private List<TroopController>[] _troops;

    [SerializeField]
    private int TroopSpawnCount = 9;

    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
    private int _resetTimer;

    private SimpleMultiAgentGroup _group1;
    private SimpleMultiAgentGroup _group2;

    void Start()
    {
        _cells = new List<CellController>[_cellsGroups.Length];
        _troops = new List<TroopController>[_cellsGroups.Length];
        _group1 = new SimpleMultiAgentGroup();
        _group2 = new SimpleMultiAgentGroup();

        InitScene();
    }

    private void FixedUpdate()
    {
        CheckCell();
        
        _resetTimer++;
        if (_resetTimer >= MaxEnvironmentSteps)
        {
            EndEpisode();
        }
    }

    public void InitScene()
    {
        _resetTimer = 0;
        RespawnObjects();
        Debug.Log("InitScene");
    }

    public void RespawnObjects()
    {
        for (int i = 0; i < _cellsGroups.Length; i++)
        {
            CellsGroupController cellsGroup = _cellsGroups[i];

            cellsGroup.RespawnCell();
            cellsGroup.RespawnTroops(TroopSpawnCount);
        }
    }

    public void RegisterAgent()
    {
        foreach (var troop in _troops[0])
        {
            if (troop != null) {
                _group1.RegisterAgent(troop);
                troop.OnTroopDeath += UnregisterGroup1;
            }

            
        }

        foreach (var troop in _troops[1])
        {
            if (troop != null)
                _group2.RegisterAgent(troop);
                troop.OnTroopDeath += UnregisterGroup2;
        }
    }

    private void UnregisterGroup1(TroopController troop)
    {
        _group1.UnregisterAgent(troop);
    }

    private void UnregisterGroup2(TroopController troop)
    {
        _group2.UnregisterAgent(troop);
    }

    private void CheckCell()
    {
        if (_cellsGroups[0].Cells.Count == 0)
        {
            _group1.AddGroupReward(-1);
            _group2.AddGroupReward(1);
            EndEpisode();
        }
        else if (_cellsGroups[1].Cells.Count == 0)
        {
            _group1.AddGroupReward(1);
            _group2.AddGroupReward(-1);
            EndEpisode();
        }
    }

    private void EndEpisode()
    {
        _group1.GroupEpisodeInterrupted();
        _group2.GroupEpisodeInterrupted();
        InitScene();
    }
}
