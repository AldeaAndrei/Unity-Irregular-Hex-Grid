using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GameManager : MonoBehaviour
{
    private int triangleCount = 0;


    [Header("Simulation Params")]
    public int rows = 7;
    public int adjustmentSteps = 100;
    public float minTriangleSize = 0.25f;

    [Header("Simulation Steps")]
    public bool spawnedPoints;
    public bool spawnedTriangles;
    public bool mergedTriangles;
    public bool adjustedTriangles;

    // Lists
    private List<Triangle> triangles;
    private List<List<Vector3>> pointsByRow;
    private List<List<Vector3>> pointsByRowChange;
    private List<List<int>> pointsByRowCount;
    private List<List<HexPoint>> quads;
    private List<GameObject> spheres;

    // Counters
    private int currentRow = 0;
    private int currentPoint = 0;
    private int currentTriangle = 0;
    private int currentAdjustment = 0;

    public void InitLists()
    {
        pointsByRow = new List<List<Vector3>>();
        triangles = new List<Triangle>();
        quads = new List<List<HexPoint>>();
        spheres = new List<GameObject>();
    }

    public void InitChangesLists()
    {
        pointsByRowChange = new List<List<Vector3>>();
        pointsByRowCount = new List<List<int>>();
        for (int i = 0; i < pointsByRow.Count; i++)
        {
            pointsByRowChange.Add(new List<Vector3>());
            pointsByRowCount.Add(new List<int>());
            for (int j = 0; j < pointsByRow[i].Count; j++)
            {
                pointsByRowChange[i].Add(new Vector3(0, 0, 0));
                pointsByRowCount[i].Add(0);
            }
        }
    }

    void Start()
    {
        Application.targetFrameRate = 120;

        InitLists();

        SpawnAtAngle();
        GenerateTriangles();

        InitChangesLists();
    }

    public void SpawnPoints()
    {
        if (spawnedPoints) return;
        if (currentRow < rows && currentPoint < pointsByRow[currentRow].Count)
        {
            // Bigger points for main axis
            float f = 0.1f;
            if (currentRow > 0 && currentPoint % currentRow == 0) f = 0.15f;

            SpawnSphere(pointsByRow[currentRow][currentPoint], f);

            currentPoint++;

            if (currentPoint >= pointsByRow[currentRow].Count)
            {
                currentPoint = 0;
                currentRow++;
            }
        }
        else
        {
            Debug.Log("Spawned Points!");
            spawnedPoints = true;
            return;
        }
    }

    public void SpawnTriangles()
    {
        if (!spawnedPoints) return;
        if (spawnedTriangles) return;
        if (triangleCount >= triangles.Count)
        {
            Debug.Log("Spawned Triangles!");
            spawnedTriangles = true;
            return;
        }

        triangles[triangleCount].SpawnObject();
        triangleCount++;
    }

    public void MergeTriangles()
    {
        if (!spawnedTriangles) return;
        if (mergedTriangles) return;
        if (currentTriangle >= triangles.Count)
        {
            Debug.Log("Merged Triangles!");
            mergedTriangles = true;
            return;
        }

        ColorCommonEdges(triangles[currentTriangle]);
        currentTriangle++;
    }

    public void AdjustTriangles()
    {
        if (!mergedTriangles) return;
        if (adjustedTriangles) return;
        if (currentAdjustment >= adjustmentSteps)
        {
            Debug.Log("Adjusted Triangles!");
            adjustedTriangles = true;
            return;
        }
        {
            if (spheres != null) DestroyDebugSpheres();

            AdjustTrianglesShapes();
            currentAdjustment++;
        }
    }

    public void DestroyDebugSpheres()
    {
        for (int i = 0; i < spheres.Count; i++)
        {
            Destroy(spheres[i]);
        }

        spheres = null;
    }

    void Update()
    {
        SpawnPoints();
        SpawnTriangles();
        MergeTriangles();
        AdjustTriangles();
    }

    public void AdjustTrianglesShapes()
    {
        for (int i = 0; i < pointsByRow.Count; i++)
        {
            for (int j = 0; j < pointsByRow[i].Count; j++)
            {
                pointsByRowChange[i][j] = new Vector3(0, 0, 0);
                pointsByRowCount[i][j] = 0;
            }
        }

        for (int i = 0; i < quads.Count; i++)
        {
            if (quads[i].Count < 4) continue;

            Vector3 a = pointsByRow[quads[i][0].row][quads[i][0].index];
            Vector3 b = pointsByRow[quads[i][1].row][quads[i][1].index];
            Vector3 c = pointsByRow[quads[i][2].row][quads[i][2].index];
            Vector3 d = pointsByRow[quads[i][3].row][quads[i][3].index];

            Vector3 ct = (a + b + c + d) / 4.0f;

            Vector3 dirA = (a - ct).normalized;
            Vector3 dirB = (b - ct).normalized;
            Vector3 dirC = (c - ct).normalized;
            Vector3 dirD = (d - ct).normalized;

            float totalLength = Vector3.Distance(a, b) + Vector3.Distance(b, c) + Vector3.Distance(c, d) + Vector3.Distance(d, a);
            float offset = Mathf.Sqrt(totalLength / 2.0f) / 2.0f;

            if (quads[i][0].row < rows - 1 && Vector3.Distance(ct, pointsByRow[quads[i][0].row][quads[i][0].index] + offset * dirA * 0.005f) <= offset)
            {
                pointsByRowChange[quads[i][0].row][quads[i][0].index] += offset * dirA * 0.005f;
                pointsByRowCount[quads[i][0].row][quads[i][0].index] += 1;
            }
            if (quads[i][1].row < rows - 1 && Vector3.Distance(ct, pointsByRow[quads[i][1].row][quads[i][1].index] + offset * dirB * 0.005f) <= offset)
            {
                pointsByRowChange[quads[i][1].row][quads[i][1].index] += offset * dirB * 0.005f;
                pointsByRowCount[quads[i][1].row][quads[i][1].index] += 1;
            }
            if (quads[i][2].row < rows - 1 && Vector3.Distance(ct, pointsByRow[quads[i][2].row][quads[i][2].index] + offset * dirC * 0.005f) <= offset)
            {
                pointsByRowChange[quads[i][2].row][quads[i][2].index] += offset * dirC * 0.005f;
                pointsByRowCount[quads[i][2].row][quads[i][2].index] += 1;
            }
            if (quads[i][3].row < rows - 1 && Vector3.Distance(ct, pointsByRow[quads[i][3].row][quads[i][3].index] + offset * dirD * 0.005f) <= offset)
            {
                pointsByRowChange[quads[i][3].row][quads[i][3].index] += offset * dirD * 0.005f;
                pointsByRowCount[quads[i][3].row][quads[i][3].index] += 1;
            }
        }

        for (int i = 0; i < pointsByRow.Count; i++)
        {
            for (int j = 0; j < pointsByRow[i].Count; j++)
            {
                if (pointsByRowCount[i][j] == 0) continue;
                pointsByRow[i][j] += pointsByRowChange[i][j] / pointsByRowCount[i][j];
            }
        }

        for (int i = 0; i < triangles.Count; i++)
        {
            Triangle t = triangles[i];

            Vector3 posA = pointsByRow[t.A.row][t.A.index];
            Vector3 posB = pointsByRow[t.B.row][t.B.index];
            Vector3 posC = pointsByRow[t.C.row][t.C.index];

            Vector3 triangleCenter = (t.A.position + t.B.position + t.C.position) / 3.0f;

            Vector3 newAPosition, newBPosition, newCPosition;

            float distA = Vector3.Distance(triangleCenter, posA);
            if (distA > minTriangleSize)
            {
                newAPosition = posA;
            }
            else
            {
                newAPosition = t.A.position;
                pointsByRow[t.A.row][t.A.index] = newAPosition;
            }

            float distB = Vector3.Distance(triangleCenter, posB);
            if (distB > minTriangleSize)
            {
                newBPosition = posB;
            }
            else
            {
                newBPosition = t.B.position;
                pointsByRow[t.B.row][t.B.index] = newBPosition;
            }

            float distC = Vector3.Distance(triangleCenter, posC);
            if (distC > minTriangleSize)
            {
                newCPosition = posC;
            }
            else
            {
                newCPosition = t.C.position;
                pointsByRow[t.C.row][t.C.index] = newCPosition;
            }

            t.SetPoints(newAPosition, newBPosition, newCPosition);
        }

    }

    public void ColorCommonEdges(Triangle t1)
    {
        if (t1.combinedRef != -1) return;
        if (Random.value < 0.5) return;

        for (int j = 0; j < triangles.Count; j++)
        {
            Triangle t2 = triangles[j];
            if (t2.combinedRef != -1) continue;
            if (t1.selfRef == t2.selfRef) continue;

            if (t1.HasCommonEdge(t2))
            {
                t1.color = t2.color;

                t1.ColorObject(t2.color);
                t2.ColorObject(t2.color);

                t1.combinedRef = t2.selfRef;
                t2.combinedRef = t1.selfRef;

                AddQuad(t1, t2);

                break;
            }
        }
    }


    public void AddQuad(Triangle t1, Triangle t2)
    {
        HashSet<HexPoint> uniqueRefs = new HashSet<HexPoint> { t1.A, t1.B, t1.C, t2.A, t2.B, t2.C };

        List<HexPoint> list = uniqueRefs.ToList();
        if (list.Count == 4) quads.Add(list);
    }

    public int NormalizedIndex(int row, int index)
    {
        if (index < 0) return (pointsByRow[row].Count + index) % pointsByRow[row].Count;
        else return index % pointsByRow[row].Count;
    }

    public void AddTriangle(int rowA, int indexA, int rowB, int indexB, int rowC, int indexC)
    {
        Vector3 positionA = pointsByRow[rowA][NormalizedIndex(rowA, indexA)];
        Vector3 positionB = pointsByRow[rowB][NormalizedIndex(rowB, indexB)];
        Vector3 positionC = pointsByRow[rowC][NormalizedIndex(rowC, indexC)];

        HexPoint hPointA = new HexPoint(rowA, NormalizedIndex(rowA, indexA), positionA);
        HexPoint hPointB = new HexPoint(rowB, NormalizedIndex(rowB, indexB), positionB);
        HexPoint hPointC = new HexPoint(rowC, NormalizedIndex(rowC, indexC), positionC);

        bool alreadyAdded = false;

        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].HasPoint(hPointA) && triangles[i].HasPoint(hPointB) && triangles[i].HasPoint(hPointC))
            {
                alreadyAdded = true;
                Debug.LogWarning("Triangle is already added!");

                break;
            }
        }

        if (!alreadyAdded)
        {
            triangles.Add(
                new Triangle(
                    triangles.Count,
                    hPointA, hPointB, hPointC
                )
            );
        }
    }

    public void GenerateTriangles()
    {
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(0, 0, 1, i, 1, (i + 1) % 6);
        }

        for (int r = 1; r < rows - 1; r++)
        {
            for (int i = 0; i <= pointsByRow[r].Count; i++)
            {
                AddTriangle(r + 1, i + (i / r) + 1, r, i + 1, r, i);

                if (i % r == 0)
                {
                    AddTriangle(r + 1, (i / r) * (r + 1) - 1, r + 1, (i / r) * (r + 1), r, i);
                    AddTriangle(r, i, r + 1, (i / r) * (r + 1), r + 1, (i / r) * (r + 1) + 1);
                }
                else
                {
                    AddTriangle(r, i, r + 1, i + (i / r), r + 1, i + (i / r) + 1);
                }
            }
        }
    }

    public void SpawnAtAngle()
    {
        for (int r = 0; r < rows; r++)
        {
            pointsByRow.Add(new List<Vector3>());

            for (float angle = 0.0f; angle < 360.0f; angle += 60.0f)
            {
                float rad = angle * Mathf.Deg2Rad;
                float x = Mathf.Cos(rad);
                float z = Mathf.Sin(rad);

                Vector3 p = new Vector3(r * x, 0, r * z);

                AddPoint(p, r);

                if (r == 0) break;

                if (r > 1 && angle <= 300)
                {
                    float radNext = (angle + 60.0f) * Mathf.Deg2Rad;
                    float xNext = Mathf.Cos(radNext);
                    float zNext = Mathf.Sin(radNext);

                    Vector3 pNext = new Vector3(r * xNext, 0, r * zNext);

                    SpawnMedianPoints(p, pNext, r - 1, r);
                }
            }
        }
    }

    public void SpawnMedianPoints(Vector3 start, Vector3 end, int count, int row)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        for (int i = 1; i <= count; i++)
        {
            Vector3 position = start + direction * (distance * i / (count + 1));
            AddPoint(position, row);
        }
    }

    public void AddPoint(Vector3 position, int row)
    {
        pointsByRow[row].Add(position);
    }

    public void SpawnSphere(Vector3 position, float f = 0.1f)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * f;

        spheres.Add(sphere);
    }
}
