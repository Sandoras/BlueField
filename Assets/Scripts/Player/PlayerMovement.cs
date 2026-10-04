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

    [SerializeField] private bool _isOnGround;
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

        #endregion

        #region COLLISION CHECKS
        bool previousOnGround = _isOnGround;
        if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
        {
            LastOnGroundTime = Data.coyoteTime;
            _isOnGround = true;
            // print(LastOnGroundTime);
            // print("On ground");
        }
        else
        {
            _isOnGround = false;
        }
        #endregion


        //LANDING FIRST FRAME CHECK
        if (!previousOnGround && _isOnGround)
        {
            Squash.DoSquash(SquashJumpAmount);
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

    //DASH METHODS

    private void StartDash()
    {
        IsDashing = true;
        _dashTimeLeft = Data.dashTime;
        LastPressedDashTime = 0;

        int dashDir = IsFacingRight ? 1 : -1;
        bool isRunning = Mathf.Abs(RB.linearVelocityX) > Data.runMaxSpeed * 0.9f;
        float startSpeed = isRunning ?
        Data.dashSpeed : Data.dashSpeed * RB.linearVelocityX * 0.1f;

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

        #region CALCULATE AccelRate
        float accelRate = Data.runAccelAmount;

        if (!_isOnGround)
            accelRate *= Data.accelInAir;

        //Decceleration
        float deccelRate = Data.runDeccelAmount;

        if (!_isOnGround)
            deccelRate *= Data.deccelInAir;
        #endregion
        if (inputX == 0)
        {
            accelRate = deccelRate;
        }
        //Bonus jump apex acceleration && hang time
        float previousVelocityY = RB.linearVelocityY;
        if (IsJumping && Mathf.Abs(RB.linearVelocityY) < Data.jumpHangTimeThreshold)
        {
            accelRate *= Data.jumpHangAccelerationMult;
            RB.AddForceY(previousVelocityY * Data.jumpHangFloatMult, ForceMode2D.Force);
            print("Treshold reached");
            //targetSpeed *= Data.jumpHangTargetSpeedMult;
        }



        #region CONSERVE MOMENTUM
        if (Data.doConserveMomentum && Mathf.Abs(RB.linearVelocity.x) > Data.runMaxSpeed
         && Mathf.Sign(RB.linearVelocityX) == inputX
         && inputX != 0 && !_isOnGround)
        {
            accelRate = 0;
        }
        #endregion

        float speedDiff = (inputX * Data.runMaxSpeed) - RB.linearVelocityX;

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
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
    }

    //JUMP & ABILITY CHECKS
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


    private bool IsRunning()
    {
        return Mathf.Abs(RB.linearVelocityX) >= 0.1f;
    }







}