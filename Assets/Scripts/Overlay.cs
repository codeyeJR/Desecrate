using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlay : MonoBehaviour
{
    public string overlayName;
    public bool open;

    // Opening the Menu
    public void Open() 
    {
        open = true;
        gameObject.SetActive(true);
    }

    // Closing the Menu
    public void Close()
    {
        open = false;
        gameObject.SetActive(false);
    }
}