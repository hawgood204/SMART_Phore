using UnityEngine;

public class LaunchMotion : MonoBehaviour
{
    public Vector3 initialVelocity = new Vector3(2f, 0f, 0f);
    public Vector3 initialAngularVelocity = Vector3.zero;
    public float delay = 0.5f;

    private Rigidbody rb;
    private MeshFilter mf;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mf = GetComponent<MeshFilter>();

        // COM을 메시 중심으로 설정
        if (mf != null && mf.sharedMesh != null)
        {
            rb.centerOfMass = mf.sharedMesh.bounds.center;
        }

        Invoke(nameof(SetInitialMotion), delay);
    }

    void SetInitialMotion()
    {
        rb.WakeUp();

        // "힘"이 아니라 "이미 갖고 있던 속도"처럼 설정
        rb.linearVelocity = initialVelocity;
        rb.angularVelocity = initialAngularVelocity;

        Debug.Log("[Velocity Applied] Now moving naturally.");
    }
}
