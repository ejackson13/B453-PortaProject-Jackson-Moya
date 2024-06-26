using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class lighting : MonoBehaviour
{
    //initialized variables 
    public int secondsForLight = 3; //seconds for light switching on and off
    public bool isLightOn; //bool to see if light is on
    public Sprite lightOnSprite; //sprite for the light being turned on
    public Sprite lightOffSprite; //sprite for the light being turned off
    SpriteRenderer spriteLight; //the sprite for the light
    
    public GameObject player; //the player

    // Start is called before the first frame update
    void Start()
    {
        spriteLight = GetComponent<SpriteRenderer>(); //grabs sprite rendrer
        isLightOn = true; //set bool to true to turn light on when game starts
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Light()); //starts the light coroutine
        if (GetComponent<Collider2D>() != null) 
        {
            GetComponent<Collider2D>().enabled = isLightOn;
        }
    }

    IEnumerator Light()
    {
        if (isLightOn == true) //if light is on
        {
            yield return new WaitForSeconds(secondsForLight); //wait 3 secs
            spriteLight.sprite = lightOffSprite; //turns light off
            isLightOn = false; //bool switched to false
            yield return new WaitForSeconds(secondsForLight); //wait 3 secs again

        }
        else
        {
            yield return new WaitForSeconds(secondsForLight); //wait 3 secs
            spriteLight.sprite = lightOnSprite; //turn light on
            isLightOn = true; //set bool to true
            yield return new WaitForSeconds(secondsForLight); //wait 3 secs again
        }
    }

    // moving the code to reset the level to a game manager singleton
    /*
    public IEnumerator LightReset() //light reset
    {
        yield return new WaitForSeconds(0.5f); //wait 0.5 seconds 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //reloads the scene from the start
    }
    */

    void OnTriggerEnter2D(Collider2D collider) //trigger detection
    {
        if (collider.gameObject.tag == "Player" && isLightOn == true) //if the player walks into the light when it's turned on, then run the code
        {
            //Debug.Log("Entered Light");
            //StartCoroutine(LightReset()); //starts the coroutine for the light reset
            GameManager.Instance.ResetStage();
        }
    }
}
