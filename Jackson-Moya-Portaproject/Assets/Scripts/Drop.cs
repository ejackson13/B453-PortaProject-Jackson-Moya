using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : MonoBehaviour
{
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Rigidbody2D rb;

    Color[] colors = { new Color32(58, 83, 57, 255), new Color32(57, 64, 113, 255) };
    Color currentColor;
    public Vector2 direction = new Vector2(0, 1);
    float waitTime;
    // Start is called before the first frame update
    void Start()
    {
        currentColor = Random.value < .6 ? colors[0] : colors[1];
        sprite.color = currentColor;
        waitTime = Random.Range(1f, 5f);
        StartCoroutine(FadeOutTimer(waitTime));

        var variation = new Vector2(direction.x == 0 ? Random.Range(-5.0f, 5.0f) : 0.0f, direction.y == 0 ? Random.Range(-5f, 5f) : 0.0f);

        rb.velocity = (direction * Random.Range(-25f, 25f)) + variation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator FadeOutTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //$twn_fade.interpolate_property(self, 'modulate:a', self.modulate.a, 0, 1, Tween.TRANS_QUART, Tween.EASE_IN, 0.2);
        float duration = 1f;
        float passedTime = 0;
        float startingOpacity = 1;
        // interpolate position to target
        while (passedTime <= duration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / duration;

            currentColor.a = Mathf.Lerp(startingOpacity, 0, alpha);

            yield return null;
        }



        Destroy(gameObject);
    }
}
