using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _levels;

    [SerializeField]
    private Sprite _levelUnlocked;

    [SerializeField]
    private Sprite _levelLocked;

    private void Start()
    {
        for (int i = 0; i < _levels.Length; i++)
        {
            if (i <= SaveManager.GetUnlockedLevel())
            {
                int level = i + 1;
                _levels[i].GetComponent<Button>().onClick.AddListener(() => {
                    SaveManager.CurrentLevel = level;
                    SceneManager.LoadScene("Level" + level);
                });
                _levels[i].GetComponent<Image>().sprite = _levelUnlocked;
                _levels[i].GetComponentsInChildren<Text>()[1].text =
                    SaveManager.GetLevelHighscore(level).ToString("0000");
            }
            else
            {
                _levels[i].GetComponent<Button>().interactable = false;
                _levels[i].GetComponent<Image>().sprite = _levelLocked;
                
                Text[] texts = _levels[i].GetComponentsInChildren<Text>();
                texts[0].color = Color.white;
                texts[1].text = "";
            }
        }
    }
}
