using UnityEngine;

public class PlayerCollidables : MonoBehaviour
{
    public enum WhichColliderType
    {
        Checkpoint,
        RespawnLastCheckpoint,
        Finish,
        EnterCave,
        ExitCave,
        RestartGame
    }

    private CarController playerScript;
    private GameMngr gameMngrScript;
    private Animator snowBallAnim;
    private MeshRenderer snowBallMeshRenderer;
    private Collider snowBallColl;
    private Rigidbody snowBallRb;
    private GameObject car;
    private Obstacles[] obstacles;
    private AudioSource _audioSource;
    private ParticleSystem[] snow;

    public WhichColliderType whichColType;

    public int checkpointNumber;

    void Start()
    {
        GameObject[] snowball = GameObject.FindGameObjectsWithTag("SnowBall");

        foreach (GameObject obj in snowball)
        {
            snowBallAnim = obj.GetComponent<Animator>();
            snowBallColl = obj.GetComponent<Collider>();
            snowBallMeshRenderer = obj.GetComponent<MeshRenderer>();
            snowBallRb = obj.GetComponent<Rigidbody>();

            snowBallAnim.enabled = false;
            snowBallColl.enabled = false;
            snowBallMeshRenderer.enabled = false;
            snowBallRb.isKinematic = true;
        }

        playerScript = FindObjectOfType<CarController>();
        gameMngrScript = FindObjectOfType<GameMngr>();
        obstacles = FindObjectsOfType<Obstacles>();
        car = GameObject.Find("Car 1 1");
        snow = Camera.main.GetComponentsInChildren<ParticleSystem>();
    }

    public void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Player"))
        {
            ObjectCheck(whichColType);
        }
    }

    private void ObjectCheck(WhichColliderType type)
    {
        switch (type)
        {
            case WhichColliderType.Checkpoint:

                if (gameMngrScript.curCheckpoint == checkpointNumber - 1)
                {
                    gameMngrScript.SaveSegmentTime();
                   playerScript.lastReachedCheckpointPos = car.transform.position;
                   playerScript.lastReachedCheckpointRot = car.transform.rotation;
                   gameMngrScript.curCheckpoint++;
                    _audioSource = GetComponent<AudioSource>();
                    _audioSource.Play();
                }


                break;
            case WhichColliderType.RespawnLastCheckpoint:
                Camera.main.GetComponent<CameraFollow>().canCamMove = false;
                playerScript.ResetToLastCheckpoint();

                break;
            case WhichColliderType.Finish:


                if (gameMngrScript.curCheckpoint == 3)
                {
                    gameMngrScript.timerOn = false;
                    gameMngrScript.CurrentLap++;
                    _audioSource = GetComponent<AudioSource>();
                    _audioSource.Play();
                    gameMngrScript.SaveSegmentTime();


                    foreach (Obstacles obstacle in obstacles)
                    {
                        obstacle.ResetObject();
                    }

                    gameMngrScript.SaveLapTime();
                    if (gameMngrScript.CurrentLap == 4)
                    {
                        gameMngrScript.FinishLaps();
                    }
                }

                gameMngrScript.MakeTop10();

                break;
            case WhichColliderType.EnterCave:

                GameObject[] snowball = GameObject.FindGameObjectsWithTag("SnowBall");

                foreach(GameObject obj in snowball)
                {
                    snowBallAnim = obj.GetComponent<Animator>();
                    snowBallColl = obj.GetComponent<Collider>();
                    snowBallMeshRenderer = obj.GetComponent<MeshRenderer>();
                    snowBallRb = obj.GetComponent<Rigidbody>();

                    snowBallAnim.enabled = true;
                    snowBallColl.enabled = true;
                    snowBallMeshRenderer.enabled = true;
                    snowBallRb.isKinematic = false;
                }
                car.GetComponent<PowerUps>().IsInCave = true;

                CarTP(new Vector3(977.630005f, -1710.20996f, 862.690002f), new Vector3(0, 170.010254f, 0), new Vector3(976.259583f, -1707.88f, 870.470215f), new Vector3(0, 170.010254f, 0));
                snow[0].Stop();

                break;
            case WhichColliderType.ExitCave:

                GameObject[] snowBall = GameObject.FindGameObjectsWithTag("SnowBall");

                foreach (GameObject obj in snowBall)
                {
                    snowBallAnim = obj.GetComponent<Animator>();
                    snowBallColl = obj.GetComponent<Collider>();
                    snowBallMeshRenderer = obj.GetComponent<MeshRenderer>();
                    snowBallRb = obj.GetComponent<Rigidbody>();

                    snowBallAnim.enabled = false;
                    snowBallColl.enabled = false;
                    snowBallMeshRenderer.enabled = false;
                    snowBallRb.isKinematic = true;
                }

                car.GetComponent<PowerUps>().IsInCave = false;
                CarTP(new Vector3(1055.39124f, 152.330002f, 1014.21509f), new Vector3(0, 165.119f, 0), new Vector3(1057.42004f, 149.330002f, 1006.58002f), new Vector3(0, -195, 0));

               
                snow[0].Play();


                break;
            case WhichColliderType.RestartGame:
                car.GetComponent<PowerUps>().IsInCave = false;
               

                foreach (Obstacles obstacle in obstacles)
                {
                    obstacle.ResetObject();
                }
                gameMngrScript.RestartGame();

                break;
            
        }
    }

    private void CarTP(Vector3 carPos, Vector3 carRot, Vector3 camPos, Vector3 camRot)
    {
        car.transform.position = carPos;
        car.transform.eulerAngles = carRot;
        Camera.main.transform.position = camPos;
        Camera.main.transform.eulerAngles = camRot;

        car.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}
