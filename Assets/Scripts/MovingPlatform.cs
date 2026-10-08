using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private GameObject _rightPointObject;
    [SerializeField] private GameObject _leftPointObject;
    [SerializeField] private Vector2 GizmosPointSize = new Vector2(0.1f, 0.1f);

    private Vector3 _rightPointVector;
    private Vector3 _leftPointVector;

    private Vector3 _nextPosition;

    public float MoveSpeed;

    public bool Active;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        _rightPointVector = _rightPointObject.transform.position;
        _leftPointVector = _leftPointObject.transform.position;
    }


    void Start()
    {
        _nextPosition = _rightPointObject.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            transform.position = Vector2.MoveTowards(transform.position, _nextPosition, MoveSpeed * Time.deltaTime);

            if (transform.position == _nextPosition)
            {
                _nextPosition = (_nextPosition == _rightPointVector) ? _leftPointVector : _rightPointVector;
            }
        }

    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = transform;

        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = null;

        }

    }

    void OnDrawGizmos()
    {
        _rightPointVector = _rightPointObject.transform.position;
        _leftPointVector = _leftPointObject.transform.position;

        Gizmos.DrawWireCube(_rightPointVector, GizmosPointSize);
        Gizmos.DrawWireCube(_leftPointVector, GizmosPointSize);

    }

}
