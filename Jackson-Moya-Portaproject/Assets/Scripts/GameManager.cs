using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Color redTint = new Color32(165, 165, 255, 20);
    private Color noTint = new Color(1, 1, 1, 0);
    //[SerializeField] private Color blueTint = new Color32(255, 255, 255, 255);  // I don't think this is needed for this implementation

    private SpriteRenderer screenTint;

    public TMP_Text text;

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
        if (SceneManager.GetActiveScene().name == "Level 21")
        {
            text.text = "IN CASE YOU'RE INTERESTED, YOUR TIME WAS: " + TimerManager.Instance.Timer;
        }
        else
        {
            text.text = null;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ResetStage()
    {
        StartCoroutine(ResetStageCoroutine());
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
}
