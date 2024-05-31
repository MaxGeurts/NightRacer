using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    private PowerUps _powerups;
    private CarController _carController;

    private Rigidbody _rb;

    private float _speed;
    private float _oldSpeedometerArmZRotation;

    private TextMeshProUGUI _speedText;
    private GameObject _speedometerArm;

    void Start()
    {
        _powerups = FindObjectOfType<PowerUps>();
        _carController = FindObjectOfType<CarController>();
        _rb = _powerups.GetComponent<Rigidbody>();
        _speedText = GameObject.FindGameObjectWithTag("SpeedText").GetComponent<TextMeshProUGUI>();
        _speedometerArm = GameObject.FindGameObjectWithTag("SpeedometerArm");
        _oldSpeedometerArmZRotation = _speedometerArm.transform.localEulerAngles.z;
    }

    void Update()
    {
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

        _speed = zVelocity + xVelocity;

        if (_speed < 0.5f)
        {
            _speed = 0.5f;
        }

        _speedText.text = ((int)_speed * 2).ToString() + " Km/H";

        _speedometerArm.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, -260, _speed / _carController._MaxSpeed));

        _speedometerArm.transform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(_oldSpeedometerArmZRotation, _speedometerArm.transform.localEulerAngles.z, Time.deltaTime));
        _oldSpeedometerArmZRotation = _speedometerArm.transform.localEulerAngles.z;
    }
}
