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
    private MeshFilter myMF;
    public string fileName;
    private List<Vector3> vertices = null;
    private List<int> triangles = new List<int>();
    public List<Vector3> newVertices = new List<Vector3>();

    private void Start()
    {
        myMF = GetComponent<MeshFilter>();
        readOFF();
        loopAlgorithm();
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


        Debug.Log("nbVertices = "+vertices.Count);
        Debug.Log("TrianglesCount = "+ triangles.Count);

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

        //pour tout vertex calculer new vertex 
        for (int i = 0; i < triangles.Count - 1; i+=3) {
            // edge 1 triangles[0] triangles[1]
            // edge 2 triangles[0] triangles[2]
            // edge 3 triangles[1] triangles[2]

            Vector3 newV1 = edgeSplit(i, i + 1);
            Vector3 newV2 = edgeSplit(i, i + 2);
            Vector3 newV3 = edgeSplit(i + 1, i + 2);

            newVertices.Add(newV1);
            newVertices.Add(newV2);
            newVertices.Add(newV3);
        }




    }

    private Vector3 edgeSplit(int v1Index, int  v2Index)
    {

        findTriangles(v1Index, v2Index, out int v3Index, out int v4Index);

        Vector3 v1 = vertices[triangles[v1Index]];
        Vector3 v2 = vertices[triangles[v2Index]];
        Vector3 v3 = vertices[triangles[v3Index]];        
        Vector3 v4 = vertices[triangles[v4Index]];

        Vector3 newVertex = (3f / 8f) * (v1 + v2) + (1f / 8f) * (v3 + v4);

            
        return newVertex;
        

    }



    private void findTriangles(int v1Index, int v2Index, out int v3Index, out int v4Index)
    {
        v3Index = -1; v4Index = -1;


        for (int i = 0; i < triangles.Count - 3; i += 3)
        {
            bool v1Exist = true, v2Exist = false;
            // edge 1 triangles[0] triangles[1]
            // edge 2 triangles[0] triangles[2]
            // edge 3 triangles[1] triangles[2]

            if (triangles[i] == triangles[v1Index])
            {
                v1Exist = true;
            }
            else if (triangles[i] == triangles[v2Index]) {
                v2Exist = true;
            }

            if (triangles[i + 1] == triangles[v1Index])
            {
                v1Exist = true;
            }
            else if (triangles[i+1] == triangles[v2Index])
            {
                v2Exist = true;
            }

            if (triangles[i+2] == triangles[v1Index])
            {
                v1Exist = true;
            }
            else if (triangles[i+2] == triangles[v2Index])
            {
                v2Exist = true;
            }

            if(v1Exist && v2Exist)
            {
                if(v3Index == -1)
                {
                    v3Index = i;
                }else if(v4Index == -1)
                {
                    v4Index = i;
                }
            }




        }
    }

    private void findNeighbors(int vertexIndex)
    {
        int counter = 0; 

        for (int i = 0; i < triangles.Count - 3; i += 3)
        {
            if (triangles[i] == vertexIndex)
            {
                counter++;
            }
            

            if (triangles[i + 1] == vertexIndex)
            {
                counter++;
            }
           

            if (triangles[i + 2] == vertexIndex)
            {
                counter++;
            }
          
           
        }

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

        Gizmos.color = Color.red;
        foreach (Vector3 v in newVertices)
        {
            Gizmos.DrawWireSphere(v, 0.1f);
        }


    }
}
