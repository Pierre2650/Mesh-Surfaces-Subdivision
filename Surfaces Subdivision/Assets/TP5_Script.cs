using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class TP5_Script : MonoBehaviour
{
    [Header("Chakin's")]
    [Range(1, 10)]
    public int nbDivitions = 1;

    public List<Vector2> baseGraph = new List<Vector2>();
    public List<Vector2> divideGraph = new List<Vector2>();

    [Header("Loop's")]
    [Range(1, 6)]
    public int nbLoopDivitions = 1;
    public string fileName;
    private MeshFilter myMF;

    private List<Vector3> vertices = null;
    private List<int> triangles = new List<int>();
    private List<Vector3> newVertices = new List<Vector3>();

    private class Edge
    {
        public int v1Index;
        public int v2Index;

        public int v3Index;
        public int v4Index = -1;

        public int splitIndex = -1;

        public Edge(int v1, int v2, int v3)
        {
            v1Index = v1;
            v2Index = v2;
            v3Index = v3;
        }

    }

    List<Edge> Edges = new List<Edge>();

    private void Start()
    {
        myMF = GetComponent<MeshFilter>();
        readOFF();

        for(int i =0; i < nbLoopDivitions; i++)
        {
            loopAlgorithm();
            Edges.Clear();
            newVertices.Clear();
        }
    }

    private void readOFF()
    {
        string path = "Assets\\";
        path += fileName;


        if (!File.Exists(path))
        {
            Debug.LogError("File Doesn't exist in the assets folder");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        string[] parts = lines[1].Split(' ');
        int nbVertices = int.Parse(parts[0]);
        int nbTriangles = int.Parse(parts[1]);

        Vector3[] tempVertices = new Vector3[nbVertices];
        Vector3 center = Vector3.zero;


        int lineIndex = 2;
        float max = 0;
        for (int j = 0; j < tempVertices.Length; lineIndex++, j++)
        {

            parts = lines[lineIndex].Split(' ');

            Vector3 aVertice = new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));

            if (Mathf.Abs(aVertice.x) > max) { max = Mathf.Abs(aVertice.x); }
            if (Mathf.Abs(aVertice.y) > max) { max = Mathf.Abs(aVertice.y); }
            if (Mathf.Abs(aVertice.z) > max) { max = Mathf.Abs(aVertice.z); }

            center += aVertice;
            tempVertices[j] = aVertice;

        }

        vertices = tempVertices.ToList();

        center = new Vector3(center.x / nbVertices, center.y / nbVertices, center.z / nbVertices);

        for (int j = 0; j < tempVertices.Length; j++)
        {
            tempVertices[j] -= center;
            tempVertices[j] = new Vector3(tempVertices[j].x / max, tempVertices[j].y / max, tempVertices[j].z / max);
        }


        for (int i = 0; i < nbTriangles; lineIndex++, i++)
        {
            parts = lines[lineIndex].Split(' ');

            triangles.Add(int.Parse(parts[1]));
            triangles.Add(int.Parse(parts[2]));
            triangles.Add(int.Parse(parts[3]));


        }



        myMF.mesh.vertices = vertices.ToArray();
        myMF.mesh.triangles = triangles.ToArray();


    }


    private void chakinAlgorithm()
    {
        divideGraph = baseGraph;
        List<Vector2> newGraph = divideGraph;

        for (int i = 0; i < nbDivitions; i++)
        {

            Divide();
        }

    }

    private void Divide()
    {
        List<Vector2> newG = new List<Vector2>();
        float ratio1 = 3f / 4f;
        float ratio2 = 1f / 4f;

        for (int i = 0; i < divideGraph.Count - 1; i++)
        {

            Vector2 newVec = ratio1*divideGraph[i] + ratio2* divideGraph[i+1];
            Vector2 newVec2 = ratio2 * divideGraph[i] + ratio1 * divideGraph[i + 1];
            newG.Add(newVec);
            newG.Add(newVec2);
        }

        newG.Add(newG[0]);

        divideGraph = newG;
    }


    private void loopAlgorithm()
    {
        //Split
       // List<int> updateTriangles = new List<int>(triangles);

        findEdges();
        int lastIndex = vertices.Count - 1;

        foreach (Edge e in Edges)
        {
            Vector3 newV = edgeSplit(e);
            newVertices.Add(newV);
            lastIndex++;
            e.splitIndex = lastIndex;

        }


        //Reposition
        List<Vector3> updateVertices = new List<Vector3>();
        for(int i = 0; i< vertices.Count; i++)
        {
            updateVertices.Add(VertexReposition(i));
        }

        //Update
        for (int i = 0; i < newVertices.Count; i++)
        {
            updateVertices.Add(newVertices[i]);
        }



        List<int> updateTriangles = newTriangles();


        vertices = updateVertices;
        triangles = updateTriangles;
        myMF.mesh.vertices = vertices.ToArray();
        myMF.mesh.triangles = triangles.ToArray();


    }

    private void findEdges()
    {
        for (int i = 0; i < triangles.Count - 1; i += 3)
        {
            // edge 1 triangles[0] triangles[1]
            // edge 2 triangles[0] triangles[2]
            // edge 3 triangles[1] triangles[2]

            if (!edgeExist(triangles[i], triangles[i + 1]))
            {
                Edges.Add(new Edge(triangles[i], triangles[i + 1], triangles[i+2]));
            }
            else
            { 
                Edge temp = Edges.Find((x => (x.v1Index == triangles[i] && x.v2Index == triangles[i+1]) || (x.v1Index == triangles[i+1] && x.v2Index == triangles[i])));
                temp.v4Index = triangles[i + 2];
            }

            if (!edgeExist(triangles[i], triangles[i + 2]))
            {
                Edges.Add(new Edge(triangles[i], triangles[i + 2], triangles[i+1]));
            }
            else
            {
                Edge temp = Edges.Find((x => (x.v1Index == triangles[i] && x.v2Index == triangles[i + 2]) || (x.v1Index == triangles[i + 2] && x.v2Index == triangles[i])));
                temp.v4Index = triangles[i+1];
            }

            if (!edgeExist(triangles[i+1], triangles[i + 2]))
            {
                Edges.Add(new Edge(triangles[i+1], triangles[i + 2], triangles[i]));
            }
            else
            {
                Edge temp = Edges.Find((x => (x.v1Index == triangles[i+1] && x.v2Index == triangles[i + 2]) || (x.v1Index == triangles[i + 2] && x.v2Index == triangles[i+1])));
                temp.v4Index = triangles[i];
            }

        }


    }

    private bool edgeExist(int v1Index, int v2Index) 
    {
        foreach(Edge e in Edges)
        {
            if(e.v1Index == v1Index && e.v2Index == v2Index)
            {
                return true;
            }

            if (e.v1Index == v2Index && e.v2Index == v1Index)
            {
                return true;
            }

        }

        return false;
    }

    private Vector3 edgeSplit(Edge e)
    {

        if (e.v3Index == -1 || e.v4Index == -1)
        {
            Debug.Log("a vertex neighbor wasn't found,   v3Index = " + e.v3Index + "    v4Index = " + e.v4Index);
            return Vector3.zero;
        }


        Vector3 v1 = vertices[e.v1Index];
        Vector3 v2 = vertices[e.v2Index];
        Vector3 v3 = vertices[e.v3Index];
        Vector3 v4 = vertices[e.v4Index];

        Vector3 newVertex = (3f / 8f) * (v1 + v2) + (1f / 8f) * (v3 + v4);

        return newVertex;


    }


    private Vector3 VertexReposition(int vIndex)
    {
        List<int> neightborsIndex = findneightborVertices(vIndex);
        int n = neightborsIndex.Count;
        float weight = 0;

        if (n == 6)
        {
            weight = 1f / 16f;
        }
        else
        {
             weight = (1 / (float)n) * (5f / 8f - Mathf.Pow(3f / 8f + 1f / 4f * Mathf.Cos((2f * Mathf.PI) / (float)n), 2));
        }
      

        Vector3 newVertexPos = Vector3.zero;
        Vector3 epsilon = Vector3.zero;
        foreach(int i  in neightborsIndex)
        {

            epsilon += vertices[i];
        }

        newVertexPos = (1 - (float)n * weight) * vertices[vIndex] + weight * epsilon;

        return newVertexPos;
    }

    private List<int> findneightborVertices(int VIndex) {

        List<int> neightborsIndexes = new List<int>();

        for (int i = 0; i < triangles.Count - 3; i += 3)
        {
            if (triangles[i] == VIndex) {
                
                if(!neightborsIndexes.Exists(x => x == triangles[i+1]))
                {
                    neightborsIndexes.Add(triangles[i + 1]);
                }

                if (!neightborsIndexes.Exists(x => x == triangles[i + 2]))
                {
                    neightborsIndexes.Add(triangles[i + 2]);
                }

            } else if (triangles[i+1] == VIndex)
            {

                if (!neightborsIndexes.Exists(x => x == triangles[i]))
                {
                    neightborsIndexes.Add(triangles[i]);
                }

                if (!neightborsIndexes.Exists(x => x == triangles[i + 2]))
                {
                    neightborsIndexes.Add(triangles[i + 2]);
                }

            } else if (triangles[i+2] == VIndex)
            {

                if (!neightborsIndexes.Exists(x => x == triangles[i]))
                {
                    neightborsIndexes.Add(triangles[i]);
                }

                if (!neightborsIndexes.Exists(x => x == triangles[i + 1]))
                {
                    neightborsIndexes.Add(triangles[i + 1]);
                }

            }
        }

        return neightborsIndexes;
    }


    private List<int> newTriangles()
    {
        List<int> updateTriangles = new List<int>();

        for (int i = 0; i < triangles.Count - 1; i += 3)
        {
            // edge 1 triangles[0] triangles[1]
            // edge 2 triangles[0] triangles[2]
            // edge 3 triangles[1] triangles[2]

            Edge edge1 = Edges.Find((x => (x.v1Index == triangles[i] && x.v2Index == triangles[i+1]) || (x.v1Index == triangles[i + 1] && x.v2Index == triangles[i])));
            Edge edge2 = Edges.Find((x => (x.v1Index == triangles[i] && x.v2Index == triangles[i + 2]) || (x.v1Index == triangles[i + 2] && x.v2Index == triangles[i])));
            Edge edge3 = Edges.Find((x => (x.v1Index == triangles[i+1] && x.v2Index == triangles[i + 2]) || (x.v1Index == triangles[i + 2] && x.v2Index == triangles[i+1])));

            updateTriangles.Add(triangles[i]);
            updateTriangles.Add(edge1.splitIndex);
            updateTriangles.Add(edge2.splitIndex);

            updateTriangles.Add(edge2.splitIndex);
            updateTriangles.Add(edge3.splitIndex);
            updateTriangles.Add(triangles[i+2]);

            updateTriangles.Add(edge1.splitIndex);
            updateTriangles.Add(triangles[i + 1]);
            updateTriangles.Add(edge3.splitIndex);

            updateTriangles.Add(edge1.splitIndex);
            updateTriangles.Add(edge3.splitIndex);
            updateTriangles.Add(edge2.splitIndex);
        }

       

        return updateTriangles;
    }


    private void OnDrawGizmos()
    {
        if (baseGraph.Count < 2) { return; }
        Gizmos.color = Color.yellow;
        for(int i = 0 ; i < baseGraph.Count - 1; i++)
        {
            Gizmos.DrawLine(baseGraph[i], baseGraph[i+1]);
        }

        chakinAlgorithm();

        if ( divideGraph.Count <2 ) {return; }
        Gizmos.color = Color.cyan;
        for (int i = 0; i < divideGraph.Count - 1; i++)
        {
            Gizmos.DrawLine(divideGraph[i], divideGraph[i + 1]);
        }

        /*for (int i = 0; i < divideGraph.Count; i++)
        {
            Gizmos.DrawWireSphere(divideGraph[i], 0.1f);
        }*/

        /*Gizmos.color = Color.red;
        foreach (Vector3 v in newVertices)
        {
            Gizmos.DrawWireSphere(v, 0.05f);
        }

        foreach (Vector3 v in vertices)
        {
            Gizmos.DrawWireSphere(v, 0.05f);
        }*/
    }
}
