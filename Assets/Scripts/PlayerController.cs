using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
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

    private PhotonView photonView;

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

    private AnimState animState;
    private float jumpTimeCounter;
    public bool isGrounded = false;
    private bool isJumping = false;
    private bool isAttacking = false;
    private bool isRolling = false;
    private bool isSneaking = false;

    public Rigidbody2D rigidBody;
    private float saveClientPlayerMoveX = 0;
    private float saveClientPlayerMoveY = 0;
    private float saveClientPlayerVelocityY = 0;
    private int saveClientDirection = 0;
    private float gravity = 30f;
    private float headDisplacementY = 0f;


    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        fovScript = fov.transform.GetComponent<FieldOfView>();
        
        if (!photonView.IsMine) { return; }

        Cursor.lockState = CursorLockMode.Confined;
        rigidBody = GetComponent<Rigidbody2D>();
        animator = model.transform.GetComponent<Animator>();
       
        camera.SetActive(true);
        groundCheck.SetActive(true);
        fov.SetActive(true);
        proximityLight.SetActive(true);
        audioListener.SetActive(true);
    }

    private void Update() 
    {
        if (!photonView.IsMine) {   
            return; 
        }

        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Movement logic
        ClientInput();
        ClientMove();
        ClientAnimate();

    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) { return; }

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
            return;
        }
        else
        {
            isAttacking = false;
            isRolling = false;
        }

        // Flashlight
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            photonView.RPC("SwitchOnOffFlashlightRpc", RpcTarget.All, flashlight.activeSelf);
            //flashlight.SetActive(!flashlight.activeSelf);
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
        InputRollAndAttack(playerMovingForward);

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
        if (Input.GetKeyDown(KeyCode.W) && isGrounded) // && !CheckAnimationUninterruptible()
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
                    animState = AnimState.Walk;
                }
                else
                {
                    saveClientPlayerMoveX += moveSpeed * slowDownMultiplier;
                    animState = AnimState.SneakWalk;
                }
            }
            else if (saveClientDirection == 1)  // facing left
            {
                playerMovingForward = -1;
                if (!isSneaking)
                {
                    saveClientPlayerMoveX += moveSpeed * slowDownMultiplier;
                    animState = AnimState.ReverseWalk;
                }
                else
                {
                    saveClientPlayerMoveX += moveSpeed * slowDownMultiplier;
                    animState = AnimState.SneakWalk;
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
                    animState = AnimState.Walk;
                }
                else
                {
                    saveClientPlayerMoveX -= moveSpeed * slowDownMultiplier;
                    animState = AnimState.SneakWalk;
                }
            }
            else if (saveClientDirection == -1) //facing right
            {
                playerMovingForward = -1;
                if (!isSneaking)
                {
                    saveClientPlayerMoveX -= moveSpeed * slowDownMultiplier;
                    animState = AnimState.ReverseWalk;
                }
                else
                {
                    saveClientPlayerMoveX -= moveSpeed * slowDownMultiplier;
                    animState = AnimState.SneakWalk;
                }
            }
        }
        else
        {
            if (!isSneaking)
            {
                animState = AnimState.Idle;
            }
            else
            {
                animState = AnimState.SneakIdle;
            }
        }

        return playerMovingForward;
    }

    private int GetForwardDirection()
    {
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
                animState = AnimState.Roll;
            }
            else if (playerMovingForward < 0)
            {
                animState = AnimState.ReverseRoll;
            }
            isAttacking = false;
        }
        // Attack
        else if (Input.GetMouseButton(0))
        {
            animState = AnimState.Attack;
            isRolling = false;
        }
    }


    private void ClientMove()
    {
       /* photonView.RPC("UpdateClientPositionAndDirectionServerRpc", RpcTarget.All, saveClientPlayerMoveX, saveClientPlayerMoveY, saveClientDirection);
        photonView.RPC("UpdateClientVelocityServerRpc", RpcTarget.All, saveClientPlayerVelocityY);*/
        transform.position = new Vector3(transform.position.x + saveClientPlayerMoveX * Time.deltaTime,
            transform.position.y + saveClientPlayerMoveY * Time.deltaTime,
            transform.position.z);

        model.transform.localScale = new Vector3(saveClientDirection, transform.localScale.y, transform.localScale.z);

        rigidBody.velocity = Vector2.up * saveClientPlayerVelocityY;
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


        if (animState == AnimState.Walk)
        {
            animator.Play("Walk");
            audioListener.GetComponent<AudioManager>().Play("Fast Walk");
        }
        else if (animState == AnimState.ReverseWalk)
        {
            animator.Play("ReverseWalk");
            audioListener.GetComponent<AudioManager>().Play("Slow Walk");
        }
        else if (animState == AnimState.SneakWalk)
        {
            animator.Play("SneakWalk");
            audioListener.GetComponent<AudioManager>().Play("Sneak Walk");
        }
        else if (animState == AnimState.Idle)
        {
            animator.Play("Idle");
            audioListener.GetComponent<AudioManager>().Pause();
        }
        else if (animState == AnimState.SneakIdle)
        {
            animator.Play("SneakIdle");
            audioListener.GetComponent<AudioManager>().Pause();
        }
        else if (animState == AnimState.Attack)
        {
            animator.Play("Attack");
        }
        else if (animState == AnimState.Roll)
        {
            animator.Play("Roll");
        }
        else if (animState == AnimState.ReverseRoll)
        {
            animator.Play("ReverseRoll");
        }
    }

    /* [PunRPC]
     public void UpdateClientPositionAndDirectionServerRpc(float saveClientPlayerMoveX, float saveClientPlayerMoveY, int saveClientDirection)
     {
         transform.position = new Vector3(transform.position.x + saveClientPlayerMoveX * Time.deltaTime,
             transform.position.y + saveClientPlayerMoveY * Time.deltaTime,
             transform.position.z);

         model.transform.localScale = new Vector3(saveClientDirection, transform.localScale.y, transform.localScale.z);
     }

     [PunRPC]
     public void UpdateClientVelocityServerRpc(float saveClientPlayerVelocityY)
     {
         *//*Debug.Log(photonView.Owner + "======================");
         Debug.Log(rigidBody);
         Debug.Log(saveClientPlayerVelocityY);*//*
         rigidBody.velocity = Vector3.up * saveClientPlayerVelocityY;
     }*/
    /*
        [PunRPC]
        public void UpdateClientAnimStateServerRpc(AnimState state)
        {
            networkPlayerAnimState.Value = state;
        }*/

    [PunRPC]
    public void SwitchOnOffFlashlightRpc(bool state)
    {
        flashlight.SetActive(!state);
    }

}
