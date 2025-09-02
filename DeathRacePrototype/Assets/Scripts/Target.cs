using UnityEngine;

public class Target : MonoBehaviour
{
    private Rigidbody rb;
    public float forceStrength = 10000.0f;
    [SerializeField] private ParticleSystem explosionVFX;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "CarHitbox")
        {
            Vector3 forceDirection = collision.contacts[0].normal;
            rb.AddForce(forceDirection * forceStrength, ForceMode.Impulse);
            explosionVFX.Play();
            explosionVFX.transform.parent = null;
            Invoke("Die", 1.0f);
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
