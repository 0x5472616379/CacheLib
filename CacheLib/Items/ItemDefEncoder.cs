namespace CacheLib.Items;

public class ItemDefEncoder
{
    public static void EncodeDefinition(ItemDefinition def, BinaryWriter writer)
    {
        if (def == null)
            throw new ArgumentNullException(nameof(def));
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        if (def.ModelId != 0)
        {
            writer.Write((byte)1);
            writer.WriteInt16BigEndian((short)def.ModelId);
        }
        
        if (!string.IsNullOrEmpty(def.Name))
        {
            writer.Write((byte)2);
            writer.WriteSafeString(def.Name);
        }
        
        if (!string.IsNullOrEmpty(def.Examine))
        {
            writer.Write((byte)3);
            writer.WriteSafeString(def.Examine);
        }
        
        if (def.IconZoom != 2000) // Default is 2000
        {
            writer.Write((byte)4);
            writer.WriteInt16BigEndian((short)def.IconZoom);
        }
        
        if (def.IconPitch != 0)
        {
            writer.Write((byte)5);
            writer.WriteInt16BigEndian((short)def.IconPitch);
        }
        
        if (def.IconYaw != 0)
        {
            writer.Write((byte)6);
            writer.WriteInt16BigEndian((short)def.IconYaw);
        }
        
        if (def.IconOffsetX != 0)
        {
            writer.Write((byte)7);
            writer.WriteInt16BigEndian((short)def.IconOffsetX);
        }
        
        if (def.IconOffsetY != 0)
        {
            writer.Write((byte)8);
            writer.WriteInt16BigEndian((short)def.IconOffsetY);
        }
        
        if (def.Stackable)
        {
            writer.Write((byte)11);
        }
        
        if (def.Cost != 1) // Default is 1
        {
            writer.Write((byte)12);
            writer.WriteInt32BigEndian(def.Cost);
        }
        
        if (def.Members)
        {
            writer.Write((byte)16);
        }
        
        if (def.MaleModelId0 != -1 || def.MaleOffsetY != 0)
        {
            writer.Write((byte)23);
            writer.WriteInt16BigEndian((short)def.MaleModelId0);
            writer.Write((byte)def.MaleOffsetY);
        }
        
        if (def.MaleModelId1 != -1)
        {
            writer.Write((byte)24);
            writer.WriteInt16BigEndian((short)def.MaleModelId1);
        }
        
        if (def.FemaleModelId0 != -1 || def.FemaleOffsetY != 0)
        {
            writer.Write((byte)25);
            writer.WriteInt16BigEndian((short)def.FemaleModelId0);
            writer.Write((byte)def.FemaleOffsetY);
        }
        
        if (def.FemaleModelId1 != -1)
        {
            writer.Write((byte)26);
            writer.WriteInt16BigEndian(0); // Unknown value often seen in original data
            writer.WriteInt16BigEndian((short)def.FemaleModelId1);
        }
        
        if (def.Options != null)
        {
            for (int i = 0; i < Math.Min(5, def.Options.Length); i++)
            {
                if (!string.IsNullOrEmpty(def.Options[i]))
                {
                    writer.Write((byte)(30 + i));
                    writer.WriteSafeString(def.Options[i]);
                }
            }
        }
        
        if (def.InventoryOptions != null)
        {
            for (int i = 0; i < Math.Min(5, def.InventoryOptions.Length); i++)
            {
                if (!string.IsNullOrEmpty(def.InventoryOptions[i]))
                {
                    writer.Write((byte)(35 + i));
                    writer.WriteSafeString(def.InventoryOptions[i]);
                }
            }
        }
        
        if (def.SrcColor != null && def.DstColor != null && 
            def.SrcColor.Length > 0 && def.SrcColor.Length == def.DstColor.Length)
        {
            writer.Write((byte)40);
            writer.Write((byte)def.SrcColor.Length);
            for (int i = 0; i < def.SrcColor.Length; i++)
            {
                writer.WriteInt16BigEndian((short)def.SrcColor[i]);
                writer.WriteInt16BigEndian((short)def.DstColor[i]);
            }
        }
        
        if (def.MaleModelId2 != -1)
        {
            writer.Write((byte)78);
            writer.WriteInt16BigEndian((short)def.MaleModelId2);
        }
        
        if (def.FemaleModelId2 != -1)
        {
            writer.Write((byte)79);
            writer.WriteInt16BigEndian((short)def.FemaleModelId2);
        }
        
        if (def.MaleHeadModelId0 != -1)
        {
            writer.Write((byte)90);
            writer.WriteInt16BigEndian((short)def.MaleHeadModelId0);
        }
        
        if (def.FemaleHeadModelId0 != -1)
        {
            writer.Write((byte)91);
            writer.WriteInt16BigEndian((short)def.FemaleHeadModelId0);
        }
        
        if (def.MaleHeadModelId1 != -1)
        {
            writer.Write((byte)92);
            writer.WriteInt16BigEndian((short)def.MaleHeadModelId1);
        }
        
        if (def.FemaleHeadModelId1 != -1)
        {
            writer.Write((byte)93);
            writer.WriteInt16BigEndian((short)def.FemaleHeadModelId1);
        }
        
        if (def.IconRoll != 0)
        {
            writer.Write((byte)95);
            writer.WriteInt16BigEndian((short)def.IconRoll);
        }
        
        if (def.LinkedId != -1)
        {
            writer.Write((byte)97);
            writer.WriteInt16BigEndian((short)def.LinkedId);
        }
        
        if (def.CertificateId != -1)
        {
            writer.Write((byte)98);
            writer.WriteInt16BigEndian((short)def.CertificateId);
        }
        
        if (def.StackId != null && def.StackCount != null)
        {
            for (int i = 0; i < Math.Min(10, def.StackId.Length); i++)
            {
                if (i < def.StackCount.Length && (def.StackId[i] != 0 || def.StackCount[i] != 0))
                {
                    writer.Write((byte)(100 + i));
                    writer.WriteInt16BigEndian((short)def.StackId[i]);
                    writer.WriteInt16BigEndian((short)def.StackCount[i]);
                }
            }
        }
        
        if (def.ScaleX != 128) // Default is 128
        {
            writer.Write((byte)110);
            writer.WriteInt16BigEndian((short)def.ScaleX);
        }
        
        if (def.ScaleZ != 128)
        {
            writer.Write((byte)111);
            writer.WriteInt16BigEndian((short)def.ScaleZ);
        }
        
        if (def.ScaleY != 128)
        {
            writer.Write((byte)112);
            writer.WriteInt16BigEndian((short)def.ScaleY);
        }
        
        if (def.LightAmbient != 0)
        {
            writer.Write((byte)113);
            writer.Write((byte)def.LightAmbient);
        }
        
        if (def.LightAttenuation != 0)
        {
            writer.Write((byte)114);
            writer.Write((byte)(def.LightAttenuation / 5)); // Stored as 1/5th of actual value
        }
        
        if (def.Team != 0)
        {
            writer.Write((byte)115);
            writer.Write((byte)def.Team);
        }
        
        writer.Write((byte)0);
    }
}