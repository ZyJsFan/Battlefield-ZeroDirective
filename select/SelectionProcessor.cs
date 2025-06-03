using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 简化版 SelectionProcessor，跳过任何粗筛，直接遍历全场所有 Selectable 进行框选。
/// </summary>
public class SelectionProcessor : MonoBehaviour
{
    public static SelectionProcessor Instance;

    [Header("无粗筛，全场遍历时不需要设置 LayerMask")]
    // public LayerMask unitLayer; // 已移除

    private List<Selectable> previouslySelected = new List<Selectable>();
    public List<Selectable> CurrentlySelected { get; private set; } = new List<Selectable>();

    private Camera mainCam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }
        Debug.Log("[SelectionProcessor] Awake: 全场遍历模式");
    }

    /// <summary>
    /// 由外部注册的本地玩家摄像机，用于所有坐标转换。
    /// </summary>
    public void SetCamera(Camera cam)
    {
        mainCam = cam;
        Debug.Log($"[SelectionProcessor] 注册 mainCam = {cam?.name}");
    }

    /// <summary>
    /// 框选入口，遍历所有 Selectable，进行屏幕投影测试并更新选中状态。
    /// </summary>
    public void ProcessScreenRect(Vector2 screenStart, Vector2 screenEnd)
    {
        Debug.Log($"[SelectionProcessor] ProcessScreenRect called start={screenStart}, end={screenEnd}");

        if (mainCam == null)
        {
            Debug.LogWarning("[SelectionProcessor] mainCam 为空，无法执行框选");
            return;
        }

        // 1. 构造屏幕空间矩形
        float xMin = Mathf.Min(screenStart.x, screenEnd.x);
        float yMin = Mathf.Min(screenStart.y, screenEnd.y);
        float width = Mathf.Abs(screenStart.x - screenEnd.x);
        float height = Mathf.Abs(screenStart.y - screenEnd.y);
        Rect screenRect = new Rect(xMin, yMin, width, height);
        Debug.Log($"[SelectionProcessor] screenRect = {screenRect}");

        // 2. 全场遍历所有 Selectable
        var allUnits = FindObjectsOfType<Selectable>();
        Debug.Log($"[SelectionProcessor] 全场 Selectable 数量 = {allUnits.Length}");

        // 3. 精确测试 & 归属过滤
        List<Selectable> newSelected = new List<Selectable>();
        foreach (var sel in allUnits)
        {
            
            if (!sel.IsOwnedByLocal)
            {
                Debug.Log($"  - Skip {sel.name} (不属于本地玩家)");
                continue;
            }

            Vector3 sp = mainCam.WorldToScreenPoint(sel.transform.position);
            Debug.Log($"  - {sel.name} screenPos = {sp}");
            if (screenRect.Contains(new Vector2(sp.x, sp.y)))
            {
                Debug.Log($"  - {sel.name} 在框内");
                newSelected.Add(sel);
            }
            else
            {
                Debug.Log($"  - {sel.name} 不在框内");
            }
        }

        // 4. 差分更新
        var removed = previouslySelected.Except(newSelected).ToList();
        var added = newSelected.Except(previouslySelected).ToList();
        Debug.Log($"[SelectionProcessor] newSelected count = {newSelected.Count}, added = {added.Count}, removed = {removed.Count}");

        if (added.Count > 0)
            Debug.Log($"[SelectionProcessor] 新增选中: {string.Join(", ", added.Select(s => s.name))}");
        if (removed.Count > 0)
            Debug.Log($"[SelectionProcessor] 取消选中: {string.Join(", ", removed.Select(s => s.name))}");

        foreach (var sel in removed) sel.SetSelected(false);
        foreach (var sel in added) sel.SetSelected(true);

        // 5. 缓存结果
        previouslySelected = newSelected;
        CurrentlySelected = new List<Selectable>(newSelected);
    }
}