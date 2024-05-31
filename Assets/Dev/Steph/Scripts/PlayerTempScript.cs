using UnityEngine;

public class PlayerTempScript : MonoBehaviour
{
    public Vector3 lastReachedCheckpointPos;
    public Quaternion lastReachedCheckpointRot;
    private Rigidbody rb;
    public float speed = 30;
    public bool shieldEquipped;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
      //  rb.velocity = new Vector3(-Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal")) * speed;
    }

}
