using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;   // 따라갈 오브젝트
    public Vector3 offset;     // 카메라와 오브젝트 사이 거리

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        transform.LookAt(target);  // 항상 대상 바라보기
    }
}
