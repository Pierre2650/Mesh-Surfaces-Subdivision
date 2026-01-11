using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;

public class TP5_Script : MonoBehaviour
{
    [Range(1, 10)]
    public int nbDivitions = 1;

    public List<Vector2> baseGraph = new List<Vector2>();
    public List<Vector2> divideGraph = new List<Vector2>();

    private void chakinAlgoritm()
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
    private void OnDrawGizmos()
    {
        if (baseGraph.Count < 2) { return; }
        Gizmos.color = Color.yellow;
        for(int i = 0 ; i < baseGraph.Count - 1; i++)
        {
            Gizmos.DrawLine(baseGraph[i], baseGraph[i+1]);
        }

        chakinAlgoritm();

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


    }
}
