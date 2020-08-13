using System;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadSceneAsync : MonoBehaviour
{
    public Animator transition;

    private readonly int _startTrigger = Animator.StringToHash("Start");

    private const float TransitionTime = 1f;

    public TMP_Text exit;

    private WaitForSeconds _waitForLabelShow = new WaitForSeconds(2f);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(ShowLabel());
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
        transition.SetTrigger(_startTrigger);
        
        yield return new WaitForSeconds(TransitionTime);

        SceneManager.LoadScene(levelIndex);
    }

    private IEnumerator ShowLabel()
    {
        exit.enabled = true;
        yield return _waitForLabelShow;
        exit.enabled = false;
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
}
