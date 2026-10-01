using UnityEngine;


[CreateAssetMenu(menuName = "Player Run Data")]


public class PlayerData : ScriptableObject
{
    [Header("Run")]

    public float runMaxSpeed; // Target max speed
    public float runAcceleration; //Time (approx.) we want it to take for the player to accelerate from 0 to the runMaxSpeed

    [HideInInspector] public float runAccelAmount;  // The amplitude applied to the player. Multiplied with speedDiff.
                                                    // speedDiff is (targetSpeed - RB.velocity.x;) Its the difference between the current player speed and the...
                                                    // desired speed set in runMaxSpeed. In PlayerRun.cs this value is mutiplied with _moveInput.x in case the...
                                                    // player uses a controller and wants more analog control

    public float runDecceleration;  //Time (approx. ) we want it to take for the player to accelerate from runMaxSpeed to 0. 
                                    //This goes from runMaxSpeed -> 0. not the other way

    [HideInInspector] public float runDeccelAmount; // Actual force applied to the player. Multiplied with speedDiff. See runAccelAmount comment above. 

    [Space(10)]

    [Header("In air")]

    [Range(0.01f, 1)] public float accelInAir; //Multipliers applied to acceleration rate when airborne
    [Range(0.01f, 1)] public float deccelInAir;
    public bool doConserveMomentum = true;

    [Header("Gravity Scale")]
    public float gravityScale = 1;
    public float JumpCutGravity = 1.5f;

    [Header("Jump")]
    public float jumpInputBufferTime;
    public float jumpForce;
    public float coyoteTime;

    [Header("Jump Hang")]
    public float jumpHangTimeThreshold;
    public float jumpHangAccelerationMult;
    //public float jumpHangTargetSpeedMult;
    public float jumpHangFloatMult;


    [Header("Ablities")]
    [Header("Dash")]
    public float dashInputBufferTime = 0.1f;
    public float dashSpeed = 1f;
    public float dashTime = 3;
    public float dashAccel;



    private void OnValidate()
    {
        runAccelAmount = 50 * runAcceleration / runMaxSpeed;
        runDeccelAmount = 50 * runDecceleration / runMaxSpeed;

        #region Variable Ranges
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);

        #endregion


    }





}
