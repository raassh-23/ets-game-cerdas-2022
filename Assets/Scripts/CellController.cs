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

    private void Start() {
        Health = _initialHealth;
        _healthFill.localScale = new Vector3(1f, 1f, 1f);
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;

        float healthPercent = Mathf.Clamp01(Health / _initialHealth);
        _healthFill.localScale = new Vector3(healthPercent, 1f, 1f);
        _healthFill.localPosition = new Vector3((1-healthPercent) * -0.31f, 0f, 0f);

        if (Health <= 0)
        {
            OnCellDestroyed(this);
            Destroy(gameObject);
        }
    }
}
