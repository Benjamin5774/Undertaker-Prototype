using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject player;
    [SerializeField] private GameObject bossShadowPrefab;

    [Header("Basic")]
    [SerializeField] float bossMaxHealth = 100f;
    [SerializeField] float bossMoveSpeed = 5f;
    private float currentMoveSpeed;
    [SerializeField] float bossHealth;
    [Header("Slowdown Factor")]
    [SerializeField] float slowFactor = 1000.0f; // The factor by which the speed is reduced


    [Header("Teleport")]
    [SerializeField] float teleportDistance = 3f;
    [SerializeField] float teleportDelay = 2f;
    private float currentTeleportDelay;

    [Header("Charge Attack")]
    [SerializeField] float chargePrepareTime = 1f;
    [SerializeField] float chargeSpeed = 25f;
    private float currentChargeSpeed;
    private float currentChargePrepareTime;
    [SerializeField] float chargeOffset = 3f;

    [Header("Action Cooldown")]
    [SerializeField] float minActionCooldown = 5f;
    [SerializeField] float maxActionCooldown = 10f;
    [SerializeField] float closeRangeThreshold = 10f;
    private float actionCooldownTimer;

    [Header("Damage Settings")]
    [SerializeField] private float frontDamage = 1f;
    [SerializeField] private float sideDamage = 1.1f;
    [SerializeField] private float backDamage = 1.5f;

    [Header("UI and Effects")]
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private Image healthBar;
    [SerializeField] ParticleSystem teleportEffect;
    [SerializeField] ParticleSystem chargeEffect;

    private Animator animator;
    private NavMeshAgent agent;
    private Camera mainCamera;
    private bool isAttacking = false;
    private bool alreadyAttacked = false;

    private float playerDistance;
    private Vector3 playerDirection;

    [HideInInspector] public enum State
    {
        cooldown,
        makeDecision,
        doAction
    }
    public State currentState = State.cooldown;

    void Start()
    {
        bossHealth = bossMaxHealth;
        actionCooldownTimer = 5f;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        currentMoveSpeed = bossMoveSpeed;
        currentChargeSpeed = chargeSpeed;
        currentChargePrepareTime = chargePrepareTime;
        currentTeleportDelay = teleportDelay;

        if (healthBarCanvas != null) healthBarCanvas.enabled = false;
    }

    private void Update()
    {
        AdjustSpeedsBasedOnPlayerEnergy();
        DisplayHealthBarBasedOnPlayerEnergy();

        if (currentState == State.cooldown)
        {
            actionCooldownTimer -= Time.deltaTime;
            if (actionCooldownTimer <= 0) currentState = State.makeDecision;
        }
        else if (currentState == State.makeDecision)
        {
            MakeDecision();
            currentState = State.doAction;
        }
    }

    private void FixedUpdate()
    {
        GetDistanceDirection();
        SetAgentMovement();
        Debug.Log(teleportDelay);
    }

    private void AdjustSpeedsBasedOnPlayerEnergy()
    {
        bool isPlayerEnergyFull = player.GetComponent<Player>().IsEnergyFull();
        // currentMoveSpeed = isPlayerEnergyFull ? bossMoveSpeed / slowFactor : bossMoveSpeed;
        // currentChargeSpeed = isPlayerEnergyFull ? chargeSpeed / slowFactor : chargeSpeed;
        // currentChargePrepareTime = isPlayerEnergyFull ? chargePrepareTime / slowFactor : chargePrepareTime;
        // currentTeleportDelay = isPlayerEnergyFull ? teleportDelay * slowFactor : teleportDelay;
    }

    private void DisplayHealthBarBasedOnPlayerEnergy()
    {
        healthBarCanvas.enabled = player.GetComponent<Player>().IsEnergyFull();
        if (healthBarCanvas.enabled) AlignHealthBarWithCamera();
    }

    private void SetAgentMovement()
    {
        agent.destination = player.transform.position;
        agent.speed = currentMoveSpeed;
        if (agent.velocity.magnitude > 0.1f) animator.SetBool("Run", true);
        else animator.SetBool("Run", false);
    }

    private void GetDistanceDirection()
    {
        Vector3 playerXZ = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        Vector3 selfXZ = new Vector3(transform.position.x, 0, transform.position.z);
        playerDistance = Vector3.Distance(playerXZ, selfXZ);
        playerDirection = (playerXZ - selfXZ).normalized;
    }

    private void MakeDecision()
    {
        if (bossHealth >= (2.0f / 3.0f) * bossMaxHealth)
        {
            if (playerDistance > closeRangeThreshold) StartCoroutine(PrepareTeleport(1));
            else CloseRangeAttack();
        }
        else if (bossHealth >= (1.0f / 3.0f) * bossMaxHealth)
        {
            if (playerDistance > closeRangeThreshold) StartCoroutine(PrepareTeleport(2));
            else StartCoroutine(ChargeAttack(3));
        }
        else
        {
            StartCoroutine(ChargeAttack(3)); // More frequent charge attacks at low health
        }
    }

    private void ResetActionCooldown()
    {
        actionCooldownTimer = Random.Range(minActionCooldown, maxActionCooldown);
        currentState = State.cooldown;
    }

    private IEnumerator StopMoving(float stopTime)
    {
        float savedSpeed = currentMoveSpeed;
        currentMoveSpeed = 0;
        yield return new WaitForSeconds(stopTime);
        currentMoveSpeed = savedSpeed;
    }

    private IEnumerator PrepareTeleport(int attackType)
    {
        yield return new WaitForSeconds(currentTeleportDelay);
        StartCoroutine(TeleportAttack(attackType));
    }

    private IEnumerator TeleportAttack(int attackType)
    {
        Vector3 teleportPosition = player.transform.position + player.transform.forward * teleportDistance;
        transform.position = teleportPosition;

        Vector3 playerDirectionXZ = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        transform.forward = playerDirectionXZ - transform.position;

        if (attackType == 1)
        {
            yield return new WaitForSeconds(1f);
            CloseRangeAttack();
        }
        else if (attackType == 2)
        {
            StartCoroutine(ChargeAttack(3));
        }
    }

