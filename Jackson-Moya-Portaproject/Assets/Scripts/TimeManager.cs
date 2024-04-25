using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    public float timer = 0f;
    public float Timer => timer; // Public getter to access the timer

    private Coroutine timerCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevents the timer from being destroyed between scenes
        }
        else
        {
            Destroy(gameObject); // Ensures only one instance exists
        }
    }

    private void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            timer += 1f;
            Debug.Log("Timer: " + timer); // Optional: For debugging

            // Check if the active scene is "Level 21"
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level 21")
            {
                // If it's "Level 21", stop incrementing the timer but don't stop the coroutine
                Debug.Log("Timer stopped because active scene is 'Level 21'.");
                break;
            }
        }
    }
}