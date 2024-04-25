using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] Transform playerTransform;
    [SerializeField] Sprite[] backgroundSprites;

    // Start is called before the first frame update
    void Start()
    {
        int bkgSpawnAmount = Random.Range(25, 100);
        for (int i=0; i<bkgSpawnAmount; i++) 
        {
            GameObject child = new GameObject();
            SpriteRenderer sr = child.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            
            int randi = Random.Range(0, backgroundSprites.Length);
            sr.sprite = backgroundSprites[randi];
            child.transform.localScale *= Random.Range(0.8f, 1.2f);
            Color newColor = sr.color;
            newColor.a = Random.Range(0.4f, .8f);
            newColor.b = Random.Range(0.8f, 1.0f);
            sr.color = newColor;

            child.transform.parent = transform;
            Vector2 viewPortPos = new Vector2(Random.Range(.05f, .95f), Random.Range(.075f, .925f));
            Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewPortPos);
            worldPos.z = 1;
            child.transform.position = worldPos;
        }

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position;
        newPos.x = -0.02f * playerTransform.position.x;
        transform.position = newPos;
    }
}
