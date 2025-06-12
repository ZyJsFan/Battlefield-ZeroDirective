using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEditor;

public class AreaRegion : NetworkBehaviour
{
    [Header("区域设置")]
    public float captureRate = 10f;
    public float recaptureRate = 15f;
    public float maxProgress = 100f;

    /// <summary>进度变化事件（传入当前进度）</summary>
    public event Action<float> OnProgressChanged;
    /// <summary>首次达到 maxProgress 时触发（由进攻方捕获）</summary>
    public event Action<AreaRegion, Faction> OnCaptured;
    /// <summary>首次从 maxProgress 回退到 0 时触发（由防守方复原）</summary>
    public event Action<AreaRegion, Faction> OnLost;

    private float progress = 0f;
    private bool wasCaptured = false;
    private HashSet<Selectable> inAllies = new();
    private HashSet<Selectable> inAxis = new();

    /// <summary>当前进度，用于 UI 初始化</summary>
    public float Progress => progress;


       public void ResetState()
   {
       progress = 0f;
       wasCaptured = false;
       inAllies.Clear();
       inAxis.Clear();
       Debug.Log($"[AreaRegion:{name}] ResetState: progress=0, wasCaptured=false");
      // 如果 UI 已经订阅了事件，主动推一遍进度（对应 BuildSubRegionRows 初始化）
       OnProgressChanged?.Invoke(progress);
  }

void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!other.TryGetComponent<Selectable>(out var sel)) return;

        var conn = sel.GetComponent<NetworkIdentity>().connectionToClient;
        var faction = GameFlowManager.Instance.GetFactionForConnection(conn);

        if (faction == Faction.Allies)
        {
            inAllies.Add(sel);
            Debug.Log($"[AreaRegion:{name}] Allies entered, Att={inAllies.Count}, Def={inAxis.Count}");
        }
        else if (faction == Faction.Axis)
        {
            inAxis.Add(sel);
            Debug.Log($"[AreaRegion:{name}] Axis entered,  Att={inAllies.Count}, Def={inAxis.Count}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isServer) return;
        if (other.TryGetComponent<Selectable>(out var sel))
        {
            if (inAllies.Remove(sel))
                Debug.Log($"[AreaRegion:{name}] Allies exited,  Att={inAllies.Count}, Def={inAxis.Count}");
            if (inAxis.Remove(sel))
                Debug.Log($"[AreaRegion:{name}] Axis exited,   Att={inAllies.Count}, Def={inAxis.Count}");
        }
    }

    /// <summary>由 AreaControlSystem 每帧调用，累加或减少进度</summary>
    public void UpdateProgress(float dt)
    {
        int atk = inAllies.Count, def = inAxis.Count;
        Debug.Log($"[AreaRegion:{name}] BeforeUpdate: Att={atk}, Def={def}, Progress={progress:F1}");

        float delta = atk > def
            ? captureRate * dt
            : (def > atk ? -recaptureRate * dt : 0f);
        float newProgress = Mathf.Clamp(progress + delta, 0f, maxProgress);

        if (Mathf.Abs(newProgress - progress) > 0.01f)
        {
            progress = newProgress;
            Debug.Log($"[AreaRegion:{name}] ProgressChanged -> {progress:F1}");
            OnProgressChanged?.Invoke(progress);
        }
        else progress = newProgress;

        Debug.Log($"[AreaRegion:{name}] AfterUpdate:  Att={atk}, Def={def}, Progress={progress:F1}");

        if (!wasCaptured && progress >= maxProgress)
        {
            wasCaptured = true;
            Debug.Log($"[AreaRegion:{name}] >>> OnCaptured!");
            OnCaptured?.Invoke(this, Faction.Allies);
        }
        else if (wasCaptured && progress <= 0f)
        {
            wasCaptured = false;
            Debug.Log($"[AreaRegion:{name}] >>> OnLost!");
            OnLost?.Invoke(this, Faction.Allies);
        }
    }
}
