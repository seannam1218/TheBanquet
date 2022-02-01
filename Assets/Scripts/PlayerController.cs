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
    [SerializeField] GameObject leftCheck;
    [SerializeField] GameObject rightCheck;
    [SerializeField] GameObject topCheck;
    [SerializeField] GameObject model;
    [SerializeField] GameObject head;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject headReference;
    [SerializeField] GameObject fov;
    [SerializeField] GameObject proximityLight;
    [SerializeField] GameObject flashlight;
    [SerializeField] GameObject audioSource;
    
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

    public AnimState animState;
    private float jumpTimeCounter;
    public bool isGrounded = false;
    public bool isBlockedOnLeft = false;
    public bool isBlockedOnRight = false;
    public bool isBlockedOnTop = false;
    private bool isJumping = false;
    private bool isAttacking = false;
    private bool isRolling = false;
    private bool isSneaking = false;

    private Rigidbody2D rigidBody;
    private float saveClientPlayerMoveX = 0;
    private float saveClientPlayerMoveY = 0;
    private float saveClientPlayerVelocityY = 0;
    private int saveClientDirection = 0;
    private float gravity = 30f;
    private float headDisplacementY = 0f;


    void Start()
    {
        photonView = GetComponent<PhotonView>();
        animator = model.transform.GetComponent<Animator>();

        if (!photonView.IsMine) { return; }

        Cursor.lockState = CursorLockMode.Confined;
        rigidBody = GetComponent<Rigidbody2D>();
        fovScript = fov.transform.GetComponent<FieldOfView>();

        camera.SetActive(true);
        weapon.SetActive(false);
        groundCheck.SetActive(true);
        leftCheck.SetActive(true);
        rightCheck.SetActive(true);
        topCheck.SetActive(true);

        fov.SetActive(true);
        proximityLight.SetActive(true);
    }

    private void Update() 
    {

        if (!photonView.IsMine) return;
        
        mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Movement logic
        ClientInput();
        ClientMove();
        //ClientAnimate();

    }

    private void LateUpdate()
    {
        if (!photonView.IsMine) { return; }

        // rotate head
        float headAngle = Utils.GetAimAngle(head.transform.position, mouseWorldPos, true) * 0.6f;
        //head.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -headAngle));
        photonView.RPC("RotateHeadRpc", RpcTarget.All, headAngle);

        // rotate weapon
        float weaponAngle = Utils.GetAimAngle(weapon.transform.position, mouseWorldPos, true);
        weapon.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -weaponAngle));

        // Crouch & peak over
        Vector3 referencePos = head.transform.position;
        if (isSneaking)
        {
            headDisplacementY = Mathf.Clamp(headDisplacementY + Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime, -0.4f, 0.4f);
            referencePos = headReference.transform.position + new Vector3(0, headDisplacementY, 0);
            if (headDisplacementY <= 0)
            {
                head.transform.position = referencePos;
            }
            else
            {
                head.transform.position = headReference.transform.position;
                fovScript.SetOrigin(referencePos);
            }
        }
        else
        {
            head.transform.position = headReference.transform.position;
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
            photonView.RPC("ToggleFlashlightRpc", RpcTarget.All, flashlight.activeSelf);
        }

        // Sneak
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isSneaking = !isSneaking;
            animator.SetBool("Sneak", isSneaking);
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


        // Equipping
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            photonView.RPC("EquipWeaponRpc", RpcTarget.All, weapon.activeSelf);
        }

        // Roll and attack
        //InputRollAndAttack(playerMovingForward);

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
            if (jumpTimeCounter < 0)
            {
                isJumping = false;
                saveClientPlayerVelocityY = jumpForce / 2;
            }
            else if (isBlockedOnTop)
            {
                isJumping = false;
                saveClientPlayerVelocityY = 0;
            }
            else
            {
                saveClientPlayerMoveY = jumpForce;
                saveClientPlayerVelocityY = 0;
                jumpTimeCounter -= Time.deltaTime;
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
        int playerMovingDir = 0;

        // Movement towards right
        if (Input.GetKey(KeyCode.D) && !isBlockedOnRight)
        {
            playerMovingDir = 1;
            HandleMoveAndMoveSound(playerMovingDir, saveClientDirection);
        }
        // Movement towards left
        else if (Input.GetKey(KeyCode.A) && !isBlockedOnLeft)
        {
            playerMovingDir = -1;
            HandleMoveAndMoveSound(playerMovingDir, saveClientDirection);
        }
        else
        {
            audioSource.GetComponent<AudioManager>().Pause();
            if (!isSneaking)
            {
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkForward", false);
            }
            else
            {
                animator.SetBool("WalkBackward", false);
                animator.SetBool("WalkForward", false);
            }
        }

        return playerMovingDir * -saveClientDirection;
    }

    private void HandleMoveAndMoveSound(int movingDir, int facingDir)
    {
        int moveForward = movingDir * -facingDir;
        animator.SetBool("WalkForward", (moveForward == 1 ? true : false));
        animator.SetBool("WalkBackward", (moveForward == -1 ? true : false));
        float speed = (movingDir > 0 ? moveSpeed : -moveSpeed);
        if (isSneaking)
        {
            saveClientPlayerMoveX += speed * slowDownMultiplier;
            audioSource.GetComponent<AudioManager>().Play("Sneak Walk");
        }
        else if (movingDir != -facingDir) // if walking backward
        {
            saveClientPlayerMoveX += speed * slowDownMultiplier;
            audioSource.GetComponent<AudioManager>().Play("Slow Walk");
        }
        else // if walking forward
        {
            saveClientPlayerMoveX += speed;
            audioSource.GetComponent<AudioManager>().Play("Fast Walk");
        }
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

    /*private void InputRollAndAttack(int playerMovingForward)
    {
        if (Input.GetKey(KeyCode.S))
        {
            if (playerMovingForward > 0)
            {
                //animState = AnimState.Roll;
                photonView.RPC("SetAnimState", RpcTarget.AllBuffered, AnimState.Roll);
            }
            else if (playerMovingForward < 0)
            {
                //animState = AnimState.ReverseRoll;
                photonView.RPC("SetAnimState", RpcTarget.AllBuffered, AnimState.ReverseRoll);
            }
            isAttacking = false;
        }
        // Attack
        else if (Input.GetMouseButton(0))
        {
            //animState = AnimState.Attack;
            photonView.RPC("SetAnimState", RpcTarget.AllBuffered, AnimState.Attack);
            isRolling = false;
        }
    }*/


    private void ClientMove()
    {
        transform.position = new Vector3(transform.position.x + saveClientPlayerMoveX * Time.deltaTime,
           transform.position.y + saveClientPlayerMoveY * Time.deltaTime,
           transform.position.z);
       
        model.transform.localScale = new Vector3(saveClientDirection, transform.localScale.y, transform.localScale.z);

        rigidBody.velocity = Vector2.up * saveClientPlayerVelocityY;

        //photonView.RPC("UpdateClientPositionAndDirectionServerRpc", RpcTarget.All, saveClientPlayerMoveX, saveClientPlayerMoveY, saveClientDirection);
        //photonView.RPC("UpdateLagCompensationValuesRpc", RpcTarget.All, pv.ViewID, saveClientPlayerMoveX, saveClientPlayerMoveY, saveClientPlayerVelocityY);
    }


    /*private void ClientAnimate()
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
            audioSource.GetComponent<AudioManager>().Play("Fast Walk");
        }
        else if (animState == AnimState.ReverseWalk)
        {
            animator.Play("ReverseWalk");
            audioSource.GetComponent<AudioManager>().Play("Slow Walk");
        }
        else if (animState == AnimState.SneakWalk)
        {
            animator.Play("SneakWalk");
            audioSource.GetComponent<AudioManager>().Play("Sneak Walk");
        }
        else if (animState == AnimState.Idle)
        {
            animator.Play("Idle");
            audioSource.GetComponent<AudioManager>().Pause();
        }
        else if (animState == AnimState.SneakIdle)
        {
            animator.Play("SneakIdle");
            audioSource.GetComponent<AudioManager>().Pause();
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
    }*/

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
         transform.GetComponent<Rigidbody2D>().velocity = Vector2.up * saveClientPlayerVelocityY;
     }*/

    /*    [PunRPC]
        public void UpdateClientAnimStateServerRpc(AnimState state)
        {
            networkPlayerAnimState.Value = state;
        }*/

    /* [PunRPC]
     public void UpdateLagCompensationValuesRpc(int Id, float saveClientPlayerMoveX, float saveClientPlayerMoveY, float saveClientPlayerVelocityY)
     {
         PhotonView view = PhotonView.Find(Id);
         PhotonTransformViewClassic ptfv = view.gameObject.GetComponent<PhotonTransformViewClassic>();
         ptfv.m_PositionModel.InterpolateMoveTowardsSpeed = Mathf.Pow(Mathf.Pow(saveClientPlayerMoveX, 2f) + Mathf.Pow(saveClientPlayerMoveY, 2f), 1f);
     }*/

    [PunRPC]
    public void RotateHeadRpc(float angle)
    {
        head.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -angle));
    }

    [PunRPC]
    public void ToggleFlashlightRpc(bool state)
    {
        flashlight.SetActive(!state);
    }

    [PunRPC]
    public void EquipWeaponRpc(bool state)
    {
        weapon.SetActive(!state);
    }

}
