using System;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadSceneAsync : MonoBehaviour
{
    public Animator transition;
    
    public TMP_Text exit;

    private readonly int _startTrigger = Animator.StringToHash("Start");

    private const float TransitionTime = 1f;
    private const float TimeToExit = 2f;
    private WaitForSeconds _waitForLabelShow = new WaitForSeconds(TimeToExit);
    private bool _isExitEnabled;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && !_isExitEnabled)
        {
            _isExitEnabled = true;
            if (exit == null) return;
            StartCoroutine(ShowLabel());

            StartCoroutine(nameof(ExitApplicationFromMenu));
        }
    }
    public void LoadScene(int sceneBuildIndex)
    {
        StartCoroutine(LoadLevel(sceneBuildIndex));
    }
    
    public void ReloadScene()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex));
    }
    
    public void LoadNextScene()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        Time.timeScale = 1f;
        transition.SetTrigger(_startTrigger);
        
        yield return new WaitForSeconds(TransitionTime);

        SceneManager.LoadScene(levelIndex);
    }

    private IEnumerator ShowLabel()
    {
        exit.enabled = true;
        yield return _waitForLabelShow;
        exit.enabled = false;
        _isExitEnabled = false;
    }

    private IEnumerator ExitApplicationFromMenu()
    {
        yield return null;
        float counter = 0;
        
        while (counter < TimeToExit)
        {
            counter += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }

            yield return null;
        }
    }
    public void QuitApplication()
    {
        EnergyManager.Instance.SaveTimeStamp();
        Application.Quit();
    }
}
