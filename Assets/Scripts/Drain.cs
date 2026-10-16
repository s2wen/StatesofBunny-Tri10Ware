using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

public class Drain : MonoBehaviour
{

    private static List<Drain> drainList = new List<Drain>();
    private int idx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        drainList.Add(this);
        idx = drainList.Count - 1;
    }

    public Drain moveNext()
    {
        int i = drainList.IndexOf(this);
        return drainList[(i + 1) % drainList.Count];
    }

    public Vector3 getPos()
    {
        return GetComponent<Collider2D>().bounds.center;
    }

    void OnDestroy()
    {
        drainList.Remove(this);
    }
}
