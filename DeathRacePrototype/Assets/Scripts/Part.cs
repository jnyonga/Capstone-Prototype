using UnityEngine;

public class Part : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    void TakeDamage(int damage)
    {
        if (currentHealth > maxHealth)
        {
            currentHealth -= damage;
        }
    }
}
