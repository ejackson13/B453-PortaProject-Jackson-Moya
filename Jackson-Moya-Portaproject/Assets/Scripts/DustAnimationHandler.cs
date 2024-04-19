using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustAnimationHandler : MonoBehaviour
{
    [SerializeField] Animator anim;
    private Vector3 lockedPos = Vector3.zero;
    private bool isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            transform.position = lockedPos;
        }
    }
    public void PlayDustAnimation()
    {
        anim.gameObject.SetActive(true);
        Vector3 newScale = anim.transform.localScale;
        newScale.x = UnityEngine.Random.Range(.6f, 1.4f);
        anim.transform.localScale = newScale;

        isPlaying = true;
        lockedPos = transform.position;

        anim.Play("Dust");
    }

    private void AnimationFinished()
    {
        gameObject.SetActive(false);
        isPlaying = false;
        transform.localPosition = Vector3.zero;
    }
}
