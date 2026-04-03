using UnityEngine;

public class SpaceShip : MonoBehaviour
{

    float health;
    int ammo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 100f;
        ammo = 100;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        // TODO: Add some visual feedback for the player taking damage.
    }

    // TODO: Add a function that will make the player lose if health is 0 or less.

    public void UseAmmo(int amount)
    {
        ammo -= amount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
