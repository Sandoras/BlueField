using UnityEngine;

public class SquashSprite : MonoBehaviour
{
    [SerializeField] private Vector2 _squash;
    [SerializeField] private Vector2 _localScale;
    [SerializeField] private float _speed = 1;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _localScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        _squash = Vector2.MoveTowards(_squash, Vector2.one, _speed * Time.deltaTime);
        transform.localScale = _localScale * _squash;

    }

    public void DoSquash(float amount)
    {
        //Example: amount is 0.1 
        _squash.x = 1 - amount;
        _squash.y = 1 + amount;

    }
}
