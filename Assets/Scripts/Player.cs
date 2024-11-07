using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cinemachine.Utility;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject Cam;
    [SerializeField] private Transform enemy;
    private Rigidbody playerRig;
    private Animator animator; // Animator

    [Header("GameInput Script")]
    [SerializeField] private GameInput gameInput;

    // Player movement
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7.0f;
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float dashDuration = 0.2f;

    // Energy bar 
    [Header("Energy")]
    [SerializeField] private Image energyBar, energyBarExtra;

    [SerializeField] private float maxEnergy, dashGain, energyFill, extraEnergyPerDodge;

    [Header("Skill Orbs")]
    [SerializeField] private GameObject[] skillOrbPrefabs;
    private GameObject[] skillOrbInstances = new GameObject[4];
    [SerializeField] private Vector3[] orbOffsets = new Vector3[4];

    private SkillOrbs currentHoveredOrb;

    [Header("Player Damage Setting")]
    public float Damage;

    private float energy;
    private float dashCount;
    private float rotateSpeed = 10f;
    private bool isWalking;
    private bool isDashing;
    private bool isInBattle = false; // flag for battle state
    public bool isEnergyFull;

    [SerializeField] private bool isInNarrativeHall = true; // bool to check if in Narrative Hall

    private Color targetColor;
    private float colorChangeInterval = 0.5f;
    private float colorChangeTimer;

    private void Awake()
    {
        playerRig = GetComponent<Rigidbody>();
        energyBar.fillAmount = energy;
        energyBarExtra.fillAmount = 0;
        animator = GetComponent<Animator>();

        // Initialize the skill orbs
        orbOffsets[0] = new Vector3(0.95f, 1.5f, 0);
        orbOffsets[1] = new Vector3(0.95f, 1, 0);
        orbOffsets[2] = new Vector3(0.7f, 1.25f, 0);
        orbOffsets[3] = new Vector3(1.2f, 1.25f, 0);

        for (int i = 0; i < 4; i++)
        {
            Vector3 spawnPosition = transform.position + orbOffsets[i];
            skillOrbInstances[i] = Instantiate(skillOrbPrefabs[i], spawnPosition, Quaternion.identity);
            skillOrbInstances[i].transform.parent = transform;
            skillOrbInstances[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (isInBattle) // Only accumulate energy when in battle
        {
            energy += energyFill * Time.deltaTime;
            if (energy > maxEnergy)
            {
                energy = maxEnergy;
                energyBarExtra.fillAmount = dashCount * extraEnergyPerDodge / maxEnergy;
            }
            energyBar.fillAmount = energy / maxEnergy;

            UpdateEnergyBarExtraColor();
        }

        if (!IsEnergyFull())
        {
            if (isInNarrativeHall)
            {
                Cam.GetComponent<CameraSwitch>().ShowNarrativeView();
            }
            else
            {
                Cam.GetComponent<CameraSwitch>().ShowOverheadView();
            }

            if (gameInput.IsJumpButtonPressed() && !isDashing)
            {
                animator.SetTrigger("isDashing");
                StartCoroutine(DashCoroutine());
                dashCount++;
                energy += dashGain;
            }

            Vector2 inputVector = gameInput.GetMovementVectorNormalized();
            Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            if (moveDir != Vector3.zero)
            {
                transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
            }

            isWalking = moveDir != Vector3.zero;
            animator.SetBool("Running", isWalking);
        }
        else
        {
            LockOnEnemy();
            ShowSkillOrbs();
            HandleMouseHover();

            if (gameInput.IsNormalAttackButtonPressed() && !isDashing)
            {
                ApplyDamage();
                ResetEnergy();
            }

            if (gameInput.IsSkillButtonPressed() && currentHoveredOrb != null)
            {
                currentHoveredOrb.ActivateSkill();
                ResetEnergy();
            }
        }
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        RaycastHit hit;
        Vector3 startPosition = transform.position;
        Vector3 destination = transform.position + transform.forward * distance;

        if (Physics.Linecast(transform.position, destination, out hit))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                destination = transform.position + transform.forward * (hit.distance - 1f);
            }
        }

        if (Physics.Raycast(destination, -Vector3.up, out hit))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                destination = hit.point;
            }
        }

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            transform.position = Vector3.Lerp(startPosition, destination, elapsed / dashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = destination;
        isDashing = false;
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    public bool IsEnergyFull()
    {
        isEnergyFull = energy >= maxEnergy;
        return isEnergyFull;
    }

    private void UpdateEnergyBarExtraColor()
    {
        colorChangeTimer += Time.deltaTime;

        if (colorChangeTimer >= colorChangeInterval)
        {
            targetColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            colorChangeTimer = 0f;
        }

        energyBarExtra.color = Color.Lerp(energyBarExtra.color, targetColor, Time.deltaTime * 2.0f);
    }

    private void ResetEnergy()
    {
        energy = 0;
        dashCount = 0;
        energyBarExtra.fillAmount = 0;
        HideSkillOrbs();
    }

    private void LockOnEnemy()
    {
        Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToEnemy.x, 0, directionToEnemy.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
        Cam.GetComponent<CameraSwitch>().ShowOvershoulderView();
    }

    private void ShowSkillOrbs()
    {
        foreach (GameObject orb in skillOrbInstances)
        {
            if (orb != null)
            {
                orb.SetActive(true);
            }
        }
    }

    private void HideSkillOrbs()
    {
        foreach (GameObject orb in skillOrbInstances)
        {
            if (orb != null)
            {
                orb.SetActive(false);
            }
        }
    }

    private void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            SkillOrbs skillOrb = hit.collider.GetComponent<SkillOrbs>();
            if (skillOrb != null)
            {
                if (currentHoveredOrb != skillOrb)
                {
                    if (currentHoveredOrb != null)
                    {
                        currentHoveredOrb.OnHover(false);
                    }

                    currentHoveredOrb = skillOrb;
                    currentHoveredOrb.OnHover(true);
                }
            }
            else if (currentHoveredOrb != null)
            {
                currentHoveredOrb.OnHover(false);
                currentHoveredOrb = null;
            }
        }
        else if (currentHoveredOrb != null)
        {
            currentHoveredOrb.OnHover(false);
            currentHoveredOrb = null;
        }
    }

    public void ApplyDamage()
    {
        enemy.GetComponent<Boss>().TakeDamage(Damage);
    }

    // Call this method to start energy accumulation when the player enters battle
    public void StartBattle()
    {
        isInBattle = true;
    }

    // Call this method to stop energy accumulation when the player exits battle
    public void EndBattle()
    {
        isInBattle = false;
        ResetEnergy();
    }

    public void SetNarrativeHallMode(bool inNarrativeHall)
    {
        isInNarrativeHall = inNarrativeHall;
    }
}



