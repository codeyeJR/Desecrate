// Doesn't work ATM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      // When the Player should take damage
       if(other.CompareTag("Player"))
        FirstPersonController.OnTakeDamage(15);
        print("Player Triggered Damage Zone");

      print("Damage Zone Trigger");
   }
}
