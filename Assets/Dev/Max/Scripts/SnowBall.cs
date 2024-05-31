using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SnowBall : MonoBehaviour
{
    [SerializeField] private GameObject _Shield;
    private GameObject _player;

    private Rigidbody _playerRb;

    private ParticleSystem _particleSystem;

    private Rigidbody _rb;

    private Collider _collider;

    private MeshRenderer _meshRenderer;

    private Animator _animator;

    private bool _justBeanhit;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerRb = _player.GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _particleSystem = GetComponent<ParticleSystem>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) < 12 && _justBeanhit == false)
        {
            _justBeanhit = true;
            if (_Shield.activeInHierarchy == false)
            {
                StartCoroutine(KnockBack());
            }
            else
            {
                StartCoroutine(HitByShield());
            }
        }
    }

    private IEnumerator KnockBack()
    {
        if (Random.Range(0, 2) == 1)
        {
            _playerRb.AddRelativeForce(new Vector3(-750000, 10000, 0));
        }
        else
        {
            _playerRb.AddRelativeForce(new Vector3(750000, 10000, 0));
        }

        yield return new WaitForSeconds(2);

        _justBeanhit = false;
    }

    private IEnumerator HitByShield()
    {
        AudioSource hitSound = Camera.main.GetComponent<AudioSource>();
        _rb.isKinematic = true;
        _meshRenderer.enabled = false;
        _collider.enabled = false;
        _animator.enabled = false;
        _particleSystem.Play();

        hitSound.time = 4.3f;
        hitSound.Play();

        yield return new WaitForSeconds(1);

        hitSound.Stop();

        yield return new WaitForSeconds(2);

        _rb.isKinematic = false;
        _meshRenderer.enabled = true;
        _collider.enabled = true;
        _animator.enabled = true;
        _animator.Play("SnowBallAnim", -1, 0f);
        _animator.Play("lightSnowball", -1, 0f);
        _justBeanhit = false;
    }
}
