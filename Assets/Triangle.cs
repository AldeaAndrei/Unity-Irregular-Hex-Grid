using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Triangle
{
    public int combinedRef;
    public int selfRef;
    public GameObject triangleObject;
    public Color color;
    public HexPoint A, B, C;


    public Triangle(int selfRef, HexPoint A, HexPoint B, HexPoint C)
    {
        this.selfRef = selfRef;
        this.A = A;
        this.B = B;
        this.C = C;
        this.combinedRef = -1;

        color = SelectRandomColor();
    }

    public Color SelectRandomColor()
    {
        string[] hexColors = new string[]
        {
            "#626F47", "#A4B465", "#F5ECD5", "#F0BB78", "#5D8736", "#809D3C",
            "#A9C46C", "#F4FFC3", "#706D54", "#A08963", "#C9B194", "#DBDBDB"
        };

        string hex = hexColors[Random.Range(0, hexColors.Length)];
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
            return color;

        return Color.white;
    }

    public bool HasPoint(HexPoint point)
    {
        return (A.Equals(point) || B.Equals(point) || C.Equals(point));
    }

    public bool HasCommonEdge(Triangle other)
    {
        int commonEdges = 0;

        if ((A.Equals(other.A) && B.Equals(other.B)) || (A.Equals(other.B) && B.Equals(other.A))) commonEdges++;
        if ((A.Equals(other.B) && B.Equals(other.C)) || (A.Equals(other.C) && B.Equals(other.B))) commonEdges++;
        if ((A.Equals(other.C) && B.Equals(other.A)) || (A.Equals(other.A) && B.Equals(other.C))) commonEdges++;

        if ((B.Equals(other.A) && C.Equals(other.B)) || (B.Equals(other.B) && C.Equals(other.A))) commonEdges++;
        if ((B.Equals(other.B) && C.Equals(other.C)) || (B.Equals(other.C) && C.Equals(other.B))) commonEdges++;
        if ((B.Equals(other.C) && C.Equals(other.A)) || (B.Equals(other.A) && C.Equals(other.C))) commonEdges++;

        if ((C.Equals(other.A) && A.Equals(other.B)) || (C.Equals(other.B) && A.Equals(other.A))) commonEdges++;
        if ((C.Equals(other.B) && A.Equals(other.C)) || (C.Equals(other.C) && A.Equals(other.B))) commonEdges++;
        if ((C.Equals(other.C) && A.Equals(other.A)) || (C.Equals(other.A) && A.Equals(other.C))) commonEdges++;

        return commonEdges > 0;
    }

    public void ColorObject(Color newColor)
    {
        MeshRenderer meshRenderer = triangleObject.GetComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Standard"));
        material.color = newColor;
        meshRenderer.material = material;
    }

    public void SetPoints(Vector3 positionA, Vector3 positionB, Vector3 positionC)
    {
        A.position = positionA;
        B.position = positionB;
        C.position = positionC;

        if (triangleObject != null)
            UpdateMesh();
    }

    public GameObject SpawnObject()
    {
        triangleObject = new GameObject("Triangle " + selfRef);

        MeshFilter meshFilter = triangleObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = triangleObject.AddComponent<MeshRenderer>();

        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        meshRenderer.material = material;

        UpdateMesh();

        return triangleObject;
    }

    private void UpdateMesh()
    {
        if (triangleObject == null) return;

        MeshFilter meshFilter = triangleObject.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[3] { A.position, B.position, C.position };
        mesh.triangles = new int[3] { 0, 2, 1 };
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
