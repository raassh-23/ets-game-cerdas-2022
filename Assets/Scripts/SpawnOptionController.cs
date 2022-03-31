using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnOptionController : MonoBehaviour
{
    [SerializeField]
    private Text _nameText;

    [SerializeField]
    private Text _priceText;

    [SerializeField]
    private Image _spawnImage;

    private SpawnOptionConfig _spawnOptionConfig;

    public void SetConfig(SpawnOptionConfig spawnOptionConfig) {
        _spawnOptionConfig = spawnOptionConfig;
        _nameText.text = _spawnOptionConfig.name;
        _priceText.text = _spawnOptionConfig.price.ToString("0");
        _spawnImage.sprite = _spawnOptionConfig.prefab.GetComponent<SpriteRenderer>().sprite;
    }

}
