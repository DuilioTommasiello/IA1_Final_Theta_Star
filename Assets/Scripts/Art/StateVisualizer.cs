using UnityEngine;
using System.Collections.Generic;

public class StateVisualizer : MonoBehaviour
{
    [System.Serializable]
    public struct StateSprite
    {
        public string stateName;   // Nombre del estado (ej. "Idle", "Move", "Attack")
        public Sprite sprite;       // Icono correspondiente
    }

    [Header("Configuración")]
    [SerializeField] private StateSprite[] stateSprites;
    [SerializeField] private float heightOffset = 2.5f;   // Altura sobre el agente
    [SerializeField] private float spriteSize = 1.5f;      // Tamaño del icono
    [SerializeField] private bool alwaysFaceCamera = true; // Si el sprite siempre mira a la cámara

    private SpriteRenderer spriteRenderer;
    private Agent agent;
    private Dictionary<string, Sprite> spriteDict;

    private void Awake()
    {
        agent = GetComponent<Agent>();
        if (agent == null)
        {
            Debug.LogError("StateVisualizer necesita un componente Agent en el mismo GameObject.");
            enabled = false;
            return;
        }

        // Crear el GameObject hijo para el icono
        GameObject iconGO = new GameObject("StateIcon");
        iconGO.transform.SetParent(transform);
        iconGO.transform.localPosition = new Vector3(0, heightOffset, 0);
        iconGO.transform.localScale = Vector3.one * spriteSize;

        spriteRenderer = iconGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 10; // Para que se vea por encima

        // Construir diccionario de sprites por estado
        spriteDict = new Dictionary<string, Sprite>();
        foreach (var item in stateSprites)
        {
            if (!spriteDict.ContainsKey(item.stateName))
                spriteDict.Add(item.stateName, item.sprite);
            else
                Debug.LogWarning($"Estado duplicado en StateVisualizer: {item.stateName}");
        }
    }

    private void LateUpdate()
    {
        if (agent == null || spriteRenderer == null) return;

        // Obtener nombre del estado actual
        string stateName = agent.GetCurrentStateName();

        // Cambiar sprite según el estado
        if (spriteDict.TryGetValue(stateName, out Sprite spr))
        {
            spriteRenderer.sprite = spr;
        }
        else
        {
            spriteRenderer.sprite = null;
        }

        // Hacer que el sprite mire a la cámara (billboard)
        if (alwaysFaceCamera && Camera.main != null)
        {
            spriteRenderer.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }
}