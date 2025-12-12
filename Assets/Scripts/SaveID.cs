using UnityEngine;
using System;

public class SaveID : MonoBehaviour
{
    public string uniqueID;

    private void Reset()
    {
        // Se ejecuta al crear el componente / prefab
        uniqueID = Guid.NewGuid().ToString();
    }

    private void OnEnable()
    {
        AssignID();
    }

    private void AssignID()
    {
        if (!string.IsNullOrEmpty(uniqueID))
            return;

        string key = "ID_" + gameObject.scene.name + "_" + gameObject.name + "_" + transform.GetSiblingIndex();

        if (PlayerPrefs.HasKey(key))
        {
            uniqueID = PlayerPrefs.GetString(key);
        }
        else
        {
            uniqueID = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(key, uniqueID);
            PlayerPrefs.Save();
        }
    }
}
