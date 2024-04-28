using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlimeAI : MonoBehaviour
{
    private Animator _animator;
    public int HP;
    public float timeToDestroy = 1f;
    private bool _isDied;

    private GameManager _gameManager;

    public enemyState enemyState;
  

    //SLIME IA
    [SerializeField]
    private bool _isWalking;
    [SerializeField]
    private bool _isAlert;
    [SerializeField]
    private bool _isPlayerVisible;
    [SerializeField]
    private bool _isAttacking;
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
        _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        ChangeState(enemyState);
    }

    void Update()
    {
        StateManager();

        _isWalking = _agent.desiredVelocity.magnitude >= 0.1f;

        _animator.SetBool("isWalking", _isWalking);
        _animator.SetBool("isAlert", _isAlert);
    }

     private void GetHit(int amount)
    {
        if(_isDied == true ){return;}

        HP -= amount;

        if(HP > 0)
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
        if (_gameManager.gameState == GameState.GAMEOVER && (enemyState == enemyState.FOLLOW || enemyState == enemyState.FURY || enemyState == enemyState.ALERT))

        {
            ChangeState(enemyState.IDLE);
        }

        switch(enemyState)
        {
            case enemyState.ALERT:
                LookAt();
            break;

            case enemyState.FOLLOW:

            _timePlayerNotVisible += Time.deltaTime;

                LookAt();
            _destination = _gameManager.player.position;
            _agent.destination = _destination;

            if(_agent.remainingDistance <= _agent.stoppingDistance)
            {
                Attack();
            }

            if (!_isPlayerVisible)
            {
                if (_timePlayerNotVisible >= _gameManager.slimeTimeFollowLimit)
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
                _isAttacking = true;
                _destination = _gameManager.player.position;
                _agent.destination = _destination;

                if(_agent.remainingDistance <= _agent.stoppingDistance)
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

        

        switch(newState)
        {
            case enemyState.IDLE:
                _agent.stoppingDistance = 0;
                _destination = transform.position;
                _agent.destination = _destination;


                StartCoroutine(IDLE());
            break;

            case enemyState.ALERT:
                _agent.stoppingDistance = 0;
                _destination = transform.position;
                _agent.destination = _destination;
                _isAlert = true;

                StartCoroutine(ALERT());
            
            break;

            case enemyState.PATROL:
                _agent.stoppingDistance = 0;
                _idWayPoint = Random.Range(0, _gameManager.slimeWayPoints.Length);
                _destination = _gameManager.slimeWayPoints[_idWayPoint].position;
                _agent.destination = _destination;

                StartCoroutine(PATROL());
            break;

            case enemyState.FOLLOW:
                _agent.stoppingDistance = _gameManager.slimeDistanceToAttack;

            break;

            case enemyState.FURY:
                _destination = transform.position; 
                _agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                _agent.destination = _destination;
            break;

            case enemyState.DEAD:
            _destination = transform.position;
            _agent.destination = _destination;
            break;
        }

        enemyState = newState;

    }

    private void OnTriggerEnter(Collider other) 
    {
        if(_gameManager.gameState != GameState.GAMEPLAY){return;}
        
        if(other.gameObject.tag == "Player")
        {
            _isPlayerVisible = true;

            if (enemyState == enemyState.IDLE || enemyState == enemyState.PATROL)
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
        if(other.gameObject.tag == "Player")
        {
            _isPlayerVisible = false;
        }
    }
    
    #region COROUTINES

    IEnumerator IDLE()
    {
        yield return new WaitForSeconds(_gameManager.slimeIdleWaitTime);

       StayStill(50);
    }


    IEnumerator PATROL()
    {
        yield return new WaitUntil(()=> _agent.remainingDistance <= 0);
        StayStill(60);
    }


    IEnumerator ALERT()
    {
        yield return new WaitForSeconds(_gameManager.slimeAlertTime);

        if(_isPlayerVisible)
        {
            ChangeState(enemyState.FOLLOW);
        }
        else
        {
            StayStill(20);
        }

    }

    IEnumerator ATTACK()
    {
        yield return new WaitForSeconds(_gameManager.slimeAttackDelay);

        _isAttacking = false;

    }


    IEnumerator Dead()
    {
        _isDied = true;
        yield return new WaitForSeconds(timeToDestroy);
        if(_gameManager.Perc(_gameManager.percDrop))
        {
            Instantiate(_gameManager.gemPrefab, transform.position,  _gameManager.gemPrefab.transform.rotation);
        }
        Destroy(this.gameObject);
    }
    #endregion

    int Rand()
    {
        int random = Random.Range(0,100);
        return random;
    }

    void StayStill(int yes)
    {
        ChangeState(Rand() <= yes ? enemyState.IDLE : enemyState.PATROL);
    }

    private void Attack()
    {
        if (_isAttacking || !_isPlayerVisible) return;
        _isAttacking = true;
        _animator.SetTrigger("Attack");
    }

      public void AttackIsDone()
    {
        StartCoroutine(ATTACK());
    }

      private void LookAt()
    {
        var lookDirection = (_gameManager.player.position - transform.position).normalized;
        var lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _gameManager.slimeLookAtSpeed * Time.deltaTime);
    }
}
