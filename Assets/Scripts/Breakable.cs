using UnityEngine;

public class Breakable : MonoBehaviour
{

    [SerializeField] private LayerMask _playerLayer;

    // Update is called once per frame
    void Update()
    {


    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        int otherLayer = collision.gameObject.layer;
        if ((_playerLayer.value & (1 << otherLayer)) != 0)
        {
            Destroy(gameObject);
        }
    }
}
