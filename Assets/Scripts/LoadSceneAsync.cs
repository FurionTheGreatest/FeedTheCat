using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoadSceneAsync : MonoBehaviour
{
    public Animator transition;

    private readonly int _startTrigger = Animator.StringToHash("Start");

    private const float TransitionTime = 1f;

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

        SceneManager.LoadSceneAsync(levelIndex);
    }
    
    public void QuitApplication()
    {
        Application.Quit();
    }
}
