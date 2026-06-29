using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameplayScene = "Game";   // Nombre de la escena del juego

    public void Play()
    {
        SceneManager.LoadScene(gameplayScene);
    }

    public void Load()
    {

    }

    public void Exit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

