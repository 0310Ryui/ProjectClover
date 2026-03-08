using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterial", menuName = "LittleDollAtelier/MaterialData")]
public class MaterialData : ScriptableObject
{
    public string materialId;
    public string materialName;
    public Sprite icon;
    public int rarity;
}
