using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar3DController : MonoBehaviour
{
    [SerializeField] private Transform fill;    // 指向 Quad
    [SerializeField] private Gradient gradient; // 红绿渐变

    private Health health;
    private Vector3 origScale;

    void Awake()
    {
        health = GetComponentInParent<Health>();
        if (fill == null) fill = transform.Find("Fill");
        origScale = fill.localScale;
    }

    void Update()
    {
        if (health == null) return;
        float pct = health.CurrentHP / health.maxHp;
        // 按百分比缩放 X 轴
        fill.localScale = new Vector3(origScale.x * pct, origScale.y, origScale.z);
        // 按百分比设置颜色
        var r = fill.GetComponent<Renderer>();
        r.material.color = gradient.Evaluate(pct);
    }
}