using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionesArma : MonoBehaviour
{
    public List<GameObject> objects;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i <objects.Count;i++)
        {
            if(objects[i] == null|| !objects[i].GetComponent<DynamicMeshCutter.MeshTarget>().canCut)
            {
                objects.Remove(objects[i]);

            }
        }

    }
    public GameObject[] GetObjects()
    {
        return objects.ToArray();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!objects.Contains(other.gameObject)&&other.GetComponent< DynamicMeshCutter.MeshTarget>() != null)
        {
            if(other.GetComponent<DynamicMeshCutter.MeshTarget>().canCut)
            objects.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (objects.Contains(other.gameObject) && other.GetComponent<DynamicMeshCutter.MeshTarget>() != null)
        {
            objects.Remove(other.gameObject);
        }
    }
}
