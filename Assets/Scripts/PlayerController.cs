using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public enum AnimState
    {
        Idle,
        SneakIdle,
        Walk,
        ReverseWalk,
        SneakWalk,
        Attack,
        Roll,
        ReverseRoll
    }


    [SerializeField] GameObject camera;
    [SerializeField] GameObject groundCheck;
    [SerializeField] GameObject model;
    [SerializeField] GameObject head;
    [SerializeField] GameObject fov;
    [SerializeField] GameObject proximityLight;
    [SerializeField] GameObject flashlight;
    [SerializeField] GameObject audioListener;
    FieldOfView fovScript;
    private Animator animator;
    private Vector3 mousePos;
    private Vector3 mouseWorldPos;

    [Header("[Settings]")]
    [SerializeField] float moveSpeed;
    [SerializeField] float scrollSpeed;
    [SerializeField] float slowDownMultiplier;
    [SerializeField] float jumpForce;
    [SerializeField] float jumpTime;
    
    [SerializeField] private float jumpTimeCounter;
    [SerializeField] public bool isGrounded = false;
    [SerializeField] bool isJumping = false;
    [SerializeField] bool isAttacking = false;
    [SerializeField] bool isRolling = false;
    [SerializeField] bool isSneaking = false;

    [SerializeField] private NetworkVariable<float> networkPlayerMoveX = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> networkPlayerMoveY = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<float> networkPlayerVelocityY = new NetworkVariable<float>();
    [SerializeField] private NetworkVariable<int> networkPlayerDirection = new NetworkVariable<int>();
    [SerializeField] private NetworkVariable<AnimState> networkPlayerAnimState = new NetworkVariable<AnimState>();

    Rigidbody2D rigidBody;
    private float saveClientPlayerMoveX = 0;
    private float saveClientPlayerMoveY = 0;
    private float saveClientPlayerVelocityY = 0;
    private int saveClientDirection = 0;
    private float gravity = 30f;
    private float headDisplacementY = 0f;


    // Start is called before the first frame update
    void Start()
    {
        rigidBody = transform.GetComponent<Rigidbody2D>();
        animator = model.transform.GetComponent<Animator>();
        fovScript = fov.transform.GetComponent<FieldOfView>();
        
        Cursor.lockState = CursorLockMode.Confined;

        if (IsClient && IsOwner)
        {
            camera.SetActive(true);
            groundCheck.SetActive(true);
            fov.SetActive(true);
            proximityLight.SetActive(true);
            audioListener.SetActive(true);
        }
    }

    private void Update() {

        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Movement logic
        if (IsClient && IsOwner)
        {
            ClientInput();
        }
        ClientMove();
        ClientAnimate();
    }

    private void LateUpdate()
    {
        // rotate head
        float angle = Utils.GetAimAngle(head.transform.position, mouseWorldPos, true) * 0.6f;
        head.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle));

        Vector3 referencePos = head.transform.position;
        if (isSneaking)
        {
            headDisplacementY = Mathf.Clamp(headDisplacementY + Input.GetAxis("Mouse ScrollWheel") * scrollSpeed, -0.4f, 0.4f);
            
            if (headDisplacementY <= 0)
            {
                head.transform.position += new Vector3(0, headDisplacementY, 0);
                referencePos = head.transform.position;
            }
            else
            {
                referencePos = head.transform.position + new Vector3(0, headDisplacementY, 0);
                fov.GetComponent<FieldOfView>().SetOrigin(referencePos);
            }
        }

        // FOV
        fovScript.SetOrigin(referencePos);
        fovScript.SetAimAngle(referencePos, mouseWorldPos);

        // flashlight
        flashlight.transform.position = referencePos;
    }

    private void ClientInput()
    {
        saveClientPlayerMoveY = 0;

        // Gravity
        if (isGrounded)
        {
            saveClientPlayerVelocityY = 0;
        }
        else
        {
            saveClientPlayerVelocityY -= gravity * Time.deltaTime;
        }

        // Direction
        saveClientDirection = GetForwardDirection();

        // Jump
        InputJump();

        // If in the middle of an uninterruptable animation, don't receive any inputs from the player.
        if (CheckAnimationUninterruptible())
        {
            SetActionStatusVariables();
            UpdateClientVelocityServerRpc(saveClientPlayerVelocityY);
            UpdateClientPositionAndDirectionServerRpc(saveClientPlayerMoveX, saveClientPlayerMoveY, saveClientDirection);
            return;
        }
        else
        {
            isAttacking = false;
            isRolling = false;
        }

        // Flashlight
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            if (flashlight.activeSelf)
            {
                flashlight.SetActive(false);
            } 
            else
            {
                flashlight.SetActive(true);
            }                
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isSneaking = !isSneaking;
        }

        // Dropping from one way platforms
        if (isGrounded && gameObject.layer != 7)
        {
            gameObject.layer = 7;
        }
        if (Input.GetKey(KeyCode.S))
        {
            gameObject.layer = 6;
            isGrounded = false;
        }


        // Movement
        int playerMovingForward = InputMovement(saveClientDirection);

        // Roll and attack
        //InputRollAndAttack(playerMovingForward);

        UpdateClientPositionAndDirectionServerRpc(saveClientPlayerMoveX, saveClientPlayerMoveY, saveClientDirection);
        UpdateClientVelocityServerRpc(saveClientPlayerVelocityY);
    }


    private bool CheckAnimationUninterruptible()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("Roll") || 
            animator.GetCurrentAnimatorStateInfo(0).IsName("ReverseRoll"))
        {
            return true;
        }
        return false;
    }

    private void SetActionStatusVariables()
    {
        isAttacking = false;
        isRolling = false;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            isAttacking = true;
            return;
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Roll") || animator.GetCurrentAnimatorStateInfo(0).IsName("ReverseRoll"))
        {
            isRolling = true;
            return;
        }
        return;
    }

    private void InputJump()
    {
        if (Input.GetKeyDown(KeyCode.W) && isGrounded && !CheckAnimationUninterruptible())
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            saveClientPlayerMoveY = jumpForce;
        }
        if (Input.GetKey(KeyCode.W) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                saveClientPlayerMoveY = jumpForce;
                saveClientPlayerVelocityY = 0;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
                saveClientPlayerVelocityY = jumpForce / 2;
            }
        }
        if (Input.GetKeyUp(KeyCode.W) && isJumping)
        {
            isJumping = false;
            saveClientPlayerVelocityY = jumpForce / 2;
        }
    }

    private int InputMovement(int saveClientDirection)
    {
        saveClientPlayerMoveX = 0;
        int playerMovingForward = 0;

        // Movement
        if (Input.GetKey(KeyCode.D))
        {
            if (saveClientDirection == -1)  //facing right
            {
                playerMovingForward = 1;
                if (!isSneaking)
                {
                    saveClientPlayerMoveX += moveSpeed;
                    UpdateClientAnimStateServerRpc(AnimState.Walk);
                }
                else
                {
                    saveClientPlayerMoveX += moveSpeed * slowDownMultiplier;
                    UpdateClientAnimStateServerRpc(AnimState.SneakWalk);
                }
            }
            else if (saveClientDirection == 1)  // facing left
            {
                playerMovingForward = -1;
                if (!isSneaking)
                {
                    saveClientPlayerMoveX += moveSpeed * slowDownMultiplier;
                    UpdateClientAnimStateServerRpc(AnimState.ReverseWalk);
                }
                else
                {
                    saveClientPlayerMoveX += moveSpeed * slowDownMultiplier;
                    UpdateClientAnimStateServerRpc(AnimState.SneakWalk);
                }
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            if (saveClientDirection == 1)   // facing left
            {
                playerMovingForward = 1;
                if (!isSneaking)
                {
                    saveClientPlayerMoveX -= moveSpeed;
                    UpdateClientAnimStateServerRpc(AnimState.Walk);
                }
                else
                {
                    saveClientPlayerMoveX -= moveSpeed * slowDownMultiplier;
                    UpdateClientAnimStateServerRpc(AnimState.SneakWalk);
                }
            }
            else if (saveClientDirection == -1) //facing right
            {
                playerMovingForward = -1;
                if (!isSneaking)
                {
                    saveClientPlayerMoveX -= moveSpeed * slowDownMultiplier;
                    UpdateClientAnimStateServerRpc(AnimState.ReverseWalk);
                }
                else
                {
                    saveClientPlayerMoveX -= moveSpeed * slowDownMultiplier;
                    UpdateClientAnimStateServerRpc(AnimState.SneakWalk);
                }
            }
        }
        else
        {
            if (!isSneaking)
            {
                UpdateClientAnimStateServerRpc(AnimState.Idle);
            }
            else
            {
                UpdateClientAnimStateServerRpc(AnimState.SneakIdle);
            }
        }

        return playerMovingForward;
    }

    private int GetForwardDirection()
    {
        //if (mousePos.x / Screen.width < 0.5f)
        if (mouseWorldPos.x < transform.position.x)
        {
            saveClientDirection = 1;    // facing left
        }
        else
        {
            saveClientDirection = -1;   //facing right
        }
        return saveClientDirection;
    }

    private void InputRollAndAttack(int playerMovingForward)
    {
        if (Input.GetKey(KeyCode.S))
        {
            if (playerMovingForward > 0)
            {
                UpdateClientAnimStateServerRpc(AnimState.Roll);
            }
            else if (playerMovingForward < 0)
            {
                UpdateClientAnimStateServerRpc(AnimState.ReverseRoll);
            }
            isAttacking = false;
        }
        // Attack
        else if (Input.GetMouseButton(0))
        {
            UpdateClientAnimStateServerRpc(AnimState.Attack);
            isRolling = false;
        }
    }


    private void ClientMove()
    {
        transform.position = new Vector3(transform.position.x + networkPlayerMoveX.Value*Time.deltaTime,
            transform.position.y + networkPlayerMoveY.Value*Time.deltaTime,
            transform.position.z);

        model.transform.localScale = new Vector3(networkPlayerDirection.Value, transform.localScale.y, transform.localScale.z);

        rigidBody.velocity = Vector2.up * networkPlayerVelocityY.Value;
    }


    private void ClientAnimate()
    {
        if (isAttacking)
        {
            return;
        }
        else if (isRolling)
        {
            return;
        }
        else
        {
            // Reset physics layers.

        }


        if (networkPlayerAnimState.Value == AnimState.Walk)
        {
            animator.Play("Walk");
            audioListener.GetComponent<AudioManager>().Play("Fast Walk");
        }
        else if (networkPlayerAnimState.Value == AnimState.ReverseWalk)
        {
            animator.Play("ReverseWalk");
            audioListener.GetComponent<AudioManager>().Play("Slow Walk");
        }
        else if (networkPlayerAnimState.Value == AnimState.SneakWalk)
        {
            animator.Play("SneakWalk");
            audioListener.GetComponent<AudioManager>().Play("Sneak Walk");
        }
        else if (networkPlayerAnimState.Value == AnimState.Idle)
        {
            animator.Play("Idle");
            audioListener.GetComponent<AudioManager>().Pause();
        }
        else if (networkPlayerAnimState.Value == AnimState.SneakIdle)
        {
            animator.Play("SneakIdle");
            audioListener.GetComponent<AudioManager>().Pause();
        }
       /* else if (networkPlayerAnimState.Value == AnimState.Attack)
        {
            animator.Play("Attack");
        }
        else if (networkPlayerAnimState.Value == AnimState.Roll)
        {
            animator.Play("Roll");
        }
        else if (networkPlayerAnimState.Value == AnimState.ReverseRoll)
        {
            animator.Play("ReverseRoll");
        }*/
    }

    [ServerRpc]
    public void UpdateClientPositionAndDirectionServerRpc(float clientPlayerMoveX, float clientPlayerMoveY, int clientDirection)
    {
        networkPlayerMoveX.Value = clientPlayerMoveX;
        networkPlayerMoveY.Value = clientPlayerMoveY;
        networkPlayerDirection.Value = clientDirection;
    }

    [ServerRpc]
    public void UpdateClientVelocityServerRpc(float clientPlayerVelocityY)
    {
        networkPlayerVelocityY.Value = clientPlayerVelocityY;
    }

    [ServerRpc]
    public void UpdateClientAnimStateServerRpc(AnimState state)
    {
        networkPlayerAnimState.Value = state;
    }
}
