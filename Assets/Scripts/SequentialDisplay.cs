using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SequentialDisplay : MonoBehaviour
{
    [SerializeField] private GameObject player;  // ָ�������
    [SerializeField] private GameObject obj1;    // ��һ������
    [SerializeField] private GameObject obj2;    // �ڶ�������
    [SerializeField] private Canvas canvas;      // ����

    [SerializeField] private Image img1;         // �����еĵ�һ��ͼ��
    [SerializeField] private Image img2;         // �����еĵڶ���ͼ��

    private bool hasTriggered = false;           // ��ֹ��δ���

    private void OnTriggerEnter(Collider other)
    {
        // ������Ķ����Ƿ�Ϊָ������ң�������δ������
        if (other.gameObject == player && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(DisplaySequence());
        }
    }

    private IEnumerator DisplaySequence()
    {
        // Step 1: ���� obj1 �� img1
        obj1.SetActive(true);
        canvas.gameObject.SetActive(true);  // ������������
        img1.gameObject.SetActive(true);
        img2.gameObject.SetActive(false);   // ȷ�� img2 ����ǽ��õ�

        // �ȴ� 4 ��
        yield return new WaitForSeconds(4f);

        // Step 2: ͣ�� obj1 ����������
        obj1.SetActive(false);
        canvas.gameObject.SetActive(false);

        // Step 3: ���� obj2
        obj2.SetActive(true);

        // �ȴ� 2 ��
        yield return new WaitForSeconds(2f);

        // Step 4: ͣ�� obj2�������� img2
        obj2.SetActive(false);
        canvas.gameObject.SetActive(true);  // ���û���
        img1.gameObject.SetActive(false);   // ȷ�� img1 ������
        img2.gameObject.SetActive(true);    // ���� img2

           // �ȴ� 2 ��
        yield return new WaitForSeconds(4f);
        img2.gameObject.SetActive(false);   // ȷ�� img1 ������
    }
}
