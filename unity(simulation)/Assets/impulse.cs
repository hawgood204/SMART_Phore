using UnityEngine;

// 이 오브젝트가 충돌할 때의 접촉 힘을 측정하는 스크립트임
public class CollisionForceMonitor : MonoBehaviour
{
    public float contactForce;   // N 단위 대략적인 충돌 힘
    public Vector3 contactImpulse; // J 단위 충격량

    void OnCollisionStay(Collision collision)
    {
        // impulse: 이 FixedUpdate 동안 교환된 총 충격량 (벡터)
        contactImpulse = collision.impulse;

        // 평균 힘: F ≈ J / Δt
        contactForce = contactImpulse.magnitude / Time.fixedDeltaTime;
    }
}