private IEnumerator ChargeAttack(int times)
{
    while (times > 0)
    {
        // Determine the direction and target position for the charge
        Vector3 playerDirectionXZ = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        transform.forward = playerDirectionXZ - transform.position;

        // Calculate the target position
        Vector3 targetPos = player.transform.position + playerDirection * chargeOffset;

        // Instantiate the boss shadow at the target position, facing the same direction as the boss
        GameObject bossShadow = Instantiate(bossShadowPrefab, targetPos, Quaternion.LookRotation(transform.forward));
        bossShadow.transform.localScale = transform.localScale * 0.8f; // Adjust size as necessary

        // Start charge preparation and add a longer pause
        yield return StartCoroutine(StopMoving(currentChargePrepareTime + 0.5f)); // Additional wait time

        // Start charge effect and move towards the target position
        chargeEffect.Play();
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, currentChargeSpeed * Time.deltaTime);
            yield return null;
        }
        chargeEffect.Stop(); // Stop effect once charge is complete

        // Destroy the boss shadow once the charge is complete
        Destroy(bossShadow);

        times--;

        // Pause slightly longer after each charge
        yield return new WaitForSeconds(1.0f); // Extra wait between charges
    }

    ResetActionCooldown();
}

    private void CloseRangeAttack()
    {
        animator.SetTrigger("MelleAttack");
        teleportEffect.Play();
        ResetActionCooldown();
    }

    private void AlignHealthBarWithCamera()
    {
        healthBarCanvas.transform.LookAt(mainCamera.transform);
        healthBarCanvas.transform.Rotate(0, 180, 0);
    }

    public void TakeDamage(float damageAmount)
    {
        float actualDamage = CalculateDamageBasedOnPosition() * damageAmount;
        bossHealth -= actualDamage;
        bossHealth = Mathf.Clamp(bossHealth, 0, bossMaxHealth);
        UpdateHealthBar();

        if (bossHealth <= 0) Die();
    }

    private float CalculateDamageBasedOnPosition()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);

        if (dotProduct > 0.5f) return frontDamage;
        else if (dotProduct < -0.7f) return backDamage;
        else return sideDamage;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null) healthBar.fillAmount = bossHealth / bossMaxHealth;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
