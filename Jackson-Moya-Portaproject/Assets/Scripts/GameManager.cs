using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    [Header("Music")]
    [SerializeField] AudioSource normalMusicPlayer;
    [SerializeField] AudioClip normalMusic;
    [SerializeField] AudioClip normalMusicLP;
    [SerializeField] AudioClip intenseMusic;
    [SerializeField] AudioClip intenseMusicLP;

    public bool isPaused = false;
    public bool isMuted = false;

    private bool destroy = false;
    private int lastSceneIndex = -1;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // destroy any attempts to have two instances of the singleton
            Destroy(gameObject);
            destroy = true;
        }
        else
        {
            Instance = this;
            
            // make sure this doesn't reload when a new scene is loaded
            DontDestroyOnLoad(this.gameObject);
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        // prevent code from running in duplicate instance
        if (destroy)
        {
            return;
        }

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
                lightMask.transform.localScale = Vector3.zero;
                lightMask.transform.parent = colParent.transform;
                Vector3 maskPos = Camera.main.ViewportToWorldPoint(new Vector2(i, j));
                maskPos.z = 0;
                lightMask.transform.position = maskPos;
                lightMask.SetActive(false);
            }
        }

        if (SceneManager.GetActiveScene().buildIndex < 4 || SceneManager.GetActiveScene().buildIndex == 21)
        {
            normalMusicPlayer.clip = normalMusic;
        }
        else
        {
            normalMusicPlayer.clip = intenseMusic;
        }
        normalMusicPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused)
        {
            return;
        }

        // debug next level
        if (Input.GetKeyDown(KeyCode.End))
        {
            NextLevel();
        }

        // mute music
        if (Input.GetKeyDown(KeyCode.M))
        {
            isMuted = !isMuted;
            normalMusicPlayer.mute = !normalMusicPlayer.mute;
        }

        // pause
        if (Input.GetKeyDown(KeyCode.Return) && !isPaused)
        {
            //isPaused = true;
            StartCoroutine(Pause());
        }
    }

    void OnSceneChanged(Scene current, Scene next)
    {
        //Debug.Log($"{lastSceneIndex} | {next.buildIndex}");
        if (lastSceneIndex != -1 && lastSceneIndex != next.buildIndex)
        {
            Instance.StartCoroutine(NewLevelLoadedCoroutine());
        }

        if (next.name == "Level 3")
        {
            StartCoroutine(FadeoutMusic(1f));
        }
        else if (next.name == "Level 4")
        {
            normalMusicPlayer.clip = intenseMusic;
            normalMusicPlayer.time = 0;
            normalMusicPlayer.volume = 1;
            normalMusicPlayer.Play();
        } 
        else if (next.name == "Level 21")
        {
            screenTint.color = noTint;
            normalMusicPlayer.clip = normalMusic;
            normalMusicPlayer.time = 0;
            normalMusicPlayer.volume = 1;
            normalMusicPlayer.Play();
        }


        lastSceneIndex = next.buildIndex;
    }

    public void ResetStage()
    {
        Instance.StartCoroutine(ResetStageCoroutine());
    }



    public void NextLevel()
    {
        if (SceneManager.GetActiveScene().name == "Level 20")
        {
            StartCoroutine(FadeoutMusic(1f));
            StartCoroutine(FadeStage(.8f, 0, 1));
            StartCoroutine(FinalLevelTransition());
        }
        else
        { 
            Instance.StartCoroutine(NextLevelCoroutine());
        }
    }


    private IEnumerator FinalLevelTransition()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    private IEnumerator Pause()
    {
        Debug.Log("Pause Coroutine");
        screenTint.color = pauseTint;
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0f;
        isPaused = true;
        normalMusicPlayer.Pause();
        while (isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                isPaused = false;
            }
            yield return null;
        }
        screenTint.color = noTint;
        Time.timeScale = 1f;
        normalMusicPlayer.UnPause();
    }


    private IEnumerator ResetStageCoroutine()
    {
        // tint screen
        screenTint.color = redTint;
        yield return new WaitForEndOfFrame();


        // pause game with time scale
        Time.timeScale = 0f;
        isPaused = true;


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
        isPaused = false;

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


    private IEnumerator FadeoutMusic(float duration)
    {
        float startingVol = normalMusicPlayer.volume;
        float currentVol;
        float targetVol = 0;
        float passedTime = 0;
        // interpolate position to target
        while (passedTime <= duration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / duration;

            currentVol = Mathf.Lerp(startingVol, targetVol, alpha);
            normalMusicPlayer.volume = currentVol;

            yield return null;
        }
        normalMusicPlayer.volume = targetVol;
    }


    private IEnumerator FadeStage(float duration, float startA, float targetA)
    {
        Color startingColor = new Color (0, 0, 0, startA);
        Color currentColor = startingColor;
        Color targetColor = new Color(0, 0, 0, targetA);
        float passedTime = 0;
        // interpolate position to target
        while (passedTime <= duration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / duration;

            currentColor.a = Mathf.Lerp(startingColor.a, targetColor.a, alpha);
            screenTint.color = currentColor;

            yield return null;
        }
        screenTint.color = targetColor;
    }



    public void ChangeToLowpassMusic()
    {
        float time = normalMusicPlayer.time;
        if (normalMusicPlayer.clip == normalMusic)
        {
            normalMusicPlayer.clip = normalMusicLP;
        } 
        else if (normalMusicPlayer.clip == intenseMusic)
        {
            normalMusicPlayer.clip = intenseMusicLP;
        }
        normalMusicPlayer.time = time;
        normalMusicPlayer.Play();
    }


    public void ChangeToRegularMusic()
    {
        float time = normalMusicPlayer.time;
        if (normalMusicPlayer.clip == normalMusicLP)
        {
            normalMusicPlayer.clip = normalMusic;
        }
        else if (normalMusicPlayer.clip == intenseMusicLP)
        {
            normalMusicPlayer.clip = intenseMusic;
        }
        normalMusicPlayer.time = time;
        normalMusicPlayer.Play();
    }
}
