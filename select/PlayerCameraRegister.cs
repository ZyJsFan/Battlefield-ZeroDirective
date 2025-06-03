using Mirror;
using UnityEngine;
using System.Collections;


/// <summary>
/// �������������ʱ�������������ע��� SelectionProcessor��
/// </summary>
public class PlayerCameraRegistrar : NetworkBehaviour
{
    [Tooltip("�������Ԥ�����ϵ������")]
    public Camera playerCamera;

    public override void OnStartLocalPlayer()
    {
        // ֻ�Ա�������������������ע��� SelectionProcessor
        playerCamera.enabled = true;
        if (SelectionProcessor.Instance != null)
            SelectionProcessor.Instance.SetCamera(playerCamera);
    }

    public override void OnStopLocalPlayer()
    {
        // �����������ʱ���������
        if (SelectionProcessor.Instance != null)
            SelectionProcessor.Instance.SetCamera(null);
        playerCamera.enabled = false;
    }
}
