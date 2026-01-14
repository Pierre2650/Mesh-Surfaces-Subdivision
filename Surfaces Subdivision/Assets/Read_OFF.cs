using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Read_OFF : MonoBehaviour
{
  

    [Header("Loop's")]

    public string fileName;
    private MeshFilter myMF;

    private List<Vector3> vertices = null;
    private List<int> triangles = new List<int>();

    private void Start()
    {
        myMF = GetComponent<MeshFilter>();
        readOFF();

      
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


}
