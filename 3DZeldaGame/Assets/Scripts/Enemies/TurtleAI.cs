using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TurtleAI : MonoBehaviour
{
    private Animator _animator;

    public int HP;

    public float timeToDestroy = 1f;

    private bool _isDied;

    private GameManager _gameManager;

    public enemyState enemyState;

    //TURTLE IA
    [SerializeField]
    private bool _isWalking;

    [SerializeField]
    private bool _isAlert;

    [SerializeField]
    private bool _isPlayerVisible;

    [SerializeField]
    private bool _isAttacking;

    [SerializeField]
    private bool _isFury;

    [SerializeField]
    private NavMeshAgent _agent;

    [SerializeField]
    private Vector3 _destination;

    [SerializeField]
    private int _idWayPoint;

    [SerializeField]
    private float _timePlayerNotVisible;

    void Start()
    {
        _gameManager = FindObjectOfType(typeof (GameManager)) as GameManager;
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        ChangeState (enemyState);
    }

    void Update()
    {
        StateManager();
        AnimationUpdate();
    }

    private void GetHit(int amount)
    {
        if (_isDied == true)
        {
            return;
        }

        HP -= amount;

        if (HP > 0)
        {
            ChangeState(enemyState.FURY);
            _animator.SetTrigger("GetHit");
        }
        else
        {
            ChangeState(enemyState.DEAD);
            StartCoroutine(Dead());
            _animator.SetTrigger("Death");
        }
    }

    void StateManager()
    {
        if (
            _gameManager.gameState == GameState.GAMEOVER &&
            (
            enemyState == enemyState.FOLLOW ||
            enemyState == enemyState.FURY ||
            enemyState == enemyState.ALERT
            )
        )
        {
            ChangeState(enemyState.IDLE);
        }

        switch (enemyState)
        {
            case enemyState.ALERT:
                LookAt();
                break;
            case enemyState.FOLLOW:
                _timePlayerNotVisible += Time.deltaTime;

                LookAt();
                _destination = _gameManager.player.position;
                _agent.destination = _destination;

                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    Attack();
                }

                if (!_isPlayerVisible)
                {
                    if (
                        _timePlayerNotVisible >=
                        _gameManager.turtleTimeFollowLimit
                    )
                    {
                        ChangeState(enemyState.PATROL);
                    }
                }
                else
                {
                    _timePlayerNotVisible = 0f; 
                }

                break;
            case enemyState.FURY:
                LookAt();
                _destination = _gameManager.player.position;
                _agent.destination = _destination;

                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    Attack();
                }
                break;
        }
    }

    void ChangeState(enemyState newState)
    {
        StopAllCoroutines();
        _isAlert = false;

        switch (newState)
        {
            case enemyState.IDLE:
                _agent.stoppingDistance = 0;
                _destination = transform.position;
                _agent.destination = _destination;

                StartCoroutine(IDLE());
                break;
            case enemyState.PATROL:
                _agent.stoppingDistance = 0;
                _idWayPoint =
                    Random.Range(0, _gameManager.turtleWayPoints.Length);
                _destination =
                    _gameManager.turtleWayPoints[_idWayPoint].position;
                _agent.destination = _destination;

                StartCoroutine(PATROL());
                break;
            case enemyState.ALERT:
                _agent.stoppingDistance = 0;
                _destination = transform.position;
                _agent.destination = _destination;
                _isAlert = true;

                StartCoroutine(ALERT());

                break;
            case enemyState.FOLLOW:
                _agent.stoppingDistance = _gameManager.turtleDistanceToAttack;

                break;
            // case enemyState.ATTACK:
            // StartCoroutine(ATTACK());
            // break;
            case enemyState.SHIELD:
                StartCoroutine(SHIELD());

                break;
            case enemyState.FURY:
                _isFury = true;
                _isAttacking = false;
                _destination = transform.position; 
                _agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                _agent.destination = _destination;
                break;
        }

        enemyState = newState;
        Debug.Log (newState);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_gameManager.gameState != GameState.GAMEPLAY)
        {
            return;
        }

        if (other.gameObject.tag == "Player")
        {
            _isPlayerVisible = true;

            if (enemyState == enemyState.IDLE || enemyState == enemyState.PATROL
            )
            {
                ChangeState(enemyState.ALERT);
            }
            else if (enemyState == enemyState.FOLLOW)
            {
                _timePlayerNotVisible = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            _isPlayerVisible = false;
        }
    }


