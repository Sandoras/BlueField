using TMPro;
using UnityEngine;

public class UI_CanvasController : MonoBehaviour
{
    private Health _playerHealth;

    private TMP_Text _livesText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playerHealth = FindAnyObjectByType<PlayerMovement>().GetComponent<Health>();
        _livesText = transform.Find("LivesText").GetComponent<TMP_Text>();

    }

    // Update is called once per frame
    void Update()
    {
        _livesText.text = "Life left: " + _playerHealth._healthPoints;


    }
}
