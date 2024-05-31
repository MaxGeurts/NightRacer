using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public enum PowerUp
{
    SpeedBoost,
    Shield,
    SnowballRamp
}
public class PowerUps : MonoBehaviour
{
    [SerializeField] private float _SpeedBoostPower;
    [SerializeField] private float _SnowBallZForce;
    [SerializeField] private float _SnowBallYForce;
    [SerializeField] private float _PowerUpCooldown;
    public float _ShieldDuration;
    public float _PowerUpCooldownTimer;

    public bool _ShieldEquipped;

    [SerializeField] public PowerUp _PowerUp;

    [SerializeField] private GameObject _Snowball;
    public GameObject _Shield;
    [SerializeField] private GameObject _BoostVisual;
    [SerializeField] private GameObject _PlayMenu;

    [SerializeField] private Image _ShieldPowerUi;
    [SerializeField] private Image _SpeedBoostPowerUi;
    [SerializeField] private Image _PowerUpUi;

    [SerializeField] MeshRenderer[] bodyMat;
    [SerializeField] Material[] mat;
    private SnowBall snowBall;

    public Rigidbody _Rb;

    [SerializeField] private AudioSource _BoostAudio;

    private Vector3 _startingBoostScale;
    public bool IsInCave;

    private float caveFallTimer;
    private bool isFalling;

    void Start()
    {
        if (PlayerPrefs.HasKey("SelectedFile"))
        {
            _PowerUp = (PowerUp)PlayerPrefs.GetInt("Vehicle" + PlayerPrefs.GetString("SelectedFile")) - 1;
        }
        _Rb = GetComponent<Rigidbody>();

        _startingBoostScale = _BoostVisual.transform.localScale;

        if (_PowerUp.ToString() == "Shield")
        {
            _ShieldPowerUi.enabled = true;
            bodyMat[0].material = mat[1];
            bodyMat[1].material = mat[1];
        }
        else
        {
            _SpeedBoostPowerUi.enabled = true;
            bodyMat[0].material = mat[0];
            bodyMat[1].material = mat[0];
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && _PowerUpCooldownTimer <= 0 && _PlayMenu.activeInHierarchy == true)
        {
            //string powerUpName = _PowerUp.ToString();
            Invoke(_PowerUp.ToString(), 0);
            _PowerUpCooldownTimer = _PowerUpCooldown;
        }
        else if (_PowerUpCooldownTimer >= 0)
        {
            if (_ShieldEquipped == false && _PlayMenu.activeInHierarchy == true)
            {
                _PowerUpCooldownTimer -= Time.deltaTime;
            }

            _PowerUpUi.fillAmount = 100 / _PowerUpCooldown * (_PowerUpCooldown - _PowerUpCooldownTimer) / 100;

            if (_PowerUp.ToString() == "Shield")
            {
                _ShieldPowerUi.fillAmount = _PowerUpUi.fillAmount;
            }
            else
            {
                _SpeedBoostPowerUi.fillAmount = _PowerUpUi.fillAmount;
            }
        }

        if (isFalling && IsInCave)
        {
            caveFallTimer += Time.deltaTime;
        }
        if (caveFallTimer > 0.2f && IsInCave)
        {
            _Rb.velocity = transform.up * -45;
            Camera.main.GetComponent<CameraFollow>().canCamMove = false;
            Camera.main.GetComponent<CameraFollow>().LookAtCar = true;
        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.CompareTag("CavePath") && IsInCave)
        {
            isFalling = false;
            caveFallTimer = 0;
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.CompareTag("CavePath") && IsInCave)
        {
            isFalling = true;
        }
    }

    private void SpeedBoost()
    {
        _Rb.AddRelativeForce(new Vector3(0, 0, _SpeedBoostPower));
        StartCoroutine(Boost());
    }

    private IEnumerator Boost()
    {
        _BoostVisual.transform.localScale = new Vector3(_BoostVisual.transform.localScale.x + 1, _BoostVisual.transform.localScale.y + 1, _BoostVisual.transform.localScale.z + 1);

        _BoostAudio.enabled = true;
        _BoostAudio.Play();
        _BoostAudio.volume = 100;
        while (_BoostVisual.transform.localScale.z > _startingBoostScale.z)
        {
            yield return new WaitForEndOfFrame();
            _BoostVisual.transform.localScale = Vector3.Lerp(_BoostVisual.transform.localScale, _startingBoostScale, Time.deltaTime);
            _BoostAudio.volume = -(100 - 100 / _startingBoostScale.z * _BoostVisual.transform.localScale.z);
        }
        _BoostAudio.enabled = false;
    }

    private void SnowballRamp()
    {
        StartCoroutine(SnowballRampCoroutine());
    }

    private IEnumerator SnowballRampCoroutine()
    {
        if (GameObject.FindGameObjectWithTag("SnowBall") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("SnowBall"));
        }

        Instantiate(_Snowball, transform.localPosition + transform.forward * 3, transform.localRotation);
        yield return new WaitForEndOfFrame();
        snowBall = GameObject.FindGameObjectWithTag("SnowBall").GetComponent<SnowBall>();
    }

    private void Shield()
    {
        StartCoroutine(ShieldCoroutine());
    }

    private IEnumerator ShieldCoroutine()
    {
        Material shield = _Shield.GetComponent<Renderer>().material;
        shield.color = new Vector4(shield.color.r, shield.color.g, shield.color.b, 0);
        float startShieldDuration = _ShieldDuration;

        _ShieldEquipped = true;
        _Shield.SetActive(true);

        AudioSource audio = _Shield.GetComponent<AudioSource>();
        audio.time = 1;

        while (shield.color.a < 0.55)
        {
            if (_PlayMenu.activeInHierarchy == true)
            {
                yield return new WaitForSeconds(0.1f);
                shield.color = new Vector4(shield.color.r, shield.color.g, shield.color.b, shield.color.a + 0.02f);
            }
            yield return new WaitForEndOfFrame();
        }

        while (startShieldDuration >= 0)
        {
            if (_PlayMenu.activeInHierarchy == true)
            {
                startShieldDuration -= Time.deltaTime;
            }
            yield return new WaitForEndOfFrame();
        }

        audio.time = 0;
        audio.Play();

        while (shield.color.a > 0)
        {
            if (_PlayMenu.activeInHierarchy == true)
            {
                yield return new WaitForSeconds(0.1f);
                shield.color = new Vector4(shield.color.r, shield.color.g, shield.color.b, shield.color.a - 0.051f);
            }
            yield return new WaitForEndOfFrame();
        }
        _ShieldEquipped = false;
        _Shield.SetActive(false);
    }

}
