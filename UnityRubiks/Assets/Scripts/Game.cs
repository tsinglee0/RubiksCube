using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int cubeOrder;

    [ContextMenu("Create Rubik")]
    void CreateRubik()
    {
        //new Rubik(null, cubeOrder);
        var result = RubikSolver.Jaap.GetResult("RU LF UB DR DL BL UL FU BD RF BR FD LDF LBD FUL RFD UFR RDB UBL RBU");
        Debug.LogWarning(result);
    }
}
