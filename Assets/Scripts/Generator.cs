using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Interactable
{
    public static Generator generatorInstance;
    public bool interacting = false;   


    void Start()
    {
        generatorInstance = this;
    }

    public override void onFocus()
    {
        // Looking at Generator
        if(!interacting)
        {
        OverlayManager.Instance.OpenOverlay("interact");
        }
    }
    
    public override void onInteract()
    {
        // Interacting with the generatorr
        FirstPersonController.Instance.mouseLocked = false;
        print("interacted with " + gameObject.name);
        interacting = true;
    }

    public override void onLoseFocus()
    {
        // No longer looking at Generator
        FirstPersonController.Instance.mouseLocked = true;
        OverlayManager.Instance.OpenOverlay("base");
        interacting = false;
    }
}
