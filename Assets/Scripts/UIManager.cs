using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    private GameObject _spawnCancelButton;

    [SerializeField]
    private Button _spawnOptionButton;

    [SerializeField]
    private GameObject _inGamePanel;

    [SerializeField]
    private GameObject _pausedPanel;

    [SerializeField]
    private GameObject _gameLostPanel;

    [SerializeField]
    private GameObject _gameWonPanel;

    [SerializeField]
    private Text _scoreText;

    [SerializeField]
    private Text _timeText;

    private Text _spawnOptionButtonText;

    private bool _isShowingPanel = false;

    private bool _isAnimatingPanel = false;

    private bool _isPaused = false;

    private void Start()
    {
        _spawnOptionButtonText = _spawnOptionButton.GetComponentInChildren<Text>();
    }

    public void ToggleSpawnOptionPanel()
    {
        if (_isAnimatingPanel)
        {
            return;
        }

        _isShowingPanel = !_isShowingPanel;

        if (_isShowingPanel)
        {
            _spawnOptionButtonText.text = "H\nI\nD\nE";
            StartCoroutine(AnimateHorizontalSpawnOptionPanel(340f, 0.5f));
        }
        else
        {
            _spawnOptionButtonText.text = "S\nH\nO\nW";
            StartCoroutine(AnimateHorizontalSpawnOptionPanel(-300f, 0.5f));
        }

        AudioManager.Instance.PlayClickSFX();
    }

    public void SetPointsText(float points)
    {
        _pointsText.text = points.ToString("0");
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
        _spawnCancelButton.SetActive(spawning);

        if (spawning)
        {
            _spawnOptionButtonText.text = "C\nA\nN\nC\nE\nL";
            StartCoroutine(AnimateHorizontalSpawnOptionPanel(-300f, 0.5f));
        }
        else
        {
            _spawnOptionButtonText.text = "H\nI\nD\nE";
            StartCoroutine(AnimateHorizontalSpawnOptionPanel(340f, 0.5f));
        }
    }

    public IEnumerator AnimateHorizontalSpawnOptionPanel(float target, float duration = 1f)
    {
        _isAnimatingPanel = true;
        RectTransform rectTransform = _spawnOptionPanel.GetComponent<RectTransform>();
        float start = rectTransform.anchoredPosition.x;
        float cur = 0f;
        while (cur < duration)
        {
            cur += Time.deltaTime;
            float curX = Mathf.SmoothStep(start, target, cur / duration);
            rectTransform.anchoredPosition = new Vector2(curX, rectTransform.anchoredPosition.y);
            yield return null;
        }
        _isAnimatingPanel = false;
    }

    public void TogglePaused()
    {
        _isPaused = !_isPaused;

        Time.timeScale = _isPaused ? 0 : 1;
        _inGamePanel.SetActive(!_isPaused);
        _pausedPanel.SetActive(_isPaused);

        AudioManager.Instance.PlayClickSFX();
    }

    public void GoToHome()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Home");

        AudioManager.Instance.PlayClickSFX();
    }

    public void ShowGameLostPanel()
    {
        _inGamePanel.SetActive(false);
        _gameLostPanel.SetActive(true);

        AudioManager.Instance.PlayClickSFX();
    }

    public void SetScoreText(float score)
    {
        _scoreText.text = score.ToString("0");
    }

    public void ShowGameWonPanel()
    {
        _inGamePanel.SetActive(false);
        _gameWonPanel.SetActive(true);
    }
    public void GoToNextLevel()
    {
        int nextLevel = SaveManager.CurrentLevel + 1;

        if (nextLevel <= SaveManager.MaxLevel)
        {
            SaveManager.CurrentLevel = nextLevel;
            SceneManager.LoadScene("Level" + nextLevel);
        }
        else
        {
            SceneManager.LoadScene("Home");
        }

        AudioManager.Instance.PlayClickSFX();
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        AudioManager.Instance.PlayClickSFX();
    }

    public void SetTimeText(float time)
    {
        float minutes = Mathf.Floor(time / 60);
        float seconds = time % 60;
        _timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
