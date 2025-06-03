using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCommand : ICommand
{
    public GameObject Target { get; }
    public AttackCommand(GameObject target) => Target = target;

    public void Execute(UnitController unit)
    {
        unit.StartAttack(Target);
    }
}

