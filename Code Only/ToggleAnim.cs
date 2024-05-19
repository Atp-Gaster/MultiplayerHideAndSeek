using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAnim : MonoBehaviour
{
    public Animator Anim;

    public void PlayAnimation()
    {        
        Anim.SetTrigger("Trigger");
    }
}
