using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _healthPoints;
    [SerializeField] private int _startingHealthPoints = 5;

    [Header("Invincibility")]
    [SerializeField] private float _invincibleDuration = 0.8f;
    [SerializeField] private float _invincibleTimer;

    [Header("Respawn")]
    [SerializeField] private Vector3 _respawnLocation;

    [Header("Damage")]
    [SerializeField] private float _knockBackForce;

    private bool _isScriptOnPlayer;
    private PlayerMovement _playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        _healthPoints = _startingHealthPoints;

        if (gameObject.GetComponent<PlayerMovement>() != null)
        {
            _playerMovement = gameObject.GetComponent<PlayerMovement>();
            _isScriptOnPlayer = true;
        }
    }

    void Start()
    {
        _respawnLocation = transform.position;
    }

    void Update()
    {
        //ANIMATOR
        _playerMovement.PlayerAnimator.SetBool("b_IsInvinsible", IsInvinsible());

        if (IsInvinsible())
        {
            _invincibleTimer -= Time.deltaTime;

        }
        //Respawn
        if (_healthPoints <= 0)
        {
            Die();

        }

    }


    public void TakeDamage(int Damage, float KnockBackForce)
    {


        if (IsInvinsible())
            return;

        _healthPoints -= Damage;
        _invincibleTimer = _invincibleDuration;

        int facingDirection = _playerMovement.IsFacingRight ? -1 : 1;

        Vector2 knockbackDirection = new Vector2(facingDirection, 1);

        _playerMovement.RB.AddForce(knockbackDirection * KnockBackForce, ForceMode2D.Impulse);
    }

    public void Die()
    {
        Respawn();
    }

    public void Respawn()
    {
        transform.position = _respawnLocation;
        _healthPoints = _startingHealthPoints;

    }

    public bool IsInvinsible()
    {
        return _invincibleTimer >= 0;
    }
}
