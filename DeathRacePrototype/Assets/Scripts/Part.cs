using UnityEngine;

public class Part : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    public int damage;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (currentHealth <= 0)
        {
            BreakOff();
        }
    }
    void TakeDamage(int damage)
    {
        if (currentHealth <= maxHealth)
        {
            currentHealth -= damage;
        }
        else if(currentHealth <= 0)
        {
            BreakOff();
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag != "Player")
        {
            TakeDamage(damage);
        }
        
    }

    void BreakOff()
    {
        Destroy(gameObject);
    }
}
