using UnityEngine;
using System.Collections;

public class Grabbable : MonoBehaviour {

    //public bool alignRotationOnGrab;
    public bool enable = true;
    public RotateType initialRotation = RotateType.normal;
    public enum RotateType { normal, zero, custom}
    [Header("Work if set to custom")]
    public Vector3 customRotation;
    public Transform parent;

    public void Start()
    {   
        if (parent != null)
            this.gameObject.transform.SetParent(parent);
    }

    public void SetEnable(bool value)
    {
        enable = value;
    }
}
