using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private bool shouldLoadAfterScene = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 🔥 ESCUCHAMOS CAMBIO DE ESCENA
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RequestLoadAfterScene()
    {
        shouldLoadAfterScene = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si esta escena es Game y venimos desde Load del menú
        if (shouldLoadAfterScene)
        {
            shouldLoadAfterScene = false;
            Debug.Log("Aplicando partida guardada al cargar la escena...");

            LoadGame();
        }
    }

    [System.Serializable]
    public class SaveFileData
    {
        public List<HierarchyState> objects = new List<HierarchyState>();
    }

    SaveFileData currentData = new SaveFileData();

    public void SaveGame()
    {
        SaveObject[] objects = FindObjectsOfType<SaveObject>();
        currentData = new SaveFileData();

        foreach (var obj in objects)
        {
            currentData.objects.Add(obj.GetHierarchyState());
        }

        string json = JsonUtility.ToJson(currentData);
        PlayerPrefs.SetString("SaveFile", json);
        PlayerPrefs.Save();

        Debug.Log("Juego guardado con jerarquías.");
    }
    
    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("SaveFile"))
            return;

        string json = PlayerPrefs.GetString("SaveFile");
        currentData = JsonUtility.FromJson<SaveFileData>(json);

        Dictionary<string, SaveObject> mapScene = new Dictionary<string, SaveObject>();

        foreach (var so in FindObjectsOfType<SaveObject>())
            mapScene[so.GetComponent<SaveID>().uniqueID] = so;

        Debug.Log("Objetos en escena: " + mapScene.Count);
        Debug.Log("Objetos en archivo: " + currentData.objects.Count);

        // 🔥🔥🔥 DESPUÉS: cargar posiciones/rotaciones/escala jerárquicas
        foreach (var entry in currentData.objects)
        {
            if (mapScene.ContainsKey(entry.id))
            {
                mapScene[entry.id].LoadHierarchyState(entry);
            }
        }

        Debug.Log("Partida cargada (con jerarquías y eliminados).");
    }


}
