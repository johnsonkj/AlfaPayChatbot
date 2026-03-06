using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
   
    public Animator CharlieKirkController;

    public void TriggerCharlieTalkAnimation(bool value)
    {
        CharlieKirkController.SetBool("Talk",value);
    }
}
