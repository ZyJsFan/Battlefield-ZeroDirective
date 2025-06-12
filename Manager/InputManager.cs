using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static event Action<Vector2> OnDragStart;
    public static event Action<Vector2> OnDrag;
    public static event Action<Vector2> OnDragEnd;
    public static event Action<RaycastHit> OnCommandIssued;

    // 掩码：排除 “Region” 这一层
    private int commandMask;

    void Awake()
    {
        // 确保你的 Region Layer 名字跟这里一致
        commandMask = ~LayerMask.GetMask("Region");
        Debug.Log($"[InputManager] commandMask = {commandMask}");
    }

    void Update()
    {
        // 左键框选
        if (Input.GetMouseButtonDown(0)) OnDragStart?.Invoke(Input.mousePosition);
        else if (Input.GetMouseButton(0)) OnDrag?.Invoke(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) OnDragEnd?.Invoke(Input.mousePosition);

        // 右键命令
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 带上 commandMask，Region 层的 Collider 会被排除
            if (Physics.Raycast(ray, out var hit, 1000f, commandMask))
            {
                Debug.Log($"[InputManager] OnCommandIssued hit `{hit.collider.name}` at {hit.point}");
                OnCommandIssued?.Invoke(hit);
            }
            else
            {
                Debug.Log("[InputManager] Raycast hit nothing (Region layer excluded)");
            }
        }
    }
}
