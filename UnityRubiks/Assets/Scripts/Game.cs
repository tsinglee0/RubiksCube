using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int cubeOrder;

    [ContextMenu("Create Rubik")]
    void CreateRubik()
    {
        new Rubik(null, cubeOrder);
    }
}
