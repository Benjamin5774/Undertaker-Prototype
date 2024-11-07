using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SequentialDisplay : MonoBehaviour
{
    [SerializeField] private GameObject player;  // 指定的玩家
    [SerializeField] private GameObject obj1;    // 第一个物体
    [SerializeField] private GameObject obj2;    // 第二个物体
    [SerializeField] private Canvas canvas;      // 画布

    [SerializeField] private Image img1;         // 画布中的第一个图像
    [SerializeField] private Image img2;         // 画布中的第二个图像

    private bool hasTriggered = false;           // 防止多次触发

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的对象是否为指定的玩家，并且尚未触发过
        if (other.gameObject == player && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(DisplaySequence());
        }
    }

    private IEnumerator DisplaySequence()
    {
        // Step 1: 启用 obj1 和 img1
        obj1.SetActive(true);
        canvas.gameObject.SetActive(true);  // 启用整个画布
        img1.gameObject.SetActive(true);
        img2.gameObject.SetActive(false);   // 确保 img2 最初是禁用的

        // 等待 4 秒
        yield return new WaitForSeconds(4f);

        // Step 2: 停用 obj1 和整个画布
        obj1.SetActive(false);
        canvas.gameObject.SetActive(false);

        // Step 3: 启用 obj2
        obj2.SetActive(true);

        // 等待 2 秒
        yield return new WaitForSeconds(2f);

        // Step 4: 停用 obj2，并启用 img2
        obj2.SetActive(false);
        canvas.gameObject.SetActive(true);  // 启用画布
        img1.gameObject.SetActive(false);   // 确保 img1 被禁用
        img2.gameObject.SetActive(true);    // 启用 img2

           // 等待 2 秒
        yield return new WaitForSeconds(4f);
        img2.gameObject.SetActive(false);   // 确保 img1 被禁用
    }
}
