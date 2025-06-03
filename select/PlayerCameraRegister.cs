using Mirror;
using UnityEngine;
using System.Collections;


/// <summary>
/// 当本地玩家启动时，把它的摄像机注册给 SelectionProcessor。
/// </summary>
public class PlayerCameraRegistrar : NetworkBehaviour
{
    [Tooltip("拖入玩家预制体上的摄像机")]
    public Camera playerCamera;

    public override void OnStartLocalPlayer()
    {
        // 只对本地玩家启用摄像机，并注册给 SelectionProcessor
        playerCamera.enabled = true;
        if (SelectionProcessor.Instance != null)
            SelectionProcessor.Instance.SetCamera(playerCamera);
    }

    public override void OnStopLocalPlayer()
    {
        // 本地玩家销毁时，清除引用
        if (SelectionProcessor.Instance != null)
            SelectionProcessor.Instance.SetCamera(null);
        playerCamera.enabled = false;
    }
}
