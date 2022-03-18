using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CellController : MonoBehaviour, IAttackable
{
    [SerializeField]
    private float _health = 100f;

    public float Health { 
        get { return _health; }
    }

    public UnityAction<CellController> OnCellDestroyed = delegate {};

    public void TakeDamage(float damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            OnCellDestroyed(this);
            Destroy(gameObject);
        }
    }
}
