using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TroopController : Agent, IAttackable
{
    [SerializeField]
    private float _damage = 3f;

    [SerializeField]
    private float _initialHealth = 10f;

    public float Health { get; private set; }

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

    private EnvironmentManager environmentManager;

    private float _existentialReward;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _attackTargets = new List<GameObject>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Health = _initialHealth;
        environmentManager = GetComponentInParent<EnvironmentManager>();
        _existentialReward = 1f / environmentManager.MaxEnvironmentSteps;
    }

    // private void Update()
    // {
    //     // get mouse position when holding lmb and change direction to mouse position
    //     // just placeholder to test movement
    //     if (Input.GetMouseButton(0))
    //     {
    //         Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //         Vector2 curPos = transform.position;

    //         if (Vector2.Distance(mousePos, curPos) > 0.1f)
    //         {
    //             _direction = (mousePos - curPos).normalized;
    //         }
    //         else
    //         {
    //             _direction = Vector2.zero;
    //         }
    //     } else {
    //         _direction = Vector2.zero;
    //     }

    //     // placeholder to attack 
    //     if (_canAttack)
    //     {
    //         StartCoroutine(Attack());
    //     }
    // }

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
        
        GameObject target = NearestObject(_attackTargets);
        IAttackable attackTarget = target.GetComponent<IAttackable>();

        if (_attackTargets == null) {
            yield break;
        }

        attackTarget.TakeDamage(_damage);
        _canAttack = false;  

        Debug.Log(gameObject.name + " has attacked " + target.name);

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
        Health -= damage;

        if (Health <= 0)
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
        if (tag == "GoodTroop" && other.gameObject.tag == "BadTroop"
            || tag == "BadTroop" && other.gameObject.tag == "GoodTroop") {
            _attackTargets.Add(other.gameObject);
            CheckCanAttack();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (tag == "GoodTroop" && other.gameObject.tag == "BadTroop"
            || tag == "BadTroop" && other.gameObject.tag == "GoodTroop") {
            _attackTargets.Remove(other.gameObject);
            CheckCanAttack();
        }
    }

    // public override void OnEpisodeBegin()
    // {
    //     environmentManager.InitScene();
    // }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Health);
        sensor.AddObservation(_damage);
        sensor.AddObservation(_attackCooldown);
        sensor.AddObservation(_speed);
        sensor.AddObservation(_canAttack);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(_existentialReward);

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
        } else {
            conActions[0] = 0;
            conActions[1] = 0;
        }

        if (_canAttack)
        {
            actionsOut.DiscreteActions.Array[0] = 1;
        }
    }
}
