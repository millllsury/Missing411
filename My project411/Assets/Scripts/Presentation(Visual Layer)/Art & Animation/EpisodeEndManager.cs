using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EpisodeEndManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadePanel; 
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float musicFadeDuration = 2f;
    [SerializeField] private GameObject endEpisodeCanvas;

 

    public void EndEpisode()
    {
        FadeOutMusic();
        StartCoroutine(FadeToBlack());
        
    }

    private IEnumerator FadeToBlack()
    {
        fadePanel.interactable = true;
        fadePanel.blocksRaycasts= true;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadePanel.alpha = 1f;


        endEpisodeCanvas.SetActive(true);
        SoundManager.Instance.PlaySoundByName("end");
    }

    private void FadeOutMusic()
    {
        SoundManager.Instance.FadeOutCurrentMusic(musicFadeDuration);

    }

}
