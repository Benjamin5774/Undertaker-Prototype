using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject player;

    [Header("Basic")]
    [SerializeField] float bossMaxHealth = 5f;
    [SerializeField] float bossMoveSpeed = 5f;
    [SerializeField] float bossHealth;

    [Header("Teleport")]
    [SerializeField] float teleportDistance = 3f; // ���ֵľ���
    [SerializeField] float teleportDelay = 2f; // ����ǰ�ĵ���ʱ

    [Header("ChargeAttack")]
    [SerializeField] float chargePrepareTime = 1f;// ����ǰҡ
    [SerializeField] float chargeSpeed = 25f;// �����ٶ�
    [SerializeField] float chargeOffset = 3f;

    [Header("Action")]
    [SerializeField] float minActionCooldown = 5f;
    [SerializeField] float maxActionCooldown = 10f;
    [SerializeField] float closeRangeThreshold = 10f;
    private float actionCooldownTimer;

    [HideInInspector] public enum State
    {
        cooldown,
        makeDesicion,
        doAction
    }
    public State currentState = State.cooldown;

    float playerDistance;
    Vector3 playerDirection;
    private NavMeshAgent agent;

    void Start()
    {
        bossHealth = bossMaxHealth;
        actionCooldownTimer = 5f;

        agent = GetComponent<NavMeshAgent>();
    }

    private void FixedUpdate()
    {
        GetDistanceDirection();
        SetAgentMovement();

        switch (currentState)
        {
            case State.cooldown:
                actionCooldownTimer -= Time.deltaTime;
                actionCooldownTimer = Mathf.Clamp(actionCooldownTimer, 0, Mathf.Infinity);
                if (actionCooldownTimer <= 0)
                {
                    currentState = State.makeDesicion;
                }
                break;
            case State.makeDesicion:
                MakeDecision();
                currentState = State.doAction;
                break;
            case State.doAction:
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            bossHealth = 50.5f;
    }

    void SetAgentMovement()
    {
        agent.destination = player.transform.position;
        agent.speed = bossMoveSpeed;
    }

    void GetDistanceDirection()
    {
        Vector3 playerXZ = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        Vector3 selfXZ = new Vector3(this.transform.position.x, 0, this.transform.position.z);
        playerDistance = Vector3.Distance(playerXZ, selfXZ);
        playerDirection = (playerXZ - selfXZ).normalized;
    }

    void MakeDecision()
    {
        if ((bossHealth / bossMaxHealth) >= 2.0f/3.0f)
        {
            if (playerDistance > closeRangeThreshold)
                StartCoroutine(PrepareTeleport(1));
            else
                CloseRangeAttack();
        }
        else if ((bossHealth / bossMaxHealth) < 2.0f/3.0f && (bossHealth / bossMaxHealth) >= 1.0f/3.0f)
        {
            if (playerDistance > closeRangeThreshold)
                StartCoroutine(PrepareTeleport(2));
            else
                StartCoroutine(ChargeAttack(3));
        }
    }

    void ResetActionCooldown()
    {
        float randomCooldown = Random.Range(minActionCooldown, maxActionCooldown);
        actionCooldownTimer = randomCooldown;
        currentState = State.cooldown;
    }

    IEnumerator StopMoving(float stopTime)
    {
        float savedSpeed = bossMoveSpeed;
        bossMoveSpeed = 0;
        yield return new WaitForSeconds(stopTime);
        bossMoveSpeed = savedSpeed;
    }

    IEnumerator PrepareTeleport(int attackType)
    {
        Debug.Log($"Teleport in {teleportDelay} seconds.");
        yield return new WaitForSeconds(teleportDelay);
        StartCoroutine(TeleportAttack(attackType));
    }

    // 1: CloseRangeAttack
    // 2: TripleChargeAttack
    //���ֹ���
    IEnumerator TeleportAttack(int attackType)
    {
        Vector3 originalPosition = transform.position; 
        Vector3 teleportPosition = player.transform.position + player.transform.forward * teleportDistance; // ��������λ��
        transform.position = teleportPosition; 
        
        // Boss �泯���
        Vector3 playerDirectionXZ = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z); 
        //transform.LookAt(playerDirectionXZ); 
        transform.forward = playerDirectionXZ - transform.position;// Boss ǿ���������

        yield return new WaitForSeconds(1.5f); // ͣ��1.5�루�ͷŹ���������ʱ�䣩
        //transform.position = originalPosition; // ����ԭʼλ�ã�Ч�����ã���������ң�

        if (attackType == 1)
            CloseRangeAttack();
        if (attackType == 2)
            StartCoroutine(ChargeAttack(3));
        // ChasingPlayer(); // ����׷�����
        // ResetActionCooldown();
    }

    //���̹���
    IEnumerator ChargeAttack(int times)
    {
        while (times > 0)
        {
            // Boss �泯���
            Vector3 playerDirectionXZ = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z); 
            transform.forward = playerDirectionXZ - transform.position;

            Debug.Log("Charging...");
            StartCoroutine(StopMoving(chargePrepareTime));
            yield return new WaitForSeconds(chargePrepareTime);

            Vector3 targetPos = player.transform.position + playerDirection * chargeOffset;

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, chargeSpeed * Time.deltaTime);
                yield return null;
            }

            times--;
        }

        ResetActionCooldown();
    }

    void CloseRangeAttack()
    {
        Debug.Log("Close range attack");
        ResetActionCooldown();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Hit!");
        }
    }
}
