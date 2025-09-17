using CacheLib.Items;

namespace CacheLib;

public static class ItemDefEncoderHelpers
{
    public static void RebuildRawOpcodes(ItemDefinition def)
    {
        def.RawOpcodes.Clear();

        // 1 = ModelId
        if (def.ModelId != 0)
            def.RawOpcodes.Add((1, (ushort)def.ModelId));

        // 2 = Name
        if (!string.IsNullOrEmpty(def.Name))
            def.RawOpcodes.Add((2, def.Name));

        // 3 = Examine
        if (!string.IsNullOrEmpty(def.Examine))
            def.RawOpcodes.Add((3, def.Examine));

        // 4,5,6 = IconZoom, IconPitch, IconYaw
        if (def.IconZoom != 0)
            def.RawOpcodes.Add((4, (ushort)def.IconZoom));
        if (def.IconPitch != 0)
            def.RawOpcodes.Add((5, (ushort)def.IconPitch));
        if (def.IconYaw != 0)
            def.RawOpcodes.Add((6, (ushort)def.IconYaw));

        // 7,8 = IconOffsetX/Y
        if (def.IconOffsetX != 0)
            def.RawOpcodes.Add((7, (short)def.IconOffsetX));
        if (def.IconOffsetY != 0)
            def.RawOpcodes.Add((8, (short)def.IconOffsetY));

        // 10 = UnusedOpCode10
        if (def.UnusedOpCode10 != 0)
            def.RawOpcodes.Add((10, (ushort)def.UnusedOpCode10));

        // 11 = Stackable
        if (def.Stackable)
            def.RawOpcodes.Add((11, null));

        // 12 = Cost
        if (def.Cost != 1)
            def.RawOpcodes.Add((12, def.Cost));

        // 16 = Members
        if (def.Members)
            def.RawOpcodes.Add((16, null));

        // 23-26 = Male/Female models and offsets
        if (def.MaleModelId0 != -1) def.RawOpcodes.Add((23, ((ushort)def.MaleModelId0, def.MaleOffsetY)));
        if (def.MaleModelId1 != -1) def.RawOpcodes.Add((24, (ushort)def.MaleModelId1));
        if (def.FemaleModelId0 != -1) def.RawOpcodes.Add((25, ((ushort)def.FemaleModelId0, def.FemaleOffsetY)));
        if (def.FemaleModelId1 != -1) def.RawOpcodes.Add((26, (ushort)def.FemaleModelId1));

        // 30-34 Options
        if (def.Options != null)
            for (int i = 0; i < def.Options.Length; i++)
                if (!string.IsNullOrEmpty(def.Options[i]))
                    def.RawOpcodes.Add(((byte)(30 + i), def.Options[i]));

        // 35-39 InventoryOptions
        if (def.InventoryOptions != null)
            for (int i = 0; i < def.InventoryOptions.Length; i++)
                if (!string.IsNullOrEmpty(def.InventoryOptions[i]))
                    def.RawOpcodes.Add(((byte)(35 + i), def.InventoryOptions[i]));

        // 40 = Recolors
        if (def.SrcColor != null && def.DstColor != null && def.SrcColor.Length > 0)
            def.RawOpcodes.Add((40, (def.SrcColor, def.DstColor)));

        // 100-109 = StackIds
        if (def.StackId != null && def.StackCount != null)
            for (int i = 0; i < def.StackId.Length; i++)
                if (def.StackId[i] != 0 || def.StackCount[i] != 0)
                    def.RawOpcodes.Add(((byte)(100 + i), ((ushort)def.StackId[i], (ushort)def.StackCount[i])));

        // 110-112 = Scale
        if (def.ScaleX != 128) def.RawOpcodes.Add((110, (ushort)def.ScaleX));
        if (def.ScaleZ != 128) def.RawOpcodes.Add((111, (ushort)def.ScaleZ));
        if (def.ScaleY != 128) def.RawOpcodes.Add((112, (ushort)def.ScaleY));

        // 113-115 = Light & Team
        if (def.LightAmbient != 0) def.RawOpcodes.Add((113, (sbyte)def.LightAmbient));
        if (def.LightAttenuation != 0) def.RawOpcodes.Add((114, (sbyte)def.LightAttenuation));
        if (def.Team != 0) def.RawOpcodes.Add((115, (byte)def.Team));

        // LinkedId and CertificateId
        if (def.LinkedId != -1) def.RawOpcodes.Add((97, (ushort)def.LinkedId));
        if (def.CertificateId != -1) def.RawOpcodes.Add((98, (ushort)def.CertificateId));

        // Always add terminator
        def.RawOpcodes.Add((0, null));
    }
}
