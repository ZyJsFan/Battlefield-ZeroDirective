using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    /// <summary>
    /// ������ unit ִ��������
    /// </summary>
    void Execute(UnitController unit);
}
