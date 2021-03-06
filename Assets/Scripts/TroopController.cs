using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;

public class TroopController : Agent, IAttackable
{
    [SerializeField]
    private float _damage = 3f;

    [SerializeField]
    private float _initialHealth = 10f;

    public float Health; //{ get; private set; }

    [SerializeField]
    private float _speed = 1f;

    [SerializeField]
    private float _attackCooldown = 2f;

    private Rigidbody2D _rigidbody;
    private Vector2 _direction;

    public UnityAction<TroopController> OnTroopDeath = delegate { };

    private List<GameObject> _attackTargets { get; set; }

    [SerializeField]
    private bool _canAttack = false;

    private SpriteRenderer _spriteRenderer;

    private EnvironmentManager environmentManager;

    private float _existentialReward = 0f;

    private Color _initialColor;

    [SerializeField]
    private Transform _healthBar;

    [SerializeField]
    private Transform _healthFill;

    [SerializeField]
    private bool _isGoodTroop = false;

    [SerializeField]
    private bool _isNearEnemyCell = false;

    [SerializeField]
    private bool _isNearWall = false;

    [SerializeField]
    private bool _isNearOwnCell = false;

    [SerializeField]
    private bool _isDamaged = false;

    [SerializeField]
    [Range(0f, 1f)]
    private float _criticalChance = 0.2f;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _attackTargets = new List<GameObject>();

        _initialColor = _spriteRenderer.color;
    }

    public void InitTroop()
    {
        environmentManager = GetComponentInParent<EnvironmentManager>();
        if (environmentManager != null)
        {
            _existentialReward = 1f / environmentManager.MaxEnvironmentSteps;
        }
        else
        {
            _existentialReward = 0f;
            Debug.Log("EnvironmentManager is null");
        }

        Health = _initialHealth;
        _canAttack = false;
        _isNearEnemyCell = false;
        _isNearWall = false;
        _isNearOwnCell = false;
        _isDamaged = false;

        _spriteRenderer.color = _initialColor;
        _attackTargets.Clear();

        _healthFill.localScale = new Vector3(1f, 1f, 1f);
        _healthFill.localPosition = new Vector3(0f, 0f, 0f);

        if (gameObject.CompareTag("GoodTroop"))
        {
            _isGoodTroop = true;
        }
        else
        {
            _isGoodTroop = false;
        }

        // Debug.Log("existential reward: " + _existentialReward);
    }

    private void FixedUpdate()
    {
        MoveTroop();
    }

    private void MoveTroop()
    {
        _rigidbody.velocity = _direction * _speed;
    }

    private IEnumerator Attack()
    {
        if (!_canAttack)
            yield break;

        try
        {
            GameObject target = NearestObject(_attackTargets);
            IAttackable attackTarget = target.GetComponent<IAttackable>();

            float damage = _damage;
            if (Random.value < _criticalChance)
            {
                damage *= Random.Range(1.5f, 2f);
            }

            attackTarget.TakeDamage(damage);
            _canAttack = false;

            if (target.CompareTag("GoodCell") || target.CompareTag("BadCell"))
            {
                AddReward(10f * _existentialReward);
            }

            Debug.Log(gameObject.name + " has attacked " + target.name);

            // placeholder for animation
            _spriteRenderer.color = Color.gray;
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error: " + ex.Message);
            Debug.Log(ex.StackTrace);
            yield break;
        }

        yield return new WaitForSeconds(_attackCooldown);

        CheckCanAttack();
        _spriteRenderer.color = _initialColor;
    }

    private GameObject NearestObject(List<GameObject> objects)
    {
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
        Health -= damage;
        _isDamaged = true;

        float healthPercent = Mathf.Clamp01(Health / _initialHealth);
        _healthFill.localScale = new Vector3(healthPercent, 1f, 1f);
        _healthFill.localPosition = new Vector3((1 - healthPercent) * -0.31f, 0f, 0f);

        if (Health <= 0)
        {
            if (!_isGoodTroop)
            {
                GameManager.AddScore(10);
            }
            
            OnTroopDeath(this);
        }
    }

    private void CheckCanAttack()
    {
        if (_attackTargets.Count == 0)
        {
            _canAttack = false;
        }
        else
        {
            _canAttack = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((_isGoodTroop && (other.gameObject.CompareTag("BadCell")
                || other.gameObject.CompareTag("BadTroop")))
            || (!_isGoodTroop && (other.gameObject.CompareTag("GoodCell")
                || other.gameObject.CompareTag("GoodTroop"))))
        {
            _attackTargets.Add(other.gameObject);
            CheckCanAttack();
        }

        if ((_isGoodTroop && other.gameObject.CompareTag("GoodCell"))
            || (!_isGoodTroop && other.gameObject.CompareTag("BadCell")))
        {
            _isNearOwnCell = true;
        }

        if ((_isGoodTroop && other.gameObject.CompareTag("BadCell"))
            || (!_isGoodTroop && other.gameObject.CompareTag("GoodCell")))
        {
            _isNearEnemyCell = true;
        }

        if (other.gameObject.CompareTag("wall"))
        {
            _isNearWall = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((_isGoodTroop && (other.gameObject.CompareTag("BadCell")
                || other.gameObject.CompareTag("BadTroop")))
            || (!_isGoodTroop && (other.gameObject.CompareTag("GoodCell")
                || other.gameObject.CompareTag("GoodTroop"))))
        {
            _attackTargets.Remove(other.gameObject);
            CheckCanAttack();
        }

        if ((_isGoodTroop && other.gameObject.CompareTag("GoodCell"))
            || (!_isGoodTroop && other.gameObject.CompareTag("BadCell")))
        {
            _isNearOwnCell = false;
        }

        if ((_isGoodTroop && other.gameObject.CompareTag("BadCell"))
            || (!_isGoodTroop && other.gameObject.CompareTag("GoodCell")))
        {
            _isNearEnemyCell = false;
        }

        if (other.gameObject.CompareTag("wall"))
        {
            _isNearWall = false;
        }
    }

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     sensor.AddObservation(Health);
    //     sensor.AddObservation(_speed);
    //     sensor.AddObservation(_attackCooldown);
    //     sensor.AddObservation(_damage);
    //     sensor.AddObservation(_attackTargets.Count);
    // }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        _direction = new Vector2(moveX, moveY).normalized;

        switch (actions.DiscreteActions[0])
        {
            case 1:
                StartCoroutine(Attack());
                break;
            default:
                break;
        }

        if (_isNearEnemyCell)
        {
            AddReward(3 * _existentialReward);
        }

        if (_isNearOwnCell || _isNearWall)
        {
            AddReward(-2 * _existentialReward);
        }

        if (_isNearWall)
        {
            AddReward(-5 * _existentialReward);
        }

        if (_isDamaged)
        {
            AddReward(-3 * _existentialReward);
            _isDamaged = false;
        }

        if (environmentManager.isOneSideDestroyed)
        {
            AddReward(-1 * _existentialReward);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var conActions = actionsOut.ContinuousActions;
        if (_attackTargets.Count > 0)
        {
            Vector3 nearestPos = NearestObject(_attackTargets).transform.position;
            Vector2 dir = (nearestPos - transform.position).normalized;
            conActions[0] = dir.x;
            conActions[1] = dir.y;
        }
        else
        {
            conActions[0] = Random.Range(-1f, 1f);
            conActions[1] = Random.Range(-1f, 1f);
        }

        if (_canAttack)
        {
            actionsOut.DiscreteActions.Array[0] = 1;
        }
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(0, 1, _canAttack);
    }
}
