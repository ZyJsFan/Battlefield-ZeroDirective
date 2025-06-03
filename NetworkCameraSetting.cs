using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NetworkCameraHelper : MonoBehaviour
{
    [Header("指定一个已经置为 Overlay 的 UI Camera")]
    public Camera uiCamera;

    private void Awake()
    {
        if (uiCamera == null)
            Debug.LogError("[NetworkCameraHelper] 请在 Inspector 里拖入你的 UI Camera!");
    }

    private void Start()
    {
        // 异步等待主摄像机就绪后，再做堆栈添加
        StartCoroutine(SetupCameraStack());
    }

    private IEnumerator SetupCameraStack()
    {
        // 等到 Mirror/场景把 MainCamera（Base 类型）创建并激活
        yield return new WaitUntil(() => Camera.main != null);

        // 拿到 Base 摄像机
        Camera baseCam = Camera.main;
        var baseData = baseCam.GetUniversalAdditionalCameraData();
        var uiData = uiCamera.GetUniversalAdditionalCameraData();

        // 确保 UI Camera 的渲染类型是 Overlay
        uiData.renderType = CameraRenderType.Overlay;

        // 如果还没加入，就添加到堆栈末尾
        if (!baseData.cameraStack.Contains(uiCamera))
            baseData.cameraStack.Add(uiCamera);

        Debug.Log("[NetworkCameraHelper] 已将 UI Camera 添加到 Base 摄像机的 URP 堆栈");
    }
}
