using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action OnEnemyDestroyed;
    public AudioClip destroyClip;
    public AudioSource audioSource;
    public int health = 3;

    public void TakeDamage(int damage)
    {
        if (health > damage)
        {
            health -= damage;

            if (health == 2)
            {
                GetComponent<Renderer>().material.color = Color.yellow;
            }
            else
            {
                GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else
        {
            audioSource.PlayOneShot(destroyClip);
            OnEnemyDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}
