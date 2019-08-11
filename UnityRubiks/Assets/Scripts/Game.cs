using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int cubeOrder;

    private void Start()
    {
        new Rubik(null);
    }

    [ContextMenu("Create Rubik")]
    void CreateRubik()
    {
        new Rubik(null, cubeOrder);
    }

    [ContextMenu("Resolve Rubik")]
    void ResolveRubik()
    {
        //Completed Rubik "UF UR UB UL DF DR DB DL FR FL BR BL UFR URB UBL ULF DRF DFL DLB DBR"
        var result = RubikSolver.Jaap.GetResult("RU LF UB DR DL BL UL FU BD RF BR FD LDF LBD FUL RFD UFR RDB UBL RBU");
        Debug.LogWarning(string.IsNullOrEmpty(result) ? "rubik is already completed" : result);
    }
}
