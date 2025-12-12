using UnityEngine;

public class SaveDestroy : MonoBehaviour
{
    SaveID id;

    private void Awake()
    {
        id = GetComponent<SaveID>();
    }

    private void OnDestroy()
    {
        if (SaveManager.instance != null)
            SaveManager.instance.RegistrarObjetoEliminado(id.uniqueID);
    }
}
