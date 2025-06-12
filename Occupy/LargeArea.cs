using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class LargeArea : MonoBehaviour
{
    [Tooltip("�ô�����������������1�C3 ����")]
    public AreaRegion[] subRegions;

    /// <summary>
    /// ������ subRegions ����ͬһ��Ӫռ��ʱ����
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
        // ֻ�������ռ�죨���� Allies Ϊ������
        if (byFaction != Faction.Allies) return;

        captured.Add(region);
        if (captured.Count == subRegions.Length)
            OnFullyCaptured?.Invoke(this);
    }

    /// <summary>
    /// ��ǰƽ��ռ����ȣ�0�C1��
    /// </summary>
    public float Progress => subRegions.Average(r => r.Progress / (float)r.maxProgress);
}
