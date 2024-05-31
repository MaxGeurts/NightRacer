using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera _cam;

    private Transform _car;
    private Transform _camFollowPoint;
    private Vector3 _startingCamFollowPos;

    private Rigidbody _carRb;

    private bool _inCollider = false;

    [SerializeField] private float _smoothSpeed;

    [SerializeField] private LayerMask _LayerMask;

    public bool canCamMove;
    public bool LookAtCar;

    private void Awake()
    {
        _camFollowPoint = GameObject.FindGameObjectWithTag("CamFollowPoint").transform;
    }
    
    void Start()
    {
        canCamMove = true;
        _startingCamFollowPos = _camFollowPoint.transform.localPosition;
        _car = GameObject.FindGameObjectWithTag("Player").transform;
        _carRb = _car.GetComponent<Rigidbody>();
        _cam = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        if (canCamMove)
        {
            float DistanceToCar = Vector3.Distance(transform.position, _car.transform.position);
            //makes the camera move smoothly and not go thruw walls
            if (Physics.Raycast(_car.transform.localPosition - new Vector3(0, 0, 0), -transform.forward, DistanceToCar, _LayerMask))
            {
                _camFollowPoint.localPosition = Vector3.MoveTowards(_camFollowPoint.transform.localPosition, Vector3.zero, Time.deltaTime * 40);
            }
            else if (_inCollider == false)
            {
                _camFollowPoint.localPosition = Vector3.MoveTowards(_camFollowPoint.transform.localPosition, _startingCamFollowPos, Time.deltaTime * 50);
            }

            if (8.5 - DistanceToCar < 1)
            {
                transform.position = Vector3.Lerp(transform.position, _camFollowPoint.transform.position, _smoothSpeed);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _camFollowPoint.transform.position, _smoothSpeed * (8.5f - DistanceToCar + 1));
            }
            transform.LookAt(_car.position);

            float xVelocity = _carRb.velocity.x;
            float zVelocity = _carRb.velocity.z;
            float yVelocity = _carRb.velocity.y;

            if (xVelocity < 0)
            {
                xVelocity = -xVelocity;
            }

            if (zVelocity < 0)
            {
                zVelocity = -zVelocity;
            }

            if (yVelocity < 0)
            {
                yVelocity = -yVelocity;
            }

            _cam.fieldOfView = 60 + (xVelocity + yVelocity + zVelocity) / 3;
            if (_cam.fieldOfView < 60)
            {
                _cam.fieldOfView = 60;
            }
        }
        else if (LookAtCar)
        {
            transform.LookAt(_car.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Default")
        {
            _inCollider = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Default")
        {
            _inCollider = true;
        }
    }
}
