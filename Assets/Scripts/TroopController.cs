using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Events;

public class TroopController : MonoBehaviour, IAttackable
{
    [SerializeField]
    private float _damage = 3f;

    [SerializeField]
    private float _health = 10f;

    [SerializeField]
    private float _speed = 1f;

    [SerializeField]
    private float _attackCooldown = 2f;

    private Rigidbody2D _rigidbody;
    private Vector2 _direction;

    public UnityAction<TroopController> OnTroopDeath = delegate {};

    private List<GameObject> _attackTargets { get; set; }

    private bool _canAttack = false;

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _attackTargets = new List<GameObject>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
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

        // placeholder to attack 
        if (_canAttack)
        {
            StartCoroutine(Attack());
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

    private IEnumerator Attack()
    {
        if (!_canAttack)
            yield break;
        
        Debug.Log(gameObject.name + " attacking");
        IAttackable attackTarget = NearestObject(_attackTargets).GetComponent<IAttackable>();
        attackTarget.TakeDamage(_damage);
        _canAttack = false;
        
        // placeholder for animation
        Color color = _spriteRenderer.color;
        _spriteRenderer.color = Color.gray;

        yield return new WaitForSeconds(_attackCooldown);

        CheckCanAttack();
        _spriteRenderer.color = color;
    }

    private GameObject NearestObject(List<GameObject> objects) {
        GameObject target = null;

        float minDistance = Mathf.Infinity;
        foreach (GameObject obj in objects)
        {
            Vector2 dirToTarget = obj.transform.position - transform.position;
            float distSqr = dirToTarget.sqrMagnitude;

            if (distSqr < minDistance)
            {
                minDistance = distSqr;
                target = obj;
            }
        }

        return target;
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            OnTroopDeath(this);
        }
    }

    private void CheckCanAttack() {
        if (_attackTargets.Count == 0) {
            _canAttack = false;
        } else {
            _canAttack = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (tag == "GoodCell" && other.gameObject.tag == "BadCell"
            || tag == "BadCell" && other.gameObject.tag == "GoodCell") {
            _attackTargets.Add(other.gameObject);
            CheckCanAttack();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (tag == "GoodCell" && other.gameObject.tag == "BadCell"
            || tag == "BadCell" && other.gameObject.tag == "GoodCell") {
            _attackTargets.Remove(other.gameObject);
            CheckCanAttack();
        }
    }
}
