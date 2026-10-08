using UnityEngine;
using System.Collections;
using System;
using NUnit.Framework;

public class PlayerMovement : MonoBehaviour
{

    public PlayerData Data;

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    public BoxCollider2D BoxCollider;
    public Animator PlayerAnimator;
    public SquashSprite Squash;
    #endregion

    #region CHECK PARAMETERS
    // Set all of these up in the inspector
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);

    //DASH
    [SerializeField] private Transform _dashCheckPoint;
    [SerializeField] private Vector2 _dashCheckSize = new Vector2(0.49f, 0.03f);

    //CROUCH
    [SerializeField] private Transform _crouchCheckPoint;
    [SerializeField] private Vector2 _crouchCheckSize = new Vector2(0.49f, 0.03f);
    public bool IsCrouching = false;
    public bool tryingToStand = false;

    //CROUCH SETTINGS
    [SerializeField] private float _crouchingColliderHeight = 0.5f;
    private Vector2 _standingColliderSize;
    private Vector2 _standingColliderOffset;

    //GROUND AND WALL CHECK
    [SerializeField] private bool _isOnGround;
    [SerializeField] private bool _isOnWall;
    #endregion


    #region INPUT PARAMETERS
    private Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region STATE PARAMETERS
    // Can be read because its public but not written to from outside the this script
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
    //public bool IsFalling { get; private set; }

    //Timers
    public float LastOnGroundTime { get; private set; }
    [SerializeField] private float _dashTimeLeft;

    //Jump
    public float SquashJumpAmount = 0.1f;
    // private bool _isJumpCut;

    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        BoxCollider = GetComponent<BoxCollider2D>();

        // CROUCH SETUP
        _standingColliderSize = BoxCollider.size;
        _standingColliderOffset = BoxCollider.offset;

    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
        //Time.timeScale = 0.1f;

    }

    private void Update()
    {
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");


        if (_moveInput.x != 0 && !IsDashing)
            CheckDirectionToFace(_moveInput.x > 0);

        //JUMP INPUT
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpInput();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            JumpCut();
        }

        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            //_isJumpCut = false;
            Jump();
        }

        //DASH INPUT
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnDashInput();
            //print("leftshiftkeydown");
        }

        if (CanDash() && LastPressedDashTime > 0)
        {
            StartDash();
        }

        if (IsDashing)
        {
            _dashTimeLeft -= Time.deltaTime;
            print(_dashTimeLeft);

            if (_dashTimeLeft <= 0 || !Input.GetKey(KeyCode.LeftShift))
            {
                print(_dashTimeLeft);
                EndDash();
                print("Ending Dash");
            }

        }

        //CROUCH INPUT
        if (Input.GetKeyDown(KeyCode.S))
        {
            IsCrouching = true;
        }

        else if (Input.GetKeyUp(KeyCode.S) && IsCrouching)
        {
            tryingToStand = true;
        }

        if (tryingToStand)
        {
            if (!Physics2D.OverlapBox(_crouchCheckPoint.position, _crouchCheckSize, 0, _groundLayer))
            {
                IsCrouching = false;
                tryingToStand = false;
            }
        }
        #endregion

        #region COLLISION CHECKS
        bool previousOnGround = _isOnGround;
        if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
        {
            LastOnGroundTime = Data.coyoteTime;
            _isOnGround = true;
            // print(LastOnGroundTime);
            //print(_isOnGround);
        }
        else
        {
            _isOnGround = false;
            //print(_isOnGround);
        }

        bool previousHitWall = _isOnWall;
        if (Physics2D.OverlapBox(_dashCheckPoint.position, _dashCheckSize, 0, _groundLayer))
        {
            _isOnWall = true;
        }
        else
        {
            _isOnWall = false;
        }
        #endregion


        //FIRST FRAME CHECKS

        //LANDING
        if (!previousOnGround && _isOnGround)
        {
            Squash.DoSquash(SquashJumpAmount);
        }

        //WALL HIT
        if (!previousHitWall && _isOnWall && IsDashing)
        {
            Collider2D hitCollider = Physics2D.OverlapBox(_dashCheckPoint.position, _dashCheckSize, 0, _groundLayer);
            Squash.DoSquash(SquashJumpAmount * 2);
            if (hitCollider != null)
            {

                hitCollider.GetComponent<Breakable>().Break();
            }
        }
        if (IsJumping && RB.linearVelocityY < 0)
        {
            IsJumping = false;
            SetGravityScale(Data.gravityScale);

            //isJumpFalling = true;
        }

        {
            SetGravityScale(Data.gravityScale);
        }

        //ANIMATOR
        PlayerAnimator.SetFloat("f_xSpeed", Mathf.Abs(RB.linearVelocityX));
        PlayerAnimator.SetFloat("f_ySpeed", RB.linearVelocityY);
        PlayerAnimator.SetBool("b_IsOnGround", _isOnGround);
        PlayerAnimator.SetBool("b_IsFalling", IsFalling());
        PlayerAnimator.SetBool("b_IsCrouching", IsCrouching);
        PlayerAnimator.SetBool("b_IsDashing", IsDashing);

    }

    private void FixedUpdate()
    {
        if (IsDashing)
        {
            int dashDir;
            dashDir = IsFacingRight ? 1 : -1;
            // float dashStartSpeed = Mathf.Abs(RB.linearVelocityX) > Data.runMaxSpeed / 2
            //  ? Data.runMaxSpeed : Data.runMaxSpeed / 2;
            // print(dashDir);
            RB.linearVelocityX = Mathf.MoveTowards(
                RB.linearVelocityX,
                dashDir * Data.dashSpeed,
                Data.dashAccel * Time.fixedDeltaTime);

            return;

        }

        Run();
        Crouch();
    }

    public void OnJumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    private void OnDashInput()
    {

        LastPressedDashTime = Data.dashInputBufferTime;
        _dashTimeLeft = Data.dashTime;

        //print(LastPressedDashTime);

    }

    public void JumpCut()
    {
        if (!CanJumpCut())
            return;
        //   _isJumpCut = true;

        RB.linearVelocityY /= Data.JumpCutGravity;
        print("CanJumpCut");
    }


    //MOVEMENT METHODS

    //JUMP METHOD
    private void Jump()
    {
        RB.linearVelocityY = Mathf.Sqrt(2 * 9.81f * RB.gravityScale * Data.jumpForce);

        //Ensures we cant jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0f;

        //Perform jump
        float force = Data.jumpForce;

        // Increase the force applied if we are falling
        if (RB.linearVelocityY < 0)
        {
            force -= RB.linearVelocityY;
        }

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        //ANIMATOR
        PlayerAnimator.SetTrigger("t_Jump");
        Squash.DoSquash(SquashJumpAmount);

    }

    private void Crouch()
    {
        Vector2 colliderSize = _standingColliderSize;
        Vector2 colliderOffset = _standingColliderOffset;


        if (IsCrouching)
        {
            colliderSize.y *= 0.5f;
            colliderOffset.y = -0.48f;
        }
        //print(IsCrouching());
        BoxCollider.size = colliderSize;
        BoxCollider.offset = colliderOffset;
    }

    //DASH METHODS

    private void StartDash()
    {
        IsDashing = true;
        _dashTimeLeft = Data.dashTime;
        LastPressedDashTime = 0;

        int dashDir = IsFacingRight ? 1 : -1;
        bool isRunning = Mathf.Abs(RB.linearVelocityX) > Data.runMaxSpeed * 0.9f;
        float startSpeed = isRunning ?
        dashDir * Data.dashSpeed : dashDir * Data.dashSpeed * RB.linearVelocityX * 0.1f;

        RB.linearVelocityX = dashDir * startSpeed;

        print("Started Dashing");
        print(_dashTimeLeft);
        print(dashDir);

    }

    private void EndDash()
    {
        IsDashing = false;
        _dashTimeLeft = 0;

    }

    private bool CanDash()
    {
        return !IsDashing && _isOnGround && !IsJumping;

    }

    // RUN METHOD
    private void Run()
    {
        //float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        int inputX = Mathf.RoundToInt(_moveInput.x);

        float moveMaxSpeed = IsCrouching ? Data.slowedMaxSpeed : Data.runMaxSpeed;

        #region CALCULATE AccelRate
        float accelRate = Data.runAccelAmount;

        if (!_isOnGround)
            accelRate *= Data.accelInAir;

        //Decceleration
        float deccelRate = Data.runDeccelAmount;

        if (!_isOnGround)
            deccelRate *= Data.deccelInAir;
        #endregion

        //If no input is held accelRate is now deccelRate
        if (inputX == 0)
        {
            accelRate = deccelRate;
        }

        //Bonus jump hang time
        float previousVelocityY = RB.linearVelocityY;
        if (IsJumping && Mathf.Abs(RB.linearVelocityY) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            RB.AddForceY(previousVelocityY * Data.jumpHangFloatMult, ForceMode2D.Force);
            print("Treshold reached");
            //targetSpeed *= Data.jumpHangTargetSpeedMult;
        }



        #region CONSERVE MOMENTUM
        if (Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > moveMaxSpeed
         && Mathf.Sign(RB.linearVelocityX) == inputX
         && inputX != 0 && !_isOnGround)
        {
            accelRate = 0;
        }
        #endregion

        float speedDiff = (inputX * moveMaxSpeed) - RB.linearVelocityX;

        float movement = speedDiff * accelRate;

        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);



    }

    // CHECK METHODS

    //TURN
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }
    private void Turn()
    {
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }

    //GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.DrawWireCube(_dashCheckPoint.position, _dashCheckSize);
        Gizmos.DrawWireCube(_crouchCheckPoint.position, _crouchCheckSize);

    }

    //JUMP, ABILITY & MOVEMENT CHECKS
    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.linearVelocityY > 0;

    }

    private bool IsFalling()
    {
        bool isFalling = !_isOnGround && RB.linearVelocityY < -0.1f;
        return isFalling;
    }

    // private bool IsCrouching()
    // {
    //     bool isCrouching = _moveInput.y < 0;
    //     bool canStand = true;
    //     if (Physics2D.OverlapBox(_crouchCheckPoint.position, _crouchCheckSize, 0, _groundLayer) && isCrouching)
    //     {
    //         canStand = false;

    //     }
    //     //print(_moveInput.y);
    //     if (isCrouching)
    //         return isCrouching;


    //     else
    //         return isCrouching && canStand;
    // }


    private bool IsRunning()
    {
        return Mathf.Abs(RB.linearVelocityX) >= 0.1f;
    }







}