using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    private readonly Player player = new Player();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player.ResetPlayer();
    }

    public void TakeDamage(float damage)
    {
        player.TakeDamage(damage);
        // TODO: Add some visual feedback for the player taking damage.
    }

    // TODO: Add a function that will make the player lose if health is 0 or less.

    public void UseAmmo(int amount)
    {
        player.UseAmmo(amount);
    }
}
