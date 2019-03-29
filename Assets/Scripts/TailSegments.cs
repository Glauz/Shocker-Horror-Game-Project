using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailSegments : MonoBehaviour
{
    public float speed = 1;
    public GameObject target;
    public float followDistance = .5f;


    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, (target.transform.position + -(target.transform.forward * followDistance)), Time.deltaTime * speed);
        //transform.position = (target.transform.position + -(target.transform.forward * followDistance)) * Time.deltaTime * speed;
        transform.rotation = target.transform.rotation;
    }
}
