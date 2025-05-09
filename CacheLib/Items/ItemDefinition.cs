namespace CacheLib.Items;

public class ItemDefinition
{
    public int ModelId { get; set; }
    public string Name { get; set; }
    public string Examine { get; set; }
    public int IconZoom { get; set; }
    public int IconPitch { get; set; }
    public int IconYaw { get; set; }
    public int IconOffsetX { get; set; }
    public int IconOffsetY { get; set; }
    public bool Stackable { get; set; }
    public int Cost { get; set; }
    public bool Members { get; set; }
    
    public int MaleModelId0 { get; set; }
    public int MaleModelId1 { get; set; }
    public int MaleModelId2 { get; set; }
    public int MaleHeadModelId0 { get; set; }
    public int MaleHeadModelId1 { get; set; }
    public int MaleOffsetY { get; set; }

    public int FemaleModelId0 { get; set; }
    public int FemaleModelId1 { get; set; }
    public int FemaleModelId2 { get; set; }
    public int FemaleHeadModelId0 { get; set; }
    public int FemaleHeadModelId1 { get; set; }
    public int FemaleOffsetY { get; set; }

    public string[] Options { get; set; } = new string[5];
    public string[] InventoryOptions { get; set; } = new string[5];

    public int[] SrcColor { get; set; }
    public int[] DstColor { get; set; }

    public int IconRoll { get; set; }
    public int LinkedId { get; set; }
    public int CertificateId { get; set; }

    public int[] StackId { get; set; } = new int[10];
    public int[] StackCount { get; set; } = new int[10];

    public int ScaleX { get; set; }
    public int ScaleY { get; set; }
    public int ScaleZ { get; set; }

    public int LightAmbient { get; set; }
    public int LightAttenuation { get; set; }

    public int Team { get; set; }
}