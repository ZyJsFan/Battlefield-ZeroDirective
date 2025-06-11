using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar3DController : MonoBehaviour
{
    [SerializeField] private Transform fill;    // ָ�� Quad
    [SerializeField] private Gradient gradient; // ���̽���

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
        // ���ٷֱ����� X ��
        fill.localScale = new Vector3(origScale.x * pct, origScale.y, origScale.z);
        // ���ٷֱ�������ɫ
        var r = fill.GetComponent<Renderer>();
        r.material.color = gradient.Evaluate(pct);
    }
}