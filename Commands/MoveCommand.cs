using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    public Vector3 TargetPosition { get; }
    public MoveCommand(Vector3 pos) => TargetPosition = pos;

    public void Execute(UnitController unit)
    {
        Debug.Log($"[MoveCommand] Execute called on unit={unit.name}, isServer={unit.isServer}, hasAuthority={unit.authority}");
        unit.StartMove(TargetPosition);
    }
}
