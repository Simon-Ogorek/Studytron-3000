using UnityEngine;
using System;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.AI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
namespace OpenAI
{
    public class PlayerControler : MonoBehaviour
    {

        [Header("References")]
        [SerializeField]
        Rigidbody rb;
        [SerializeField]
        AudioSource audioSource;
        [SerializeField]
        GameObject LoseScreen;
        [SerializeField]
        TMP_Text healthText;
        [SerializeField]
        TMP_Text questionText;
        [SerializeField]
        GameObject questionUI;

        [Space()]

        [Header("Assets")]
        [SerializeField]
        GameObject bullet;
        [SerializeField]
        GameObject zombie;

        
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
            public float health;

            public void Init()
            {
                health = maxHealth;
            }
        }

        public int frameCounter = 0;
        public int framesToSpawn = 100;
        bool answering = false;
        bool justAnswered = false;
        bool isTrueCorrect = false;
        private OpenAIApi openai = new OpenAIApi("");
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            stats.Init();
            StartCoroutine(DifficultyCurve());
            QuestionRoutine();
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 inputVector = Vector3.zero;

            inputVector.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
            inputVector.x += Input.GetKey(KeyCode.D) ? 1 : 0;
            inputVector.z += Input.GetKey(KeyCode.W) ? 1 : 0;
            inputVector.z -= Input.GetKey(KeyCode.S) ? 1 : 0;

            rb.AddForce(inputVector * stats.speed * Time.deltaTime);

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane ground = new Plane(Vector3.up, transform.position);

                if (ground.Raycast(ray, out float distance))
                {
                    Vector3 mousePos = ray.GetPoint(distance);
                    Vector3 mouseDir = mousePos - transform.position;
                    mouseDir.y = 0;
                    mouseDir.Normalize();

                    GameObject tempBullet = Instantiate(bullet, transform.position, Quaternion.LookRotation(mouseDir));

                    tempBullet.GetComponent<Rigidbody>().AddForce(mouseDir * 100, ForceMode.Impulse);
                    StartCoroutine(TimeOutBullet(tempBullet));
                }
            }

            if (answering)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (isTrueCorrect)
                    {
                        PositiveEffect();
                    }
                    else
                    {
                        NegativeEffect();
                    }
                    answering = false;
                    justAnswered = true;
                    questionUI.SetActive(false);

                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (!isTrueCorrect)
                    {
                        PositiveEffect();
                    }
                    else
                    {
                        NegativeEffect();
                    }
                    answering = false;
                    justAnswered = true;
                    questionUI.SetActive(false);
                }
            }
        }

        void PositiveEffect()
        {
            stats.health += 20;
        }

        void NegativeEffect()
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                obj.GetComponent<NavMeshAgent>().speed *= 3;
            }
        }

        IEnumerator DifficultyCurve()
        {
            while (framesToSpawn > 20)
            {
                yield return new WaitForSecondsRealtime(0.5f);
                framesToSpawn--;
            }
            yield break;
        }

        void FixedUpdate()
        {
            frameCounter++;

            if (frameCounter > framesToSpawn)
            {
                frameCounter = 0;
                SpawnEnemy();
            }

            healthText.text = stats.health.ToString();
        }

        void SpawnEnemy()
        {
            GameObject tempEnemy = Instantiate(zombie);
            NavMeshAgent agent = tempEnemy.GetComponent<NavMeshAgent>();
            NavMeshHit hit;
            NavMesh.SamplePosition(tempEnemy.transform.position + (UnityEngine.Random.insideUnitSphere * 100f), out hit, float.MaxValue, -1);

            agent.Warp(hit.position); 
            tempEnemy.GetComponent<Zombie>().player = transform;
        }

        IEnumerator TimeOutBullet(GameObject bullet)
        {
            yield return new WaitForSecondsRealtime(3f);
            Destroy(bullet);
        }

        public void PlayKillSound()
        {
            audioSource.PlayOneShot(killSound);
        }

        void Lose()
        {
            LoseScreen.SetActive(true);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.transform.CompareTag("Enemy"))
            {
                stats.health -= 5;

                if (stats.health <= 0)
                {
                    Lose();
                    Destroy(gameObject);
                }
                audioSource.PlayOneShot(hurtSound);
                Debug.Log("Ow");
            }
        }

        async void QuestionRoutine()
        {
            while (true)
            {
                await Awaitable.WaitForSecondsAsync(UnityEngine.Random.Range(15,30));

                if (!answering && !justAnswered)
                {
                    answering = true;
                    var req = new CreateChatCompletionRequest
                    {
                        Model = "gpt-5-mini",
                        Messages = new List<ChatMessage>()
                        {
                            new ChatMessage()
                            {
                                Role = "system",
                                Content = "Your goal is to make true or false questions for these topics, pick a random topic from this list of things and return a true or false question, only basic text, no formatting, no markdown. The list: APT, Remote Access Trojan, backdoor, botnet, cryptojacker, infostealer, keylogger, malvertising, malware, ransomware, rootkit, spyware, Trojan, horse, virus, worm. Your response should be less than two sentences. YOU MUST LEAVE THE LAST CHARACTER OF YOUR RESPONSE EITHER T OR F, NO OTHER CHARACTER IS ACCEPTABLE"
                            }
                        }
                    };
                    var res = await openai.CreateChatCompletion(req);
                    string response = res.Choices[0].Message.Content.Trim();
                    char answer = response[^1];
                    response = response.Remove(response.Length - 1);

                    isTrueCorrect = answer == 'T';

                    questionUI.SetActive(true);
                    questionText.text = response;
                    Debug.Log("Correct answer: " + isTrueCorrect);
                }

                if (justAnswered)
                    justAnswered = false;
            }
        }
    }
}