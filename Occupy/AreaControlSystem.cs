using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaControlSystem : MonoBehaviour
{
    AreaRegion[] regions;

    void Awake()
    {
        regions = FindObjectsOfType<AreaRegion>();
        Debug.Log($"[AreaControlSystem] Found {regions.Length} regions");
    }

    void Update()
    {
        float dt = Time.deltaTime;
        foreach (var r in regions)
            r.UpdateProgress(dt);
    }
}