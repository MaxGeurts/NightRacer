using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
    [SerializeField] private Vector3 _CenterOfMass;

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.centerOfMass = _CenterOfMass;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.localPosition + _CenterOfMass, 0.3f);
    }
}
