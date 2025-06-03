using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    public Vector3 TargetPosition { get; }
    public MoveCommand(Vector3 pos) => TargetPosition = pos;

    public void Execute(UnitController unit)
    {
        unit.StartMove(TargetPosition);
    }
}
