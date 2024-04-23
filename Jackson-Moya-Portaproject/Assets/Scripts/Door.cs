using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // when the player collides with the object, move to the next scene
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.NextLevel();

        }
    }
}
