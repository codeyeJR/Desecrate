using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCheckComponent : MonoBehaviour
{
    public void OnClick()
    {
        // Calls to kill the skillcheck and fail it in SkillCheck.CS
        SkillCheck.Instance.KillSkillCheckFail();
    }
}
