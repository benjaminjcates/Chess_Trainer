using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlights : MonoBehaviour
{

    public static BoardHighlights Instance { set; get; }

    public GameObject highlightPrefab;
    private List<GameObject> highlights;
    private List<GameObject> databaseHighlights;

    private void Start()
    {
        Instance = this;
        highlights = new List<GameObject>();
        databaseHighlights = new List<GameObject>();
    }

    private GameObject GetHighLightObject()
    {
        GameObject go = highlights.Find(g => !g.activeSelf);
 

        if (go == null)
        {
            go = Instantiate(highlightPrefab);
            highlights.Add(go);
        }

        return go;
    }

    public void HighLightAllowedMoves(bool[,] moves)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (moves[i, j])
                {

                    //does this as many times as there are viable moves
                    GameObject go = GetHighLightObject();
                    go.SetActive(true);
                    go.transform.position = new Vector3(i + 0.5f, 0.0001f, j + 0.5f);
                    print("sup");

                }
            }

        }
    }
    //This function highlights the square i,j
    public void HighlightOption(int i, int j)
    {
        GameObject go = GetHighLightObject();
        go.SetActive(true);
        go.transform.position = new Vector3(i + 0.5f, 0.0001f, j + 0.5f);
        print("sup again");
    }

    public void HideHighlights()
    {
        foreach (GameObject go in highlights)
        {
            go.SetActive(false);
            print("Hide all highlights");
        }

    }

    public void HighLightDatabaseMoves()
    {
        //1. find out how many variations still true after making blacks move.



    }
}
