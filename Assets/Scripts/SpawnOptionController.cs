using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpawnOptionController : MonoBehaviour
{
    [SerializeField]
    private Text _priceText;

    [SerializeField]
    private Image _spawnImage;

    [SerializeField]
    private Button _spawnButton;

    [SerializeField]
    private Image _spawnBG;

    private SpawnOptionConfig _config;

    public UnityAction<SpawnOptionConfig> OnClicked = delegate { };

    public void SetConfig(SpawnOptionConfig spawnOptionConfig) {
        _config = spawnOptionConfig;
        _priceText.text = _config.price.ToString("0");
        _spawnImage.sprite = _config.prefab.GetComponent<SpriteRenderer>().sprite;
        _spawnButton.onClick.AddListener(delegate { OnClicked(_config); });
        Color bgColor = _config.bgColor;
        bgColor.a = 1f;
        _spawnBG.color = bgColor;
    }

    public void SetSpawnable(bool isSpawnable) {
        Color color = Color.black;
        color.a = isSpawnable ? 0f : 0.5f;
        _spawnButton.image.color = color;
        _priceText.color = isSpawnable ? Color.black : Color.white;
    }

    public float GetPrice() {
        return _config.price;
    }
}