// using UnityEditor.Experimental.GraphView;
// using UnityEngine;
// using System.Collections; // Required for IEnumerator if coroutines are used

// public class Player : MonoBehaviour
// {
//     private Rigidbody playerRig;
//     [SerializeField] private GameInput gameInput;

//     [Header("Movement")]
//     [SerializeField] private float moveSpeed = 7.0f;
//     [SerializeField] private float distance = 2.0f;
//     [SerializeField] private float dashDuration = 0.2f; // Keeping dash duration for timing purposes

//     private float rotateSpeed = 10f;
//     private bool isWalking;
//     private bool isDashing; // To ensure dash is executed only once

//     private void Awake()
//     {
//         playerRig = GetComponent<Rigidbody>();
//     }

//     private void Update()
//     {
//         // Check for dash input, only dash if not already dashing
//         if (gameInput.IsJumpButtonPressed() && !isDashing)
//         {
//             Debug.Log("dash");
//             BlinkDash(); // Perform instant blink (teleport)
//         }

//         // Get the movement vector from GameInput script
//         Vector2 inputVector = gameInput.GetMovementVectorNormalized();

//         // Move the player with the moveSpeed value
//         Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
//         transform.position += moveDir * moveSpeed * Time.deltaTime;

//         // Smooth rotation towards movement direction
//         if (moveDir != Vector3.zero)
//         {
//             transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
//         }

//         isWalking = moveDir != Vector3.zero;
//     }

//     // Instant Blink Dash (Teleport)
//     private void BlinkDash()
//     {
//         isDashing = true;

//         RaycastHit hit;
//         Vector3 destination = transform.position + transform.forward * distance;

//         // Check if there's an obstacle in the path
//         if (Physics.Linecast(transform.position, destination, out hit))
//         {
//             // Adjust destination to stop just before the obstacle
//             destination = transform.position + transform.forward * (hit.distance - 1f);
//         }

//         // Ensure the destination is grounded
//         if (Physics.Raycast(destination, -Vector3.up, out hit))
//         {
//             destination = hit.point;
//         }

//         // Instantly move the player to the destination (blink/teleport)
//         transform.position = destination;

//         // Invoke reset function after a short delay to allow for cooldown
//         Invoke(nameof(ResetDash), dashDuration);
//     }

//     private void ResetDash()
//     {
//         isDashing = false;
//     }

//     public bool IsWalking()
//     {
//         return isWalking;
//     }
// }
