using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] Menu[] menus;

    void Awake()
    {
        Instance = this;
    }

    // Opens the menu
    public void OpenMenu(string menuName)
    {
        for(int i =0; i< menus.Length; i++)
        {
            if(menus[i].menuName == menuName)
            {
                menus[i].Open();
            }
            else if(menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    // Opens a new menu
    public void OpenMenu(Menu menu)
    {
          for(int i =0; i< menus.Length; i++)
        {
            if(menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }

    // Closes the menu
    public void CloseMenu(Menu menu)
    {
        menu.Close();
    }
}
