using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] GameObject player;

    [Header("Basic")]
    [SerializeField] float bossMaxHealth = 5f;
    [SerializeField] float bossMoveSpeed = 2.5f;
    [SerializeField] float rotateSmooth = 0.1f;
    float bossHealth;

    [Header("Teleport")]
    [SerializeField] float teleportDistance = 3f; 
    [SerializeField] float teleportDelay = 2f;

    [Header("Action")]
    [SerializeField] float minActionCooldown = 3f;
    [SerializeField] float maxActionCooldown = 4f;
    [SerializeField] float closeRangeThreshold = 1f;
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

    void Start()
    {
        bossHealth = bossMaxHealth;
        actionCooldownTimer = 5f;
    }

    private void FixedUpdate()
    {
        GetDistanceDirection();

        switch (currentState)
        {
            case State.cooldown:
                ChasingPlayer();
                LookingPlayer();
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

    void ChasingPlayer()
    {
        transform.position += playerDirection * Time.deltaTime * bossMoveSpeed;
    }

    void LookingPlayer()
    {
        transform.forward = Vector3.Lerp(transform.forward, playerDirection, rotateSmooth);
    }

    void GetDistanceDirection()
    {
        Vector3 playerXZ = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        Vector3 selfXZ = new Vector3(this.transform.position.x, 0, this.transform.position.z);
        playerDistance = Vector3.Distance(playerXZ, selfXZ);
        playerDirection = (playerXZ - selfXZ).normalized;
    }

    //根据距离选择招式
    void MakeDecision()
    {
        if (playerDistance <= closeRangeThreshold)
        {
            CloseRangeAttack();
        }
        else
        {
            StartCoroutine(PrepareTeleport());
        }
    }

    //测试用闪现攻击提示
    IEnumerator PrepareTeleport()
    {
        Debug.Log($"Teleport in {teleportDelay} seconds.");
        yield return new WaitForSeconds(teleportDelay);
        StartCoroutine(LongRangeAttack());
    }

    //闪现
    IEnumerator LongRangeAttack()
    {
        Vector3 originalPosition = transform.position;
        Vector3 teleportPosition = player.transform.position + player.transform.forward * teleportDistance; 
        transform.position = teleportPosition; 
        
 
        Vector3 playerDirectionXZ = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z); 
        transform.LookAt(playerDirectionXZ); 

        yield return new WaitForSeconds(1.5f); 
         //transform.position = originalPosition; 
        Debug.Log("Long range attack");

        ChasingPlayer(); 
        ResetActionCooldown();
    }

    //攻击间隙（CD）
    void ResetActionCooldown()
    {
        float randomCooldown = Random.Range(minActionCooldown, maxActionCooldown);
        actionCooldownTimer = randomCooldown;
        currentState = State.cooldown;
    }

    //近身攻击
    void CloseRangeAttack()
    {
        Debug.Log("Close range attack");
        Invoke("ResetActionCooldown", 5f);
    }
}
