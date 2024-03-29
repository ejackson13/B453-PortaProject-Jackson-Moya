using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public bool activated = true;
    public GameObject button;
    public SpriteRenderer lightSource;
    public Sprite lightSourceOnSprite;
    public Sprite lightSourceOffSprite;

    // Start is called before the first frame update
    void Start()
    {
        lightSource = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (activated == false)
        {
            lightSource.sprite = lightSourceOffSprite;

        }
        else
        {
            lightSource.sprite = lightSourceOnSprite;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player" && this.gameObject == button && activated == true)
        {
            activated = false;
        }
        else if (collider.tag == "Player" && this.gameObject == button && activated == false)
        {
            activated = true;
        }
    }
}
