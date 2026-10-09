using OpenAI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [SerializeField]
    NavMeshAgent agent;

    [SerializeField]
    public Transform player;

    // Update is called once per frame
    void FixedUpdate()
    {
        agent.SetDestination(player.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Bullet"))
        {
            player.GetComponent<PlayerControler>().PlayKillSound();
            Destroy(gameObject);
        }
    }
}
