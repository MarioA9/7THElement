using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameplayScene = "Game";   // Nombre de la escena del juego

    public void Play()
    {
        // Inicia una partida desde cero
        PlayerPrefs.DeleteKey("SaveFile");
        SceneManager.LoadScene(gameplayScene);
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("SaveFile"))
        {
            SaveManager.instance.RequestLoadAfterScene(); 
            SceneManager.LoadScene(gameplayScene);
        }
        else
        {
            Debug.Log("No hay partida guardada.");
        }
    }

    public void Exit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

