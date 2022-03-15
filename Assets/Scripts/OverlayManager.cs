using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayManager : MonoBehaviour
{
    public static OverlayManager Instance;
    [SerializeField] Overlay[] overlays;

    void Awake()
    {
        Instance = this;
    }
        // Opening a new overlay
        public void OpenOverlay(string overlayName)
    {
        for(int i =0; i< overlays.Length; i++)
        {
            if(overlays[i].overlayName == overlayName)
            {
                overlays[i].Open();
            }
            else if(overlays[i].open)
            {
                // Calls the function that closes the overlay
                CloseOverlay(overlays[i]);
            }
        }
    }

    // Opens new overlay
    public void OpenOverlay(Overlay overlay)
    {
          for(int i =0; i< overlays.Length; i++)
        {
            if(overlays[i].open)
            {
                CloseOverlay(overlays[i]);
            }
        }
        overlay.Open();
    }

    // Closes the overlay
    public void CloseOverlay(Overlay overlay)
    {
        overlay.Close();
    }
}
