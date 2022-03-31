using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _spawnOptionPanel;

    [SerializeField]
    private Text _pointsText;

    [SerializeField]
    private Transform _spawnOptionContainer;

    [SerializeField]
    private GameObject _spawnOptionPrefab;

    public void ToggleSpawnOptionPanel()
    {
        Text buttonText = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>();

        if(_spawnOptionPanel.activeSelf)
        {
            _spawnOptionPanel.SetActive(false);
            buttonText.text = "Show";
        }
        else
        {
            _spawnOptionPanel.SetActive(true);
            buttonText.text = "Hide";
        }
    }

    public void SetPointsText(float points) {
        _pointsText.text = "Points:\n" + points.ToString("0");
    }

    public void SetSpawnOptions(SpawnOptionConfig[] spawnOptions)
    {
        foreach(SpawnOptionConfig spawnOption in spawnOptions)
        {
            GameObject spawnOptionButton = Instantiate(_spawnOptionPrefab, _spawnOptionContainer, false);
            spawnOptionButton.GetComponent<SpawnOptionController>().SetConfig(spawnOption);
        }
    }
}
