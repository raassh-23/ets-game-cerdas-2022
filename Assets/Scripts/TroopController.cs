using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TroopController : MonoBehaviour
{
    [SerializeField]
    private float _damage = 3f;

    [SerializeField]
    private float _health = 10f;

    [SerializeField]
    private float _speed = 1f;

    private Rigidbody2D _rigidbody;
    private Vector2 _direction;

    public UnityAction<TroopController> OnTroopDeath = delegate {};

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // get mouse position when holding lmb and change direction to mouse position
        // just placeholder to test movement
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 curPos = transform.position;

            if (Vector2.Distance(mousePos, curPos) > 0.1f)
            {
                _direction = (mousePos - curPos).normalized;
            }
            else
            {
                _direction = Vector2.zero;
            }
        } else {
            _direction = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        MoveTroop();
    }

    private void MoveTroop()
    {
        _rigidbody.velocity = _direction * _speed;

        if (_direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void Death() {
        OnTroopDeath(this);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (tag == "GoodCell" && other.gameObject.tag == "BadCell"
            || tag == "BadCell" && other.gameObject.tag == "GoodCell") {
            Death();
        }
    }
}
