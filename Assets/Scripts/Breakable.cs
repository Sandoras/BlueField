using UnityEngine;

public class Breakable : MonoBehaviour
{
    public ParticleSystem particle;
    public void Break()
    {
        ParticleSystem effect = Instantiate(particle, transform.position, transform.rotation);
        effect.Play();
        Destroy(gameObject);
    }
}
