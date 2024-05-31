using System.Collections;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public enum WhichObstacle
    {
        Snowpile,
        Spike,
        Tree,
        Air
    }

    private PowerUps powerUps;
    private CarController carScript;
    private Rigidbody carRb;
    private Collider[] colliders;

    private ParticleSystem _particleSystem;

    public WhichObstacle obstacleType;

    //slowness
    public float slowness;
    public float duration;
    private float startingMaxSpeed;

    //spike
    public float distance;
    private float startingDistance;
    private Vector3 startPosition;

    //air
    private float directionWind;
    private Rigidbody playerRB;

    void Start()
    {
        powerUps = FindObjectOfType<PowerUps>();
        carScript = FindObjectOfType<CarController>();
        colliders = GetComponents<Collider>();
        playerRB = powerUps.GetComponent<Rigidbody>();
        startingDistance = distance;
        startingMaxSpeed = carScript._MaxSpeed;
        carRb = powerUps.gameObject.GetComponent<Rigidbody>();


        switch (obstacleType)
        {
            case WhichObstacle.Spike:

                startPosition = transform.position;

                break;
            case WhichObstacle.Air:

                StartCoroutine(StartPositionAir());

                break;
        }
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, powerUps.transform.position) <= distance)
        {
            switch (obstacleType)
            {
                case WhichObstacle.Spike:

                    distance = 0;
                    StartCoroutine(DropSpike());

                    break;
                case WhichObstacle.Air:

                    break;
            }
        }
    }

    public void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Player"))
        {
            if (powerUps._ShieldEquipped)
            {
                if (obstacleType == WhichObstacle.Snowpile || obstacleType == WhichObstacle.Spike)
                {
                    if (obstacleType == WhichObstacle.Snowpile)
                    {
                        StartCoroutine(HitSound(Camera.main.GetComponent<AudioSource>(), 4.3f));
                    }
                    else if (obstacleType == WhichObstacle.Spike)
                    {
                        StartCoroutine(HitSound(GameObject.FindGameObjectWithTag("IceBreakSound").GetComponent<AudioSource>(), 1.5f));
                    }

                    _particleSystem = GetComponent<ParticleSystem>();
                    _particleSystem.Play();
                }
                GetComponent<MeshRenderer>().enabled = false;
                foreach (Collider collider in GetComponents<Collider>())
                {
                    collider.enabled = false;
                }
            }
            else
            {
                ObjectCheck(obstacleType);
            }
        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.CompareTag("Player"))
        {
            if (powerUps._ShieldEquipped)
            {
                gameObject.SetActive(false);
            }

            ObjectCheck(WhichObstacle.Air);
        }
    }
    private void ObjectCheck(WhichObstacle type)
    {
        switch (type)
        {
            case WhichObstacle.Snowpile:
                StartCoroutine(HitSound(Camera.main.GetComponent<AudioSource>(), 4.3f));

                StartCoroutine(TimerSlowness(false));

                _particleSystem = GetComponent<ParticleSystem>();
                _particleSystem.Play();


                break;
            case WhichObstacle.Spike:
                float velocityX = powerUps._Rb.velocity.x;
                float velocityZ = powerUps._Rb.velocity.z;

                StartCoroutine(HitSound(GameObject.FindGameObjectWithTag("IceBreakSound").GetComponent<AudioSource>(), 1.5f));

                if (velocityX < 0)
                {
                    velocityX = -velocityX;
                }

                if(velocityZ < 0)
                {
                    velocityZ = -velocityZ;
                }

                if (velocityZ + velocityX >= 30)
                {
                    StartCoroutine(TimerSlowness(false));
                    _particleSystem = GetComponent<ParticleSystem>();
                    _particleSystem.Play();
                }
                else
                {
                    slowness = 0;
                    StartCoroutine(TimerSlowness(true));

                }

                break;

            case WhichObstacle.Tree:
                slowness = 0;
                StartCoroutine(TimerSlowness(true));


                break;
            case WhichObstacle.Air:
                Vector3 pushDirection = new Vector3(0, 0, directionWind);
                playerRB.AddForce(pushDirection * 250);

                break;
        }
    }

    private IEnumerator HitSound(AudioSource hitSound, float startTime)
    {
        hitSound.time = startTime;
        hitSound.Play();

        yield return new WaitForSeconds(1);

        hitSound.Stop();
    }

    private IEnumerator TimerSlowness(bool keepObstacle)
    {
        GetComponent<MeshRenderer>().enabled = keepObstacle; //Turn off all colliders + the mesh renderer
        foreach (Collider col in colliders)
        {
            col.enabled = keepObstacle;
        }

        float fps = 1 / Time.deltaTime;
        float speedAdded =  carScript._MaxSpeed / (duration * 5) / (fps / 2); //how much speed added per frame
        carScript._MaxSpeed *= slowness;

        while (carScript._MaxSpeed < startingMaxSpeed)
        {
            carScript._MaxSpeed += speedAdded;
            carRb.drag =  1 - slowness;
            //Debug.Log(carScript._MaxSpeed);
            yield return null;

        }
        carRb.drag = 0.15f;
        carScript._MaxSpeed = startingMaxSpeed;

        gameObject.SetActive(keepObstacle);
    }


    private IEnumerator DropSpike()
    {
        for (int i = 0; i < 10; i++) //Amount of shakes
        {
            transform.position = new Vector3(startPosition.x + Random.Range(-0.5f, 0.5f), transform.position.y, startPosition.z + Random.Range(-0.5f, 0.5f)); //Position of shakes
            yield return new WaitForSeconds(0.075f); //Speed of shakes
        }

        while (!Physics.Raycast(transform.position, Vector3.down, 1f))
        {
            transform.position = new Vector3(startPosition.x, transform.position.y - 1f, startPosition.z);
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator StartPositionAir()
    {
        while (directionWind == 0)
        {
            directionWind = Random.Range(-1, 2);
        }

        transform.localPosition = new Vector3(powerUps.transform.position.x - 20, powerUps.transform.position.y, powerUps.transform.position.z); //temp inspawn system
        yield return null;
    }

    public void ResetObject()
    {
        if (gameObject.activeSelf == false || GetComponent<MeshRenderer>().enabled == false)
        {
            gameObject.SetActive(true);
            GetComponent<MeshRenderer>().enabled = true;
            foreach (Collider collider in GetComponents<Collider>())
            {
                collider.enabled = true;
            }
        }

        if ("Spike" == obstacleType.ToString())
        {
            transform.position = startPosition;
            distance = startingDistance;
        }
    }

}
