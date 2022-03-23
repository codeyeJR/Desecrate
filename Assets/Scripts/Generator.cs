using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Interactable
{
    public static Generator generatorInstance;
    public bool interacting = false;   
    public bool complete = false;


    void Start()
    {
        generatorInstance = this;
    }

    public override void onFocus()
    {
        // Looking at Objective, opens the overlay
        if(!interacting && !complete)
        {
        OverlayManager.Instance.OpenOverlay("interact");
        }
    }
    
    public override void onInteract()
    {
        if(!interacting && !complete)
        {
            // Interacting with the Objective, locks the camera
            FirstPersonController.Instance.mouseLocked = false;
            interacting = true;
        }
    }

    public override void onLoseFocus()
    {
        // No longer looking at Objective, clears overlay 
        FirstPersonController.Instance.mouseLocked = true;
        OverlayManager.Instance.OpenOverlay("base");
        interacting = false;
    }
}
