using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class LargeArea : MonoBehaviour
{
    [Tooltip("该大区域下所有子区域（1–3 个）")]
    public AreaRegion[] subRegions;

    /// <summary>
    /// 当所有 subRegions 都被同一阵营占领时触发
    /// </summary>
    public event Action<LargeArea> OnFullyCaptured;

    private HashSet<AreaRegion> captured = new HashSet<AreaRegion>();

    private void Awake()
    {
        foreach (var r in subRegions)
            r.OnCaptured += HandleSubCaptured;
    }

    private void HandleSubCaptured(AreaRegion region, Faction byFaction)
    {
        // 只算进攻方占领（假设 Allies 为进攻）
        if (byFaction != Faction.Allies) return;

        captured.Add(region);
        if (captured.Count == subRegions.Length)
            OnFullyCaptured?.Invoke(this);
    }

    /// <summary>
    /// 当前平均占领进度（0–1）
    /// </summary>
    public float Progress => subRegions.Average(r => r.Progress / (float)r.maxProgress);
}