#region COROUTINES

    IEnumerator IDLE()
    {
        yield return new WaitForSeconds(_gameManager.turtleIdleWaitTime);

        StayStill(50); 
    }

    IEnumerator PATROL()
    {
        yield return new WaitUntil(() => _agent.remainingDistance <= 0);
        StayStill(20); 
    }

    IEnumerator ALERT()
    {
        yield return new WaitForSeconds(_gameManager.turtleAlertTime);

        if (_isPlayerVisible)
        {
            ChangeState(enemyState.FOLLOW);
        }
        else
        {
            ChangeState(enemyState.PATROL);
        }
    }

    IEnumerator ATTACK()
    {
        yield return new WaitForSeconds(_gameManager.slimeAttackDelay);

        _isAttacking = false;

        if (_isFury)
        {
            DefendOrFury(50);
        }
        else
        {
            DefendOrFollow(50);
        }
    }

   IEnumerator SHIELD()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _animator.SetTrigger("Shield");
            yield return new WaitForSeconds(_gameManager.turtleTimeShield);

            if (_isPlayerVisible)
            {
                if (_isFury)
                {
                    ChangeState(enemyState.FURY);
                }
                else
                {
                    ChangeState(enemyState.FOLLOW);
                }
            }
            else if (_isFury)
            {
                ChangeState(enemyState.FURY);
            }
            else
            {
                ChangeState(enemyState.PATROL);
            }
        }
        else
        {
            if (_isPlayerVisible && !_isFury)
            {
                ChangeState(enemyState.FOLLOW);
            }
            else if (_isFury)
            {
                ChangeState(enemyState.FURY);
            }
        }
    }


    IEnumerator Dead()
    {
        _isDied = true;
        yield return new WaitForSeconds(timeToDestroy);
        if (_gameManager.Perc(_gameManager.percDrop))
        {
            Instantiate(_gameManager.gemPrefab,
            transform.position,
            _gameManager.gemPrefab.transform.rotation);
        }
        Destroy(this.gameObject);
    }


#endregion



#region METODS
    void StayStill(int yes)
    {
        if (Rand() <= yes)
        {
            ChangeState(enemyState.IDLE);
        }
        else
        {
            ChangeState(enemyState.PATROL);
        }
    }

    void DefendOrFollow(int yes)
    {
        if (Rand() <= yes)
        {
            ChangeState(enemyState.SHIELD);
        }
        else
        {
            ChangeState(enemyState.FOLLOW);
        }
    }

    void DefendOrFury(int yes)
    {
        if (Rand() <= yes)
        {
            ChangeState(enemyState.SHIELD);
        }
        else
        {
            ChangeState(enemyState.FURY);
        }
    }

    int Rand()
    {
        int random = Random.Range(0, 100);
        return random;
    }

    void AnimationUpdate()
    {
        if (_agent.desiredVelocity.magnitude >= 0.1f)
        {
            _isWalking = true;
        }
        else
        {
            _isWalking = false;
        }

        _animator.SetBool("isWalking", _isWalking);
        _animator.SetBool("isAlert", _isAlert);
    }

    void Attack()
    {
        if (!_isAttacking && _isPlayerVisible)
        {
            _isAttacking = true;
            _animator.SetTrigger("Attack");
        }
    }

    public void AttackIsDone()
    {
        StartCoroutine(ATTACK());
        Debug.Log("Enemie attack done");
    }

    void LookAt()
    {
        Vector3 lookDirection =
            (_gameManager.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation =
            Quaternion
                .Slerp(transform.rotation,
                lookRotation,
                _gameManager.turtleLookAtSpeed * Time.deltaTime);
    }
#endregion
}
