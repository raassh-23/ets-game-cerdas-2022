using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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
            Debug.Log("EnvironmentManager is null");
        }

        Health = _initialHealth;
        _canAttack = false;
        _isNearEnemyCell = false;
        _isNearWall = false;
        _isNearOwnCell = false;
        
        _spriteRenderer.color = _initialColor;
        _attackTargets.Clear();

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

            attackTarget.TakeDamage(_damage);
            _canAttack = false;

            if (target.CompareTag("GoodCell") || target.CompareTag("BadCell"))
            {
                AddReward(3f*_existentialReward);
            }

            Debug.Log(gameObject.name + " has attacked " + target.name);

            // placeholder for animation
            _spriteRenderer.color = Color.gray;
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error: " + ex.Message);
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

        float healthPercent = Mathf.Clamp01(Health / _initialHealth);
        _healthFill.localScale = new Vector3(healthPercent, 1f, 1f);
        _healthFill.localPosition = new Vector3((1 - healthPercent) * -0.31f, 0f, 0f);

        if (Health <= 0)
        {
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

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_isGoodTroop);
        sensor.AddObservation(Health);
        sensor.AddObservation(_damage);
        sensor.AddObservation(_attackCooldown);
        sensor.AddObservation(_speed);
        sensor.AddObservation(_attackTargets.Count);
    }

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
            AddReward(_existentialReward);
        }

        if (_isNearWall || _isNearOwnCell)
        {
            AddReward(-_existentialReward);
        }

        if (environmentManager != null && environmentManager.isOneSideDestroyed)
        {
            AddReward(-_existentialReward);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var conActions = actionsOut.ContinuousActions;
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 curPos = transform.position;

            if (Vector2.Distance(mousePos, curPos) > 0.1f)
            {
                Vector2 dir = (mousePos - curPos).normalized;
                conActions[0] = dir.x;
                conActions[1] = dir.y;
            }
            else
            {
                conActions[0] = 0;
                conActions[1] = 0;
            }
        }
        else
        {
            conActions[0] = 0;
            conActions[1] = 0;
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
