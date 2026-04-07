using UnityEngine;

public class SpaceShip : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player.ResetPlayer();
    }

    public void TakeDamage(float damage)
    {
        Player.TakeDamage(damage);
        // TODO: Add some visual feedback for the player taking damage.
    }

    // TODO: Add a function that will make the player lose if health is 0 or less.

    public void UseAmmo(int amount)
    {
        Player.UseAmmo(amount);
    }
}
