using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AreaRegion : NetworkBehaviour
{
    [Header("参数")]
    public float captureRate = 10f;
    public float recaptureRate = 15f;
    public float maxProgress = 100f;



    public event Action<AreaRegion, Faction> OnCaptured;
    public event Action<AreaRegion, Faction> OnLost;

    float progress = 0f;
    bool wasCaptured = false;

    HashSet<Selectable> inAllies = new();
    HashSet<Selectable> inAxis = new();

    void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;   // 只在服务器端统计
        if (!other.TryGetComponent<Selectable>(out var sel)) return;

        var ni = sel.GetComponent<NetworkIdentity>();
        var conn = ni.connectionToClient;
        var faction = GameFlowManager.Instance.GetFactionForConnection(conn);

        if (faction == Faction.Allies)
        {
            inAllies.Add(sel);
            Debug.Log($"[AreaRegion:{name}] +Allies {sel.name} → Att={inAllies.Count}, Def={inAxis.Count}");
        }
        else if (faction == Faction.Axis) {
            inAxis.Add(sel);
            Debug.Log($"[AreaRegion:{name}] +Axis {sel.name} → Att={inAllies.Count}, Def={inAxis.Count}");
        } 
    }

    void OnTriggerExit(Collider other)
    {
        if (!isServer) return;
        if (other.TryGetComponent<Selectable>(out var sel)) {
            if (inAllies.Remove(sel))
            {
                Debug.Log($"[AreaRegion:{name}] –Allies {sel.name} → Att={inAllies.Count}, Def={inAxis.Count}");
            }
            if (inAxis.Remove(sel))
            {
                Debug.Log($"[AreaRegion:{name}] –Axis {sel.name} → Att={inAllies.Count}, Def={inAxis.Count}");
            }
        }


    }

    /// <summary>
    /// 由 AreaControlSystem 每帧调用
    /// </summary>
    public void UpdateProgress(float dt)
    {
        int atk = inAllies.Count;
        int def = inAxis.Count;

        Debug.Log($"[AreaRegion:{name}] Before: Att={atk}, Def={def}, Progress={progress:F1}");
        if (atk > def) progress += captureRate * dt;
        else if (def > atk) progress -= recaptureRate * dt;

        progress = Mathf.Clamp(progress, 0f, maxProgress);

        Debug.Log($"[AreaRegion:{name}] After:  Att={atk}, Def={def}, Progress={progress:F1}");


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
