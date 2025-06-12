using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

public class AreaRegion : NetworkBehaviour
{
    [Header("区域配置")]
    public float captureRate = 10f;
    public float recaptureRate = 15f;
    public float maxProgress = 100f;

    // —— 这是网络同步的进度变量 —— 
    [SyncVar(hook = nameof(OnNetworkProgressChanged))]
    private float progressVar = 0f;

    private bool wasCaptured = false;
    private HashSet<Selectable> inAllies = new();
    private HashSet<Selectable> inAxis = new();

    /// <summary>
    /// UI 拉取的进度来源。现在读的是同步变量 progressVar。
    /// </summary>
    public float Progress => progressVar;


    public void ResetState()
    {
        progressVar = 0f;
        wasCaptured = false;
        inAllies.Clear();
        inAxis.Clear();
        Debug.Log($"[AreaRegion:{name}] ResetState called");
        // 如果 UI 已绑定 OnProgressChanged 的话，可以主动推一次
        // OnProgressChanged?.Invoke(progress);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!other.TryGetComponent<Selectable>(out var sel)) return;

        var faction = GameFlowManager.Instance
                       .GetFactionForConnection(sel.GetComponent<NetworkIdentity>().connectionToClient);

        if (faction == Faction.Allies) { inAllies.Add(sel); Debug.Log($"[AreaRegion:{name}] Allies entered"); }
        else if (faction == Faction.Axis) { inAxis.Add(sel); Debug.Log($"[AreaRegion:{name}] Axis entered"); }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isServer) return;
        if (!other.TryGetComponent<Selectable>(out var sel)) return;

        if (inAllies.Remove(sel)) Debug.Log($"[AreaRegion:{name}] Allies exited");
        if (inAxis.Remove(sel)) Debug.Log($"[AreaRegion:{name}] Axis exited");
    }

    /// <summary>
    /// 服务器端每帧调用，累加或减少进度，然后把新值写给 progressVar。
    /// </summary>
    [ServerCallback]
    public void UpdateProgress(float dt)
    {
        if (maxProgress <= 0f) return;  // 防止 0 导致一开始就触发

        int atk = inAllies.Count, def = inAxis.Count;
        float delta = atk > def
            ? captureRate * dt
            : (def > atk ? -recaptureRate * dt : 0f);

        float newProg = Mathf.Clamp(progressVar + delta, 0f, maxProgress);
        progressVar = newProg;  // 赋值给 SyncVar，Mirror 会自动推给客户端

        // 只在服务器端本地检测“满值”来触发捕获
        if (!wasCaptured && newProg >= maxProgress)
        {
            wasCaptured = true;
            Debug.Log($"[AreaRegion:{name}] >>> OnCaptured!");
            OnCaptured?.Invoke(this, Faction.Allies);
        }
    }

    /// <summary>
    /// 当进度通过网络推给客户端时（客户端触发）会调用这里
    /// </summary>
    void OnNetworkProgressChanged(float oldVal, float newVal)
    {
        // 可选：打个日志确认客户端收到了
        Debug.Log($"[AreaRegion:{name}] [Client] Progress updated → {newVal}");
    }

    // 你原来的事件，UI 可以继续订阅
    public event Action<AreaRegion, Faction> OnCaptured;
    public event Action<AreaRegion, Faction> OnLost;
}
