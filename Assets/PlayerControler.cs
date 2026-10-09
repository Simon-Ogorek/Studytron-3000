using UnityEngine;
using System;
using Unity.VisualScripting;
public class PlayerControler : MonoBehaviour
{

    [Header("References")]
    [SerializeField]
    Rigidbody rb;
    [SerializeField]
    AudioSource audioSource;

    [Space()]

    [Header("Assets")]
    [SerializeField]
    GameObject bullet;
    
    [Space]

    [Header("Sounds")]
    [SerializeField]
    AudioClip hurtSound;
    [SerializeField]
    AudioClip upgradeSound;
    [SerializeField]
    AudioClip killSound;

    [Header("Config")]
    [SerializeField]
    PlayerStats stats;

    [Serializable]
    public class PlayerStats
    {
        // Config 
        public float maxHealth;
        public float speed;
        public int bulletsPerShot;
        public float damagePerBullet;

        // Runtime
        float health;

        public void Init()
        {
            health = maxHealth;
        }

        public void Damage(float damage)
        {
            health -= damage;
        }
        
        public void Heal(float amount)
        {
            health = MathF.Min(health + amount, maxHealth);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputVector = Vector3.zero;

        inputVector.x += Input.GetKey(KeyCode.A) ? 1 : 0;
        inputVector.x -= Input.GetKey(KeyCode.D) ? 1 : 0;
        inputVector.z += Input.GetKey(KeyCode.W) ? 1 : 0;
        inputVector.z -= Input.GetKey(KeyCode.S) ? 1 : 0;

        rb.AddForce(inputVector * stats.speed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseDir = (Vector2)Input.mousePosition - new Vector2(Screen.width / 2f, Screen.height / 2);
            mouseDir.Normalize();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            stats.Damage(5);
            audioSource.PlayOneShot(hurtSound);
            Debug.Log("Ow");
        }
    }
}
