using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class LargeArea : MonoBehaviour
{
    [Tooltip("�ô�����������������1�C3 ����")]
    public AreaRegion[] subRegions;

    /// <summary>��ͬһ��Ӫ����������������ʱ����</summary>
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

    /// <summary>��ǰ����������ȣ�0�C1��</summary>
    public float Progress => subRegions.Average(r => r.Progress / r.maxProgress);
}
