using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ObjectState
{
    public string path;      // 🔥 ruta completa dentro de la jerarquía
    public float[] position;
    public float[] rotation;
    public float[] scale;

    public bool isPlayer;
    public float playerHealth;

    public bool Voz, Tono, Ritmo, Fraseo, Diccion, Respiracion;
}

[System.Serializable]
public class HierarchyState
{
    public string id; // ID único del objeto raíz
    public List<ObjectState> states = new List<ObjectState>();
}

public class SaveObject : MonoBehaviour
{
    public bool saveChildren = true;

    private SaveID id;
    private Player player;

    void Awake()
    {
        id = GetComponent<SaveID>();
        player = GetComponent<Player>();
    }

    // -------------------------------------------------------------
    // GENERAR RUTA DEL TRANSFORM
    // -------------------------------------------------------------
    string GetPath(Transform tr)
    {
        if (tr == transform) return "root";

        List<int> indices = new List<int>();
        Transform current = tr;

        while (current != transform)
        {
            indices.Add(current.GetSiblingIndex());
            current = current.parent;
        }

        indices.Reverse();
        return string.Join("/", indices);
    }

    // -------------------------------------------------------------
    // CREAR State desde Transform
    // -------------------------------------------------------------
    ObjectState GetStateFromTransform(Transform tr)
    {
        ObjectState st = new ObjectState();
        st.path = GetPath(tr);

        st.position = new float[] { tr.position.x, tr.position.y, tr.position.z };
        st.rotation = new float[] { tr.eulerAngles.x, tr.eulerAngles.y, tr.eulerAngles.z };
        st.scale = new float[] { tr.localScale.x, tr.localScale.y, tr.localScale.z };

        // Guardado especial del Player
        if (player != null && tr == transform)
        {
            st.isPlayer = true;
            st.playerHealth = player.health;

            st.Voz = player.Voz;
            st.Tono = player.Tono;
            st.Ritmo = player.Ritmo;
            st.Fraseo = player.Fraseo;
            st.Diccion = player.Diccion;
            st.Respiracion = player.Respiracion;
        }

        return st;
    }

    // -------------------------------------------------------------
    // OBTENER TODA LA JERARQUÍA
    // -------------------------------------------------------------
    public HierarchyState GetHierarchyState()
    {
        HierarchyState hs = new HierarchyState();
        hs.id = id.uniqueID;

        foreach (Transform tr in GetComponentsInChildren<Transform>(true))
        {
            hs.states.Add(GetStateFromTransform(tr));
        }

        return hs;
    }

    // -------------------------------------------------------------
    // BUSCAR TRANSFORM A PARTIR DE RUTA
    // -------------------------------------------------------------
    Transform FindByPath(string path)
    {
        if (path == "root") return transform;

        Transform current = transform;
        string[] parts = path.Split('/');

        foreach (string p in parts)
        {
            int index = int.Parse(p);
            current = current.GetChild(index);
        }

        return current;
    }

    // -------------------------------------------------------------
    // CARGAR TODA LA JERARQUÍA
    // -------------------------------------------------------------
    public void LoadHierarchyState(HierarchyState data)
    {
        foreach (ObjectState st in data.states)
        {
            Transform tr = FindByPath(st.path);

            tr.position = new Vector3(st.position[0], st.position[1], st.position[2]);
            tr.eulerAngles = new Vector3(st.rotation[0], st.rotation[1], st.rotation[2]);
            tr.localScale = new Vector3(st.scale[0], st.scale[1], st.scale[2]);

            if (player != null && st.isPlayer)
            {
                player.health = st.playerHealth;

                player.Voz = st.Voz;
                player.Tono = st.Tono;
                player.Ritmo = st.Ritmo;
                player.Fraseo = st.Fraseo;
                player.Diccion = st.Diccion;
                player.Respiracion = st.Respiracion;
            }
        }
    }
}
