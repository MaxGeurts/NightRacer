using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody _rb;

    [SerializeField] private Transform _CarTransform;

    [SerializeField] private AudioSource _TrusterAudio;
    [SerializeField] private GameObject _TrusterVisual;
    [SerializeField] private GameObject _BigTrusterVisual;
    [SerializeField] private Vector3 _StartingTrusterScale;

    [SerializeField] private WheelCollider _LeftFrontWheel;
    [SerializeField] private WheelCollider _RightFrontWheel;
    [SerializeField] private WheelCollider _LeftBackWheel;
    [SerializeField] private WheelCollider _RightBackWheel;

    [SerializeField] private float _TurnSpeed;
    [SerializeField] private float _MaxTurnAngle;
    [SerializeField] private float _MaxCarZRotation;
    [SerializeField] private float _Power;
    [SerializeField] private float _DownForce;
    public float _MaxSpeed;
    private float _startPower;
    private float _turnAngle;

    WheelFrictionCurve startingFFriction;
    WheelFrictionCurve startingSFriction;

    [SerializeField] private LayerMask _LayerMask;

    public bool ObstacleHasBeenHit;
    private float slownessTimeCalc;

    public Vector3 lastReachedCheckpointPos;
    public Quaternion lastReachedCheckpointRot;

    private PowerUps _powerUps;
    private GameObject _shield;

    public bool MovementOn = false;
    private Vector3 velocityBeforePause;


    private void Start()
    {
        _rb = _CarTransform.GetComponent<Rigidbody>();

        startingFFriction = _LeftFrontWheel.forwardFriction;
        startingSFriction = _LeftFrontWheel.sidewaysFriction;

        _StartingTrusterScale = _BigTrusterVisual.transform.localScale;

        _TrusterAudio.enabled = false;

        _startPower = _Power;

        _powerUps = _CarTransform.gameObject.GetComponent<PowerUps>();
        _shield = _powerUps._Shield;
    }

    void Update()
    {
        if (MovementOn)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Camera.main.GetComponent<CameraFollow>().canCamMove = false;

                ResetToLastCheckpoint();
            }

            if (_TrusterAudio.time > 7)
            {
                _TrusterAudio.time = 1;
            }

            if (Input.GetAxisRaw("Vertical") > 0)
            {
                _TrusterAudio.enabled = true;
                if (_TrusterAudio.volume <= 100)
                {
                    _TrusterAudio.volume += Time.deltaTime;
                }
                _TrusterVisual.SetActive(true);
                _BigTrusterVisual.transform.localScale = Vector3.Lerp(_BigTrusterVisual.transform.localScale, _StartingTrusterScale, Time.deltaTime);
            }
            else
            {
                if (_TrusterAudio.volume >= 0)
                {
                    _TrusterAudio.volume -= Time.deltaTime;
                }
                _BigTrusterVisual.transform.localScale = Vector3.Lerp(_BigTrusterVisual.transform.localScale, Vector3.zero, Time.deltaTime);
                if (_BigTrusterVisual.transform.localScale.z <= 0.15f)
                {
                    _TrusterVisual.SetActive(false);
                }
            }

            if (ObstacleHasBeenHit)
            {
                slownessTimeCalc += Time.deltaTime;

                // maxspeed * time / 2
            }
            //if (time > 2)
            // {
            //     false
            //    time = 0
            // }
            float xVelocity = _rb.velocity.x;
            float zVelocity = _rb.velocity.z;

            if (xVelocity < 0)
            {
                xVelocity = -xVelocity;
            }

            if (zVelocity < 0)
            {
                zVelocity = -zVelocity;
            }

            if (xVelocity + zVelocity < _MaxSpeed)
            {
                _LeftFrontWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
                _RightFrontWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
                _RightBackWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
                _LeftBackWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
            }
            else if (Input.GetAxisRaw("Vertical") < 0)
            {
                _LeftFrontWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
                _RightFrontWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
                _RightBackWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
                _LeftBackWheel.motorTorque = _Power * Input.GetAxisRaw("Vertical") * Time.deltaTime;
            }
            else
            {
                _LeftFrontWheel.motorTorque = 0;
                _RightFrontWheel.motorTorque = 0;
                _RightBackWheel.motorTorque = 0;
                _LeftBackWheel.motorTorque = 0;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
            {
                Turn(_TurnSpeed * Input.GetAxisRaw("Horizontal") * Time.deltaTime * 60);

                if (Input.GetAxisRaw("Horizontal") < 0 && _turnAngle > 0)
                {
                    transform.localRotation = new quaternion(transform.localRotation.x, 0, transform.localRotation.z, transform.localRotation.w);
                }
                else if (Input.GetAxisRaw("Horizontal") > 0 && _turnAngle < 0)
                {
                    transform.localRotation = new quaternion(transform.localRotation.x, 0, transform.localRotation.z, transform.localRotation.w);
                }
            }

            if (Input.GetAxisRaw("Horizontal") == 0 && _LeftFrontWheel.steerAngle != 0)
            {
                //makes the wheels look forward after you stoped turning
                SteeringToStaraight(_TurnSpeed * Time.deltaTime * 10);
            }

            ClampSoYouDontFallOverAndPhysicMaterialCheck();

            _rb.AddForce(-transform.up * _DownForce);
        }
    }

    public void FreezeCarMovement(bool isGamePaused)
    {
        if (isGamePaused) { velocityBeforePause = _rb.velocity; }
        _rb.freezeRotation = isGamePaused;
        _rb.isKinematic = isGamePaused;
        MovementOn = !isGamePaused;
        _rb.useGravity = !isGamePaused;
        if (!isGamePaused) { _rb.velocity = velocityBeforePause; }
    }

    void Turn(float whatChanged)
    {
        _turnAngle += whatChanged;
        _turnAngle = Mathf.Clamp(_turnAngle, -_MaxTurnAngle, _MaxTurnAngle);
        //turns the bar that the wheals are on.
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, _turnAngle, transform.localRotation.eulerAngles.z);
        //turns the rotation of the wheels along with the bar
        _LeftFrontWheel.steerAngle = _turnAngle;
        _RightFrontWheel.steerAngle = _turnAngle;
    }

    void SteeringToStaraight(float whatChanged)
    {
        _turnAngle = Mathf.Lerp(transform.localRotation.y, 0, whatChanged);
        transform.localRotation = new quaternion(transform.localRotation.x, _turnAngle, transform.localRotation.z, transform.localRotation.w);
        _LeftFrontWheel.steerAngle = _turnAngle;
        _RightFrontWheel.steerAngle = _turnAngle;
    }

    private void ClampSoYouDontFallOverAndPhysicMaterialCheck()
    {
        float zRotation = _CarTransform.rotation.eulerAngles.z;

        RaycastHit hit;
        if (Physics.Raycast(_CarTransform.localPosition, -_CarTransform.up, out hit, 3, _LayerMask))
        {
            //makes it so the car can't fall over
            if (zRotation > 180)
            {
                //makes it so the car can't fall over to the right
                zRotation = Mathf.Clamp(zRotation, 360 - _MaxCarZRotation, 360);
            }
            else
            {
                //makes it so the car can't fall over to the left
                zRotation = Mathf.Clamp(zRotation, 0, _MaxCarZRotation);
            }

            //makes it so the pysicmetarials works
            if (hit.collider.gameObject.tag == "Ice")
            {
                WheelFrictionCurve fStifnes = _RightFrontWheel.forwardFriction;
                fStifnes.stiffness = hit.collider.material.staticFriction;
                _RightFrontWheel.forwardFriction = fStifnes;
                _LeftFrontWheel.forwardFriction = fStifnes;
                _RightBackWheel.forwardFriction = fStifnes;
                _LeftBackWheel.forwardFriction = fStifnes;

                WheelFrictionCurve sStifnes = _RightFrontWheel.sidewaysFriction;
                sStifnes.stiffness = hit.collider.material.dynamicFriction;
                _RightFrontWheel.sidewaysFriction = sStifnes;
                _LeftFrontWheel.sidewaysFriction = sStifnes;
                _RightBackWheel.sidewaysFriction = sStifnes;
                _LeftBackWheel.sidewaysFriction = sStifnes;

                _Power = _startPower * 2f;
            }
            else
            {
                //sets the varibles in the wheel colliders to what it is when you started driving
                _RightFrontWheel.forwardFriction = startingFFriction;
                _LeftFrontWheel.forwardFriction = startingFFriction;
                _RightBackWheel.forwardFriction = startingFFriction;
                _LeftBackWheel.forwardFriction = startingFFriction;

                _RightFrontWheel.sidewaysFriction = startingSFriction;
                _LeftFrontWheel.sidewaysFriction = startingSFriction;
                _RightBackWheel.sidewaysFriction = startingSFriction;
                _LeftBackWheel.sidewaysFriction = startingSFriction;

                _Power = _startPower;
            }
        }
        else
        {
            if (zRotation > 180)
            {
                //makes it so the car can't fall over to the right
                zRotation = Mathf.Clamp(zRotation, 360 - _MaxCarZRotation * 2, 360);
            }
            else
            {
                //makes it so the car can't fall over to the left
                zRotation = Mathf.Clamp(zRotation, 0, _MaxCarZRotation * 2);
            }

            //turns Z axis of the car in the air to stay strait up in the air
            if (zRotation < 180)
            {
                zRotation = Mathf.Lerp(zRotation, 0.1f, Time.deltaTime * 3);
            }
            else
            {
                zRotation = Mathf.Lerp(zRotation, 359.9f, Time.deltaTime * 3);
            }

        }
        _CarTransform.transform.rotation = Quaternion.Euler(_CarTransform.transform.rotation.eulerAngles.x, _CarTransform.transform.rotation.eulerAngles.y, zRotation);
    }

    public void ResetToLastCheckpoint()
    {
        StartCoroutine(ResetToLastCheckpointCoroutine());

    }

    private IEnumerator ResetToLastCheckpointCoroutine()
    {
        Material shield = _shield.GetComponent<Renderer>().material;
        _shield.SetActive(false);
        shield.color = new Vector4(shield.color.r, shield.color.g, shield.color.b, 1);
        yield return new WaitForSeconds(0.11f);
        shield.color = new Vector4(shield.color.r, shield.color.g, shield.color.b, 0);
        _powerUps._ShieldDuration = 0;
        _powerUps._PowerUpCooldownTimer = 0;

        Camera.main.GetComponent<CameraFollow>().LookAtCar = false;
        if (_powerUps.IsInCave)
        {
            _CarTransform.position = new Vector3(977.630005f, -1710.20996f, 862.690002f);
            _CarTransform.eulerAngles = new Vector3(0, 170, 0);
        }
        else
        {
            _CarTransform.position = lastReachedCheckpointPos;
            _CarTransform.rotation = lastReachedCheckpointRot;
        }
        _rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.1f);
        Camera.main.transform.position = lastReachedCheckpointPos;
        Camera.main.transform.rotation = lastReachedCheckpointRot;
        Camera.main.GetComponent<CameraFollow>().canCamMove = true;
    }

}
