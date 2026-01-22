using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    public bool destroyed = false;

    public void DestroyAndMark()
    {
        destroyed = true;
        gameObject.SetActive(false); // 🔥 NO Destroy inmediato
    }
}
