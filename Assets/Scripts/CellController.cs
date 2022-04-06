using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CellController : MonoBehaviour, IAttackable
{
    [SerializeField]
    private float _initialHealth = 100f;

    public float Health; // { get; private set; }

    [SerializeField]
    private Transform _healthFill;

    public UnityAction<CellController> OnCellDestroyed = delegate {};

    private SpriteRenderer _spriteRenderer;
    private Color _initialColor;

    private Coroutine _curCoroutine = null;

    public UnityAction<CellController> OnCellClicked = delegate { };

    private void Start() {
        Health = _initialHealth;
        _healthFill.localScale = new Vector3(1f, 1f, 1f);

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialColor = _spriteRenderer.color;
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;

        float healthPercent = Mathf.Clamp01(Health / _initialHealth);
        _healthFill.localScale = new Vector3(healthPercent, 1f, 1f);
        _healthFill.localPosition = new Vector3((1-healthPercent) * -0.315f, 0f, 0f);

        if (Health <= 0)
        {
            OnCellDestroyed(this);
            Destroy(gameObject);
        }
    }

    public void StartBlinking()
    {
        _curCoroutine = StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            _spriteRenderer.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            _spriteRenderer.color = _initialColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void StopBlinking()
    {
        StopCoroutine(_curCoroutine);
        _curCoroutine = null;
        _spriteRenderer.color = _initialColor;
    }

    private void OnMouseDown() {
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("Left clicked from cell");
            OnCellClicked(this);
        }
    }
}
