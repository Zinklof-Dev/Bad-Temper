using System.Collections.Generic;
using UnityEngine;

public class SpiralGridMaker : MonoBehaviour
{
    public string xS = "";
    public string yS = "";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<int> x = new List<int>();
        List<int> y = new List<int>();

        for (int i = 0; i < 2000; i += 10)
        {
            for (int j = i; j < i * i; j++)
            {
                x.Add(1 + j);
                y.Add(i);
            }
            for (int j = i; j < i * i; j++)
            {
                y.Add(1 + j);
                x.Add(i);
            }
            for (int j = i; j < i * i; j++)
            {
                x.Add(-1 + j);
                y.Add(i);
            }
            for (int j = i; j < i * i; j++)
            {
                y.Add(-1 + j);
                x.Add(i);
            }
        }

        foreach (int i in x)
        {
            xS += x[i].ToString() + ", ";
        }
        foreach (int i in y)
        {
            yS += y[i].ToString() + ", ";
        }
    }
}
