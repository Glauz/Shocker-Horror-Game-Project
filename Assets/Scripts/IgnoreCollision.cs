using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    public GameObject[] gameObjects;

    private Collider collider;

    private void Start()
    {
        collider = GetComponent<Collider>();

        foreach (var go in gameObjects)
                Physics.IgnoreCollision(go.GetComponent<Collider>(), collider);
    }

    //public void OnTriggerEnter(Collider other)
    //{
    //    if (!enabled) return; //Prevents Reflections

    //    Debug.Log(other.name);


    //    foreach (string tag in tags)
    //        if (other.gameObject.tag == tag)
    //            Physics.IgnoreCollision(other.transform.GetComponent<Collider>(), collider);
    //}
}
