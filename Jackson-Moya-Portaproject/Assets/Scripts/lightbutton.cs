using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class lightbutton : MonoBehaviour
{
    public Sprite lightSourceOnSprite;
    public Sprite lightSourceOffSprite;
    public SpriteRenderer spriteLight;
    public Button classInstance;
    // Start is called before the first frame update
    void Start()
    {
        spriteLight = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (classInstance.activated == false) //if activated is false
        {
            spriteLight.sprite = lightSourceOffSprite; //turn off light

        }
        else if (classInstance.activated == true) //if it is true
        {
            spriteLight.sprite = lightSourceOnSprite; //turn on light
        }
    }

    /*
    public IEnumerator LightReset() //light reset
    {
        yield return new WaitForSeconds(0.5f); //wait 0.5 seconds 
        SceneManager.LoadScene("EvanTestScene"); //reloads the scene from the start
    }
    */

    public void OnTriggerEnter2D(Collider2D collider)
    {
        // Ensure the collider is the player before proceeding.
        if (collider.tag == "Player")
        {
            if (classInstance.activated == true) //if light is on, then
            {
                //StartCoroutine(LightReset()); //reset the scene!
                GameManager.Instance.ResetStage();
            }
        }
    }
}
