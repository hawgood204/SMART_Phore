using UnityEngine;
using System.Collections.Generic;

public class ForceAndImpulseMonitor : MonoBehaviour
{
    public Rigidbody rb;
    public List<float> forceHistory = new List<float>();
    public List<float> impulseHistory = new List<float>();

    public int maxSamples = 300;

    Vector3 prevVelocity;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        prevVelocity = rb.linearVelocity;
    }

    void FixedUpdate()
    {
        // 1) 합력 = m * a
        Vector3 a = (rb.linearVelocity - prevVelocity) / Time.fixedDeltaTime;
        float force = (rb.mass * a).magnitude;
        prevVelocity = rb.linearVelocity;

        // 저장
        forceHistory.Add(force);
        if (forceHistory.Count > maxSamples)
            forceHistory.RemoveAt(0);
    }

    void OnCollisionStay(Collision col)
    {
        // impulse는 Vector3
        float impulse = col.impulse.magnitude;
        float force = impulse / Time.fixedDeltaTime;

        // impulse 기록
        impulseHistory.Add(force);
        if (impulseHistory.Count > maxSamples)
            impulseHistory.RemoveAt(0);
    }
}
