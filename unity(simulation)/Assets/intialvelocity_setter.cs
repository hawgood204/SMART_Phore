using UnityEngine;

public class InitialVelocitySetter : MonoBehaviour
{
    public Vector3 initialVelocity = Vector3.zero;
    public Vector3 initialAngularVelocity = Vector3.zero;

    private Rigidbody rb;
    private bool applied = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!applied && Application.isPlaying)
        {
            if (rb != null)
            {
                rb.linearVelocity = initialVelocity;
                rb.angularVelocity = initialAngularVelocity;
            }
            applied = true; // 한 번만 적용
        }
    }
}
