using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Color redTint = new Color32(165, 165, 255, 20);
    [SerializeField] private Color pauseTint = new Color32(75, 75, 75, 20);
    private Color noTint = new Color(1, 1, 1, 0);
    //[SerializeField] private Color blueTint = new Color32(255, 255, 255, 255);  // I don't think this is needed for this implementation

    private SpriteRenderer screenTint;

    [SerializeField] private float transitionDuration = 0.66f;
    [SerializeField] private float transitionBaseDelay = 0.1f;
    [SerializeField] private float transitionDelay = 0.02f;
    [SerializeField] private float maximumLightMaskSize = 0.55f;
    [SerializeField] private float lightMaskSpacingX = 64f/384f; // space the light masks so they match the number of circles in the original regardless of the resolution
    [SerializeField] private float lightMaskSpacingY = 64f / 216f; // space the light masks so they match the number of circles in the original regardless of the resolution
    [SerializeField] private GameObject lightMaskObject;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // destroy any attempts to have two instances of the singleton
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // make sure this doesn't reload when a new scene is loaded
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        screenTint = GetComponent<SpriteRenderer>();

        // tint screen
        screenTint.color = noTint;
        // create sprite masks
        int rowNum = 0;
        for (float i=0; i<=1; i+=lightMaskSpacingX)  // iterate using viewport to keep consistent number of masks
        {
            GameObject colParent = new GameObject("Col " + rowNum++);
            colParent.transform.parent = transform.GetChild(0);
            Vector3 parentPos = Camera.main.ViewportToWorldPoint(new Vector2(i, 0));
            parentPos.z = 0;
            colParent.transform.position = parentPos;
            for (float j = 1; j >= 0f; j -= lightMaskSpacingY)
            {
                GameObject lightMask = Instantiate(lightMaskObject);
                lightMask.SetActive(false);
                lightMask.transform.localScale = Vector3.zero;
                lightMask.transform.parent = colParent.transform;
                Vector3 maskPos = Camera.main.ViewportToWorldPoint(new Vector2(i, j));
                maskPos.z = 0;
                lightMask.transform.position = maskPos;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ResetStage()
    {
        Instance.StartCoroutine(ResetStageCoroutine());
    }



    public void NextLevel()
    {
        Instance.StartCoroutine(NextLevelCoroutine());
    }


    private IEnumerator ResetStageCoroutine()
    {
        // tint screen
        screenTint.color = redTint;
        yield return new WaitForEndOfFrame();


        // pause game with time scale
        Time.timeScale = 0f;


        // wait .75 seconds (should be timeScale independent)
        float waitTill = Time.realtimeSinceStartup + .75f;
        while (Time.realtimeSinceStartup < waitTill)
        {
            yield return null;
        }
        
        // reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //reloads the scene from the start

        // unpause
        Time.timeScale = 1f;

        // set regular tint
        screenTint.color = noTint;
    }



    private IEnumerator NextLevelCoroutine()
    {
        //$twnGrow.interpolate_property(i, "texture_scale", i.texture_scale, maximumLightMaskSize, duration, Tween.TRANS_QUINT, Tween.EASE_OUT, baseDelay + delay * 2 * (x + y) / lightMaskSpacing)
        // grow circles for transition using coroutine
        for (int i = 1; i < 8; i++)
        {
            Transform col = transform.GetChild(0).GetChild(i);
            for (int j = 0; j < 4; j++)
            {
                GameObject lightMask = col.GetChild(j).gameObject;
                Instance.StartCoroutine(SpriteMaskTween(lightMask, maximumLightMaskSize, transitionDuration, transitionBaseDelay + (transitionDelay * 2 * (i+j))));
            }
        }

        // wait for circles to finish growing
        yield return new WaitForSeconds(2);

        // load next scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        /*
        // make sure scene has finished loading
        yield return new WaitForSeconds(1);

        // shrink circles for transition into new stage
        for (int i = 1; i < 8; i++)
        {
            Transform col = transform.GetChild(0).GetChild(i);
            for (int j = 0; j < 4; j++)
            {
                GameObject lightMask = col.GetChild(j).gameObject;
                Instance.StartCoroutine(SpriteMaskTween(lightMask, maximumLightMaskSize, transitionDuration, transitionDelay * 2 * (i + j)));
            }
        }
        */
    }


    private IEnumerator NewLevelLoadedCoroutine()
    {
        // make sure scene has finished loading
        yield return new WaitForSeconds(1);

        // shrink circles for transition into new stage
        for (int i = 1; i < 8; i++)
        {
            Transform col = transform.GetChild(0).GetChild(i);
            for (int j = 0; j < 4; j++)
            {
                GameObject lightMask = col.GetChild(j).gameObject;
                Instance.StartCoroutine(SpriteMaskTween(lightMask, 0, transitionDuration, transitionDelay * 2 * (i + j)));
            }
        }
    }



    private IEnumerator SpriteMaskTween(GameObject spriteMask, float endingScale, float duration, float delay)
    {
        if (endingScale > 0)
        {
            spriteMask.SetActive(true);
        }

        yield return new WaitForSeconds(delay);

        Vector3 startingScale = spriteMask.transform.localScale;
        Vector3 currentScale;
        Vector3 targetScale = new Vector3(endingScale, endingScale, endingScale);
        float passedTime = 0;
        // interpolate position to target
        while (passedTime <= duration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / duration;

            currentScale = Vector3.Lerp(startingScale, targetScale, alpha);
            spriteMask.transform.localScale = currentScale;

            yield return null;
        }
        spriteMask.transform.localScale = targetScale;


        if (endingScale == 0)
        {
            spriteMask.SetActive(false);
        }

    }
}
