using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    private void OnEnable()
    {
        // 订阅拖拽开始／过程／结束事件
        InputManager.OnDragStart += LogDragStart;
        InputManager.OnDrag += LogDragging;
        InputManager.OnDragEnd += LogDragEnd;

        // 订阅右键命令事件
        InputManager.OnCommandIssued += LogCommandIssued;
    }

    private void OnDisable()
    {
        InputManager.OnDragStart -= LogDragStart;
        InputManager.OnDrag -= LogDragging;
        InputManager.OnDragEnd -= LogDragEnd;
        InputManager.OnCommandIssued -= LogCommandIssued;
    }

    private void LogDragStart(Vector2 screenPos)
    {
       // Debug.Log($"[DebugLogger] 拖拽开始 at {screenPos}");
    }

    private void LogDragging(Vector2 screenPos)
    {
       // Debug.Log($"[DebugLogger] 拖拽中 at {screenPos}");
    }

    private void LogDragEnd(Vector2 screenPos)
    {
        //Debug.Log($"[DebugLogger] 拖拽结束 at {screenPos}");
    }

    private void LogCommandIssued(RaycastHit hit)
    {
        var hitPoint = hit.point;
        var hitName = hit.collider != null ? hit.collider.name : "None";
        Debug.Log($"[DebugLogger] 命令 issued at {hitPoint}, hit object: {hitName}");
    }
}
