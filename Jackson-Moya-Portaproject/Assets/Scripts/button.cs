using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    public bool activated = true; //activated bool for the light to turn off and on

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player") //if the collider was the player
        {
            activated = !activated; // Toggle the activated state
        }
    }
}
