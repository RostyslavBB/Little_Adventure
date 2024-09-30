using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    //Player
    [Header("Movement Settings")]

    private CharacterController _characterController;
    private Vector3 _movementVelocity;
    private PlayerInput _playerInput;
    private Animator _playerAnimator;
    private float _verticalVelocity;
    private float _attackAnimationDuration;
    
    public float moveSpeed = 0f;
    public float gravity = -9.8f;
    public int coin;

    [Space(10)]
    [Header("Player Settings")]
    //Enemy
    public bool isPlayer = true;
    public float spawnDuration = 2f;

    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Transform _targetPlayer;
    private float _currentSpawnTime;

    //State Machine
    public enum characterState
    {
        Normal,
        Attacking,
        Dead,
        BeingHit,
        Slide,
        Spawn
    }
    public characterState currentState;

    //Player Slides
    private float _attackStartTime;
    private Vector3 _impactOnCharacter;

    public float attackSlideDuration = 0.4f;
    public float attackSlideSpeed = 0.06f;
    public bool isInvincible;
    public float invincibleDuration = 2f;
    public float slideSpeed = 9f;

    //Health
    private Health _health;

    //Damage Caster
    DamageCaster _damageCaster;

    //Material animation
    private MaterialPropertyBlock _materialPropertyBlock;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    //Drop Item
    [Space(10)]
    [Header("Item Settings")]
    public GameObject dropItem;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerAnimator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _damageCaster = GetComponentInChildren<DamageCaster>();   

        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        _skinnedMeshRenderer.GetPropertyBlock(_materialPropertyBlock);

        if (!isPlayer)
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            _targetPlayer = GameObject.FindWithTag("Player").transform;
            _navMeshAgent.speed = moveSpeed;
            SwitchStateTo(characterState.Spawn);
        }
        else
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }
    private void CalculateEnemyMovement()
    {
        if (Vector3.Distance(_targetPlayer.position, transform.position) >= _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.SetDestination(_targetPlayer.position);
            _playerAnimator.SetFloat("Speed", 0.2f);
        }
        else
        {
            _navMeshAgent.SetDestination(transform.position);
            _playerAnimator.SetFloat("Speed", 0);
            SwitchStateTo(characterState.Attacking);
        }

    }
    private void CalculatePlayerMovement()
    {
        if (_playerInput.mouseButtonDown && _characterController.isGrounded)
        {
            SwitchStateTo(characterState.Attacking);
            return;
        }
        else if(_playerInput.spaceKeyDown && _characterController.isGrounded)
        {
            SwitchStateTo(characterState.Slide);
            return;
        }
        _movementVelocity.Set(_playerInput.horizontalAxis, 0f, _playerInput.verticalAxis);
        _movementVelocity.Normalize();
        _movementVelocity = Quaternion.Euler(0f, -45f, 0f) * _movementVelocity;
        _playerAnimator.SetFloat("Speed", _movementVelocity.magnitude);
        _movementVelocity *= moveSpeed * Time.deltaTime;

        if (_movementVelocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_movementVelocity);

        _playerAnimator.SetBool("AirBorne", !_characterController.isGrounded);
    }
    private void FixedUpdate()
    {
        switch(currentState)
        {
            case characterState.Normal:
                if (isPlayer)
                    CalculatePlayerMovement();
                else
                    CalculateEnemyMovement();
                break;
            case characterState.Attacking:
                if (isPlayer)
                {
                    if (Time.time < _attackStartTime + attackSlideDuration)
                    {
                        float timePassed = Time.time - _attackStartTime;
                        float lerpTime = timePassed / attackSlideDuration;
                        _movementVelocity = Vector3.Lerp(transform.forward * attackSlideSpeed, Vector3.zero, lerpTime);
                    }
                    if(_playerInput.mouseButtonDown && _characterController.isGrounded)
                    {
                        string currentClipName = _playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                        _attackAnimationDuration = _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                        if(currentClipName != "LittleAdventurerAndie_ATTACK_03" && _attackAnimationDuration > 0.5f && _attackAnimationDuration < 0.7f)
                        {
                            _playerInput.mouseButtonDown = false;
                            SwitchStateTo(characterState.Attacking);
                        }
                    }
                }
                break;
            case characterState.Dead:
                return;
            case characterState.BeingHit: 
                BeingHitAnimationHit();
                break;
            case characterState.Slide:
                _movementVelocity = transform.forward * slideSpeed * Time.deltaTime;
                break;
            case characterState.Spawn:
                _currentSpawnTime -= Time.deltaTime;
                if (_currentSpawnTime <= 0)
                    SwitchStateTo(characterState.Normal);
                break;
        }
        if (_impactOnCharacter.magnitude > 0.2f)
            _movementVelocity = _impactOnCharacter * Time.deltaTime;
        _impactOnCharacter = Vector3.Lerp(_impactOnCharacter, Vector3.zero, Time.deltaTime * 5);
        if (isPlayer)
        {
            if (!_characterController.isGrounded)
                _verticalVelocity = gravity;
            else
                _verticalVelocity = gravity * 0.3f;

            _movementVelocity += _verticalVelocity * Vector3.up * Time.deltaTime;
            _characterController.Move(_movementVelocity);
            _movementVelocity = Vector3.zero;
        }
        else
        {
            if(currentState != characterState.Normal) 
            {
                _characterController.Move(_movementVelocity);
                _movementVelocity = Vector3.zero;
            }
        }
    }
    public void SwitchStateTo(characterState newState)
    {
        if(isPlayer)
            _playerInput.ClearCache();

        switch(currentState)
        {
            case characterState.Normal:
                break;
            case characterState.Attacking: 
                if(_damageCaster != null)
                    DisableDamageCaster();
                if(isPlayer)
                    GetComponent<ControlPlayerVFX>().StopBlade();
                break;
            case characterState.Dead:
                return;
            case characterState.BeingHit:
                break;
            case characterState.Slide:
                break;
            case characterState.Spawn:
                isInvincible = false;
                break;
        }

        switch (newState)
        {
            case characterState.Normal:
                break;
            case characterState.Attacking:
                _playerAnimator.SetTrigger("Attack");
                if (isPlayer)
                {
                    _attackStartTime = Time.time;
                    RotateToCursor();
                }
                else
                {
                    Quaternion newRotation = Quaternion.LookRotation(_targetPlayer.position - transform.position);
                    transform.rotation = newRotation;
                }
                break;
            case characterState.Dead:
                _characterController.enabled = false;
                _playerAnimator.SetTrigger("Dead");
                StartCoroutine(MaterialDisolve());
                if(!isPlayer)
                {
                    SkinnedMeshRenderer mesh = GetComponentInChildren<SkinnedMeshRenderer>();
                    mesh.gameObject.layer = 0;
                }    
                break;
            case characterState.BeingHit:
                _playerAnimator.SetTrigger("BeingHit");
                if(isPlayer)
                {
                    isInvincible = true;
                    StartCoroutine(DelayCancalInvincible());
                }
                break;
            case characterState.Slide:
                _playerAnimator.SetTrigger("Slide");
                break;
            case characterState.Spawn:
                isInvincible = true;
                _currentSpawnTime = 1;
                StartCoroutine(MaterialAppear());
                break;
        }
        currentState = newState;
    }
    public void AttackAnimationEnds()
    {
        SwitchStateTo(characterState.Normal);
    }
    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {
        if (isInvincible)
            return;
        if (_health != null)
            _health.ApplyDamage(damage);
        if (!isPlayer)
            GetComponent<EnemyVFXManager>().PlayBeingHitVFX(attackerPos);
        StartCoroutine(MaterialBlink());
        if (isPlayer)
        {
            SwitchStateTo(characterState.BeingHit);
            AddImpact(attackerPos, 10f);
        }
        else
        {
            AddImpact(attackerPos, 2.5f);
        }
    }
    public void EnableDamageCaster()
    {
        _damageCaster.EnableDamageCaster();
    }
    public void DisableDamageCaster()
    {
       _damageCaster.DisableDamageCaster();
    }
    public void DropItem()
    {
        if (dropItem != null)
        {
            Instantiate(dropItem, transform.position, Quaternion.identity);
        }
    }
    public void BeingHitAnimationHit()
    {
        SwitchStateTo(characterState.Normal);
    }
    private void AddImpact(Vector3 attackerPos, float force)
    {
        Vector3 impactDirection = transform.position - attackerPos;
        impactDirection.Normalize();
        impactDirection.y = 0f;
        _impactOnCharacter = impactDirection * force;
    }
    public void PickUpItem(PickUp item)
    {
        switch(item.type)
        {
            case PickUp.pickUpType.Heal:
                AddHealth(item.getFromPickUp);
                break;
            case PickUp.pickUpType.Coin:
                AddCoin(item.getFromPickUp);
                break;
        }
    }
    private void AddHealth(int health)
    {
        _health.AddHealth(health);
        GetComponent<ControlPlayerVFX>().PlayHealVFX();
    }
    private void AddCoin(int coin)
    {
        this.coin += coin;
    }
    public void SlideAnimationEnds() 
    {
        SwitchStateTo(characterState.Normal);
    }
    public void RotateToTarget()
    {
        if (currentState != characterState.Dead)
        {
            transform.LookAt(_targetPlayer, Vector3.up);
        }
    }
    private void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;
        if (Physics.Raycast(ray, out hitResult, 1000, 1 << LayerMask.NameToLayer("CursorTest"))) 
        {
            Vector3 cursorPos = hitResult.point;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cursorPos, 1);
        }
    }
    private void RotateToCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;
        if (Physics.Raycast(ray, out hitResult, 1000, 1 << LayerMask.NameToLayer("CursorTest")))
        {
            Vector3 cursorPos = hitResult.point;
            transform.rotation = Quaternion.LookRotation(cursorPos - transform.position, Vector3.up);
        }
    }
    IEnumerator MaterialBlink()
    {
        _materialPropertyBlock.SetFloat("_blink", 0.4f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        yield return new WaitForSeconds(0.2f);
        _materialPropertyBlock.SetFloat("_blink", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
    IEnumerator MaterialDisolve()
    {
        yield return new WaitForSeconds(2f);

        float dissolveTimeDuration = 2f;
        float currentDissolveTime = 0f;
        float dissolveHightStart = 20f;
        float dissolveHightTarget = -10f;
        float dissolveHeight;

        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHightStart, dissolveHightTarget, currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }
        DropItem();
    }
    IEnumerator DelayCancalInvincible() 
    {
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }
    IEnumerator MaterialAppear() 
    {
        float dissolveTimeDuration = spawnDuration;
        float currentDissolveTime = 0f;
        float dissolveHightStart = -10f;
        float dissolveHightTarget = 20f;
        float dissolveHeight;

        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock( _materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHeight = Mathf.Lerp(dissolveHightStart, dissolveHightTarget, currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHeight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }
        _materialPropertyBlock.SetFloat("_enableDissolve", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}

