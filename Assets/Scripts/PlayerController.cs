using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float crouchSpeed = 4f;
    public float sprintSpeed = 14f;
    public float sprintDuration = 3f;
    public float sprintDecayRate = 4f;
    public float sprintCooldown = 5f;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;


    public Transform orientation;

    private float currentSprintTime;
    private bool isSprinting;
    private bool isCrouching;
    private bool canSprint = true;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public float noiseRunRadius = 20f;
    public float noiseWalkRadius = 10f;
    public float noiseSilentRadius = 0f;
    public LayerMask enemyLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        currentSprintTime = sprintDuration;
    }

    private void MyInput()
    {

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Salto egiteko
        if(Input.GetKey(jumpKey) && readyToJump && grounded){
            readyToJump = false;
            // Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(Input.GetKey(sprintKey) && canSprint && grounded)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        if(Input.GetKey(crouchKey) && grounded)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Lurrean dagoen begiratu
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        SpeedControl();

        if(grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        bool isMoving = horizontalInput != 0 || verticalInput != 0;

        if (isMoving)
        {
            if (isSprinting && currentSprintTime > 0)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, Time.deltaTime * sprintDecayRate);
                currentSprintTime -= Time.deltaTime;

                EmitNoise(noiseRunRadius);

                if (currentSprintTime <= 0)
                {
                    StartCoroutine(SprintCooldown());
                }
            }
            else if (isCrouching)
            {
                moveSpeed = crouchSpeed;
                // EmitNoise(noiseSilentRadius); // Soinurik ez
            }
            else
            {
                moveSpeed = 7f;
                if (currentSprintTime < sprintDuration && !isSprinting)
                {
                    currentSprintTime += Time.deltaTime * (sprintDuration / sprintCooldown);
                }
                EmitNoise(noiseWalkRadius);
            }
        }
        else
        {
            moveSpeed = 7f;
        }
        

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        // Mugimendu abiadura kalkulatu
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if(!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Abiadura kontrolatu behar bada
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private IEnumerator SprintCooldown()
    {
        canSprint = false;
        yield return new WaitForSeconds(sprintCooldown);
        canSprint = true;
    }

    public void EmitNoise(float radius)
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        foreach(Collider enemy in enemiesInRange)
        {
            EnemyHearing enemyHearing = enemy.GetComponent<EnemyHearing>();

            if(enemyHearing != null) enemyHearing.OnNoiseHeard(transform.position);
        }
    }
}
