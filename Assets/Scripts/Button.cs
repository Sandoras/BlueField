using UnityEngine;

public class Button : MonoBehaviour
{
    public bool ButtonPress;
    public MovingPlatform OtherObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OtherObject.Active = true;
            gameObject.SetActive(false);
        }
    }
    void Update()
    {
        // OtherObject.Active = ButtonPress;
    }
}
