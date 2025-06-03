using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    /// <summary>
    /// 立即对 unit 执行这个命令。
    /// </summary>
    void Execute(UnitController unit);
}
