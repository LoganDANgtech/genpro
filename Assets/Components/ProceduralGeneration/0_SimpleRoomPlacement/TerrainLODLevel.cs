using UnityEngine;

[System.Serializable]
public class TerrainLODLevel
{
    [Range(-1f, 1f)]
    public float transitionHeight = 0f;
    public string name = "LAYER";
    public Color color = Color.black;
}
