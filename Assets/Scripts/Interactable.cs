using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string[] tags = new string[] { "Player" };
    public InteractType interactType = InteractType.None;
    public UnityEvent events;        
    
    public enum InteractType { None, AutoRun, Activation, OnCollisionEnter, OnTriggerEnter, OnTriggerExit }

    public void OnEnable()
    {
        if (interactType != InteractType.AutoRun) return;
        if (interactType == InteractType.Activation && gameObject.layer == 2)
        { Debug.LogError(name + " has Ignore Raycast Layer as Activasion Type!");}

        InvokeEvents();
    }

    //Call This Method through raycast with this method name
    public void Interact()
    {
        if (!enabled) return; //Prevents Reflections
        if (interactType != InteractType.Activation) return;
        
        InvokeEvents();
    }      
    
    public void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return; //Prevents Reflections
        if (interactType != InteractType.OnCollisionEnter) return;
        var collider = GetComponent<Collider>();
        if (collider == false ) { Debug.LogError("Collider On Object!"); return; }
        if (collider.isTrigger == true) { Debug.LogError("Collider should not be a trigger!"); return; }

        foreach (string tag in tags)
            if (collision.gameObject.tag == tag)
                InvokeEvents();
    }  

    public void OnTriggerEnter(Collider other)
    {
        if (!enabled) return; //Prevents Reflections
        if (interactType != InteractType.OnTriggerEnter) return;
        var collider = GetComponent<Collider>();
        if (collider == false) { Debug.LogError("Collider On Object!"); return; }
        if (collider.isTrigger == false) { Debug.LogError("Collider should be a trigger!"); return; }
          
        foreach (string tag in tags)
            if (other.gameObject.tag == tag)
                InvokeEvents();
    }

    public void OnTriggerExit(Collider other)
    {
        if (!enabled) return; //Prevents Reflections
        if (interactType != InteractType.OnTriggerExit) return;
        var collider = GetComponent<Collider>();
        if (collider == false) { Debug.LogError("Collider On Object!"); return; }
        if (collider.isTrigger == false) { Debug.LogError("Collider should be a trigger!"); return; }

        foreach (string tag in tags)
            if (other.gameObject.tag == tag)
                InvokeEvents();
    }

    private void InvokeEvents() { events.Invoke(); }

    public void Disable() { this.enabled = false;  }
    public void Enable() { this.enabled = true; }

    
}
