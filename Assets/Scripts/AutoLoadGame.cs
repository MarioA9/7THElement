using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AutoLoadGame : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadAfterSceneReady());
    }

    IEnumerator LoadAfterSceneReady()
    {
        // Esperar a que termine todo
        yield return new WaitForSeconds(0.2f);

        // Esperar que Unity registre TODOS los objetos de la escena
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (PlayerPrefs.HasKey("SaveFile"))
        {
            Debug.Log("Aplicando archivo guardado...");
            SaveManager.instance.LoadGame();
        }
    }
}

