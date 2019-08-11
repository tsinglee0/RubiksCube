using UnityEngine;

public class Rubik
{
    public Transform root;
    public int cubesOrder { get; private set; }
    public int cubesCount { get; private set; }
    RubikCube[] cubes;

    public Rubik(Transform rubikTrans, int order = 3)
    {
        cubesOrder = Mathf.Max(order, 1);

        if(null == rubikTrans)
        {
            var obj = new GameObject();
            obj.name = string.Format("Rubik[{0}x{0}]_{1}", cubesOrder, obj.GetInstanceID());
            rubikTrans = obj.transform;
        }

        root = rubikTrans;
        CreateCubes();
    }

    public void OnDestry()
    {
        if (null != cubes)
        {
            for (int i = 0; i < cubes.Length; i++)
                cubes[i]?.OnDestry();
        }
        cubes = null;
    }

    private void CreateCubes()
    {
        cubesCount = cubesOrder * cubesOrder * cubesOrder;
        cubes = new RubikCube[cubesCount];

        for (int i = 0; i < cubesCount; i++)
            cubes[i] = new RubikCube(this, i);
    }
}

public class RubikCube
{
    public int cubeID;
    public Rubik rubik;
    public Transform cube;

    public RubikCube(Rubik rubik, int id)
    {
        cubeID = id;
        this.rubik = rubik;
        cube = CreateCubeTrans();
    }

    public void OnDestry()
    {
        Object.Destroy(cube);
    }

    private Transform CreateCubeTrans()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.layer = rubik.root.gameObject.layer;

        var pos = RubikMethods.GetRubikCubePos(rubik.cubesOrder, cubeID);
        var cubeMaterial = RubikMethods.CreateRubikMaterial(rubik.cubesOrder, pos);

        if (Application.isPlaying)
            go.GetComponent<MeshRenderer>().material = cubeMaterial;
        else
            go.GetComponent<MeshRenderer>().sharedMaterial = cubeMaterial;

        go.name = string.Format("Cube-{0}-({1},{2},{3})", cubeID, pos.x, pos.y, pos.z);

        var trans = go.transform;
        trans.SetParent(rubik.root);
        trans.localScale = Vector3.one * 0.98f;
        trans.localRotation = Quaternion.identity;
        trans.localPosition = pos;

        return trans;
    }
}

public class RubikMethods
{
    static Shader _rubikShader;
    static Shader RubikShader
    {
        get
        {
            if (null == _rubikShader)
                _rubikShader = Shader.Find("Custom/Rubik");

            return _rubikShader;
        }
    }

    public static Material CreateRubikMaterial(int cubeOrder, Vector4 cubePos)
    {
        var mat = new Material(RubikShader);
        mat.SetInt("_RubikOrder", cubeOrder);
        mat.SetVector("_RubikPos", cubePos);

        return mat;
    }

    public static Vector3 GetRubikCubePos(int cubeOrder, int cubeId)
    {
        Vector3 pos = Vector3.zero;

        var sideWidth = (cubeOrder - 1) / 2f;
        var layerSize = cubeOrder * cubeOrder;

        pos.y = cubeId / layerSize - sideWidth;
        pos.z = sideWidth - cubeId % layerSize / cubeOrder;
        pos.x = cubeId % cubeOrder - sideWidth;

        return pos;
    }
}