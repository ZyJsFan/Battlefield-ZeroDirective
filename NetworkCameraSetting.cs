using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NetworkCameraHelper : MonoBehaviour
{
    [Header("ָ��һ���Ѿ���Ϊ Overlay �� UI Camera")]
    public Camera uiCamera;

    private void Awake()
    {
        if (uiCamera == null)
            Debug.LogError("[NetworkCameraHelper] ���� Inspector ��������� UI Camera!");
    }

    private void Start()
    {
        // �첽�ȴ��������������������ջ���
        StartCoroutine(SetupCameraStack());
    }

    private IEnumerator SetupCameraStack()
    {
        // �ȵ� Mirror/������ MainCamera��Base ���ͣ�����������
        yield return new WaitUntil(() => Camera.main != null);

        // �õ� Base �����
        Camera baseCam = Camera.main;
        var baseData = baseCam.GetUniversalAdditionalCameraData();
        var uiData = uiCamera.GetUniversalAdditionalCameraData();

        // ȷ�� UI Camera ����Ⱦ������ Overlay
        uiData.renderType = CameraRenderType.Overlay;

        // �����û���룬����ӵ���ջĩβ
        if (!baseData.cameraStack.Contains(uiCamera))
            baseData.cameraStack.Add(uiCamera);

        Debug.Log("[NetworkCameraHelper] �ѽ� UI Camera ��ӵ� Base ������� URP ��ջ");
    }
}
