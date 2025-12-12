using UnityEngine;

public class SaveButton : MonoBehaviour
{
    public void Guardar()
    {
        SaveManager.instance.SaveGame();
    }
}
