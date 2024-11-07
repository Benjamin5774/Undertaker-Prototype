using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // 要销毁的对象
    [SerializeField] private GameObject player;       // 指定的玩家

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的对象是否为指定的玩家
        if (other.gameObject == player)
        {
            Destroy(targetObject); // 销毁目标对象
        
        }
    }
}
