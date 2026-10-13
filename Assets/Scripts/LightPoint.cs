using UnityEngine;

public class LightPoint : MonoBehaviour
{
    [SerializeField] private int points = 10; // Points for each light 
    [SerializeField] private ParticleSystem collectEffect;

    PlayerController playerController;
    SpriteRenderer lightSpriteRenderer;

    private void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>();
        lightSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && lightSpriteRenderer.enabled)
        {
            lightSpriteRenderer.enabled = false;
            playerController.AddPoints(points);
            collectEffect.Play();
        }
    }


}
