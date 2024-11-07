using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // Ҫ���ٵĶ���
    [SerializeField] private GameObject player;       // ָ�������

    private void OnTriggerEnter(Collider other)
    {
        // ������Ķ����Ƿ�Ϊָ�������
        if (other.gameObject == player)
        {
            Destroy(targetObject); // ����Ŀ�����
        
        }
    }
}
