using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] player playerScript;

    void PlayFootstepSfx()
    {
        playerScript.PlayFootstepSfx();
    }
}
