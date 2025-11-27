using UnityEngine;
using System.Collections.Generic;

public class ForceSender : MonoBehaviour
{
    public List<float> History = new List<float>();
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // 힘 = 질량 * 가속도 (혹은 원하는 계산 방식)
        float forceMag = rb.mass * rb.linearVelocity.magnitude;
        History.Add(forceMag);

        // 과도한 메모리 증가 방지 (선택)
        if (History.Count > 2000)
            History.RemoveRange(0, History.Count - 2000);
    }
}
