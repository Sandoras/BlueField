using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private GameObject _rightPointObject;
    [SerializeField] private GameObject _leftPointObject;
    [SerializeField] private Vector2 GizmosPointSize = new Vector2(0.1f, 0.1f);

    private Vector2 _rightPointVector;
    private Vector2 _leftPointVector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        _rightPointVector = _rightPointObject.transform.position;
        _leftPointVector = _leftPointObject.transform.position;
    }


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    void OnDrawGizmos()
    {
        _rightPointVector = _rightPointObject.transform.position;
        _leftPointVector = _leftPointObject.transform.position;

        Gizmos.DrawWireCube(_rightPointVector, GizmosPointSize);
        Gizmos.DrawWireCube(_leftPointVector, GizmosPointSize);

    }

}
