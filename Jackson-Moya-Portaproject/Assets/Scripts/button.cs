using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public bool activated = true;
    public GameObject lightSource;
    public SpriteRenderer lightSourceOnSprite;
    public SpriteRenderer lightSourceOffSprite;
    public SpriteRenderer lightBeam;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (activated == false)
        {
            lightBeam.enabled = false;
        }
        else
        {
            lightBeam.enabled = true;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player" && activated == true)
        {
            activated = false;
        }
        else if (collider.tag == "Player" && activated == false)
        {
            activated = true;
        }
    }
}
