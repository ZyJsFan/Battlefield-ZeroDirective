using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class LargeArea : MonoBehaviour
{
    [Tooltip("该大区域下所有子区域（1–3 个）")]
    public AreaRegion[] subRegions;

    /// <summary>当同一阵营捕获完所有子区域时触发</summary>
    public event Action<LargeArea> OnFullyCaptured;

    private HashSet<AreaRegion> captured = new();

       public void ResetState()
   {
       captured.Clear();
       Debug.Log($"[LargeArea:{name}] ResetState: captured cleared");
   }

private void Awake()
    {
        foreach (var reg in subRegions)
            reg.OnCaptured += HandleSubCaptured;
    }

    private void HandleSubCaptured(AreaRegion region, Faction byFaction)
    {
        if (byFaction != Faction.Allies) return;
        captured.Add(region);
        Debug.Log($"[LargeArea:{name}] Captured sub {region.name}, total={captured.Count}/{subRegions.Length}");
        if (captured.Count == subRegions.Length)
            OnFullyCaptured?.Invoke(this);
    }

    /// <summary>当前大区整体进度（0–1）</summary>
    public float Progress => subRegions.Average(r => r.Progress / r.maxProgress);
}
