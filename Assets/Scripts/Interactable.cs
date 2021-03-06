using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
  // defines the objects that the Player can interact with
  public virtual void Awake()
  {
    gameObject.layer = 8;
  }
  public abstract void onInteract();
  public abstract void onFocus();
  public abstract void onLoseFocus();
}
