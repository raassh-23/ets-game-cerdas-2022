using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [SerializeField]
    private GameObject _spawnOptionPanelContainer;

    [SerializeField]
    private GameObject _spawnCancelButton;

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

    public SpawnOptionController SetSpawnOptions(SpawnOptionConfig spawnOption)
    {
        GameObject spawnOptionButton = Instantiate(_spawnOptionPrefab, _spawnOptionContainer, false);
        SpawnOptionController spawnOptionController = spawnOptionButton.GetComponent<SpawnOptionController>();
        spawnOptionController.SetConfig(spawnOption);
        return spawnOptionController;
    }

    public void SetSpawnUI(bool spawning)
    {
        _spawnOptionPanelContainer.SetActive(!spawning);
        _spawnCancelButton.SetActive(spawning);
    }
}
