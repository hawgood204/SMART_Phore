using UnityEngine;

// 한 Rigidbody에 걸리는 합력(F)와 토크(τ)를 측정하는 스크립트임
public class PhysicsMonitor : MonoBehaviour
{
    public Rigidbody rb;          // 측정할 rigidbody
    public Vector3 totalForce;    // 이번 FixedUpdate에서의 합력 [N]
    public Vector3 torque;        // 이번 FixedUpdate에서의 토크 [N·m]

    private Vector3 prevVelocity;
    private Vector3 prevAngularVelocity;

    void Start()
    {
        // 같은 오브젝트에 붙은 Rigidbody 자동 참조
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        prevVelocity = rb.linearVelocity;
        prevAngularVelocity = rb.angularVelocity;
    }

    void FixedUpdate()
    {
        // 1) 합력 F = m * a  (a = dv/dt)
        Vector3 acc = (rb.linearVelocity - prevVelocity) / Time.fixedDeltaTime;
        totalForce = rb.mass * acc;

        // 2) 토크 τ ≈ I ⊙ α  (대각선 관성텐서 가정, α = dω/dt)
        Vector3 angAcc = (rb.angularVelocity - prevAngularVelocity) / Time.fixedDeltaTime;
        Vector3 I = rb.inertiaTensor;
        torque = new Vector3(I.x * angAcc.x, I.y * angAcc.y, I.z * angAcc.z);

        prevVelocity = rb.linearVelocity;
        prevAngularVelocity = rb.angularVelocity;
    }
}
