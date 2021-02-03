using UnityEngine;
using UnityEngine.Networking;

public class bullet : NetworkBehaviour
{
    public AudioSource hit;
    public bool isExplosion;
    public GameObject explosionPrefab;
    private void OnDestroy()
    {
        NetworkServer.UnSpawn(gameObject);
        NetworkServer.Destroy(gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy")) {
            hit.Play();
            if (isExplosion) {
                Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 15f);
                Destroy(gameObject);
            }
        }
    }
}
