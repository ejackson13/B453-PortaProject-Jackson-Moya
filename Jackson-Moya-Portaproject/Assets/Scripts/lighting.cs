using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lighting : MonoBehaviour
{
    public int secondsForLight = 3;
    private bool isLightOn;
    public Sprite lightOnSprite;
    public Sprite lightOffSprite;
    SpriteRenderer spriteLight;

    // Start is called before the first frame update
    void Start()
    {
        spriteLight = GetComponent<SpriteRenderer>();
        isLightOn = true;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Light());
    }

    IEnumerator Light()
    {
        if (isLightOn == true)
        {
            yield return new WaitForSeconds(3);
            spriteLight.sprite = lightOffSprite;
            isLightOn = false;
            yield return new WaitForSeconds(3);

        }
        else
        {
            yield return new WaitForSeconds(3);
            spriteLight.sprite = lightOnSprite;
            isLightOn = true;
            yield return new WaitForSeconds(3);
        }

    }
}
