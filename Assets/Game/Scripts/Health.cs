using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;
    private Character _character;
    public float currentHealthPercentage 
    {
        get 
        {
            return (float)currentHealth / (float)maxHealth;
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        _character = GetComponent<Character>();
    }
    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;
        CheckHealth();
    }
    private void CheckHealth()
    {
        if (currentHealth <= 0)
            _character.SwitchStateTo(Character.characterState.Dead);
    }
    public void AddHealth(int health) 
    {
        currentHealth += health;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
}
