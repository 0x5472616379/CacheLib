using System.IO;
using System.Text;
using CacheLib;
using CacheLib.Items;

public class ItemDefEncoder
{
    public void Run(ItemDefinition[] definitions, string outputFilePath)
    {
        try
        {
            using (var msData = new MemoryStream())
            using (var msIdx = new MemoryStream())
            using (var dataWriter = new BinaryWriter(msData))
            using (var idxWriter = new BinaryWriter(msIdx))
            {
                idxWriter.WriteUInt16BigEndian((ushort)definitions.Length);

                int totalLength = 2;

                foreach (var definition in definitions)
                {
                    var definitionBytes = Encode(definition);
                    
                    idxWriter.WriteUInt16BigEndian((ushort)definitionBytes.Length);
                    totalLength += definitionBytes.Length;
                    
                    dataWriter.Write(definitionBytes);
                }

                File.WriteAllBytes($"{outputFilePath}.dat", msData.ToArray());
                File.WriteAllBytes($"{outputFilePath}.idx", msIdx.ToArray());
            }
        }
        catch (IOException e)
        {
            throw new Exception("Error encoding ItemDefinitions.", e);
        }
    }

    public static byte[] Encode(ItemDefinition definition)
    {
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            if (definition.ModelId != 0) { 
                writer.Write((byte)1);
                writer.WriteUInt16BigEndian((ushort)definition.ModelId);
            }
            if (definition.Name != null) {
                writer.Write((byte)2);
                writer.WriteCacheString(definition.Name);
            }
            if (definition.Examine != null) {
                writer.Write((byte)3);
                writer.WriteCacheString(definition.Examine);
            }
            if (definition.IconZoom != 2000) {
                writer.Write((byte)4);
                writer.WriteUInt16BigEndian((ushort)definition.IconZoom);
            }
            if (definition.IconPitch != 0) {
                writer.Write((byte)5);
                writer.WriteUInt16BigEndian((ushort)definition.IconPitch);
            }
            if (definition.IconYaw != 0) {
                writer.Write((byte)6);
                writer.WriteUInt16BigEndian((ushort)definition.IconYaw);
            }
            if (definition.IconOffsetX != 0) {
                writer.Write((byte)7);
                writer.WriteInt16BigEndian((short)definition.IconOffsetX);
            }
            if (definition.IconOffsetY != 0) {
                writer.Write((byte)8);
                writer.WriteInt16BigEndian((short)definition.IconOffsetY);
            }
            if (definition.UnusedOpCode10 != 0) {
                writer.Write((byte)10);
                writer.WriteUInt16BigEndian((ushort)definition.UnusedOpCode10);
            }
            if (definition.Stackable) {
                writer.Write((byte)11);
            }
            if (definition.Cost != 1) {
                writer.Write((byte)12);
                writer.WriteInt32BigEndian(definition.Cost);
            }
            if (definition.Members) {
                writer.Write((byte)16);
            }
            if (definition.MaleModelId0 != -1 || definition.MaleOffsetY != 0) {
                writer.Write((byte)23);
                writer.WriteUInt16BigEndian((ushort)definition.MaleModelId0);
                writer.Write((sbyte)definition.MaleOffsetY);
            }
            if (definition.MaleModelId1 != -1) {
                writer.Write((byte)24);
                writer.WriteUInt16BigEndian((ushort)definition.MaleModelId1);
            }
            if (definition.FemaleModelId0 != -1 || definition.FemaleOffsetY != 0) {
                writer.Write((byte)25);
                writer.WriteUInt16BigEndian((ushort)definition.FemaleModelId0);
                writer.Write((sbyte)definition.FemaleOffsetY);
            }
            if (definition.FemaleModelId1 != -1) {
                writer.Write((byte)26);
                writer.WriteUInt16BigEndian((ushort)definition.FemaleModelId1);
            }

            if (definition.Options != null) {
                for (int i = 0; i < definition.Options.Length; i++) {
                    var option = definition.Options[i];
                    if (option != null) {
                        writer.Write((byte)(30 + i));
                        writer.WriteCacheString(option);
                    }
                }
            }

            if (definition.InventoryOptions != null) {
                for (int i = 0; i < definition.InventoryOptions.Length; i++) {
                    var invOption = definition.InventoryOptions[i];
                    if (invOption != null) {
                        writer.Write((byte)(35 + i));
                        writer.WriteCacheString(invOption);
                    }
                }
            }

            if (definition.SrcColor != null) {
                writer.Write((byte)40);
                writer.Write((byte)definition.SrcColor.Length);
                for (int i = 0; i < definition.SrcColor.Length; i++) {
                    writer.WriteUInt16BigEndian(definition.SrcColor[i]);
                    writer.WriteUInt16BigEndian(definition.DstColor[i]);
                }
            }

            if (definition.MaleModelId2 != -1) {
                writer.Write((byte)78);
                writer.WriteUInt16BigEndian((ushort)definition.MaleModelId2);
            }
            if (definition.FemaleModelId2 != -1) {
                writer.Write((byte)79);
                writer.WriteUInt16BigEndian((ushort)definition.FemaleModelId2);
            }
            if (definition.MaleHeadModelId0 != -1) {
                writer.Write((byte)90);
                writer.WriteUInt16BigEndian((ushort)definition.MaleHeadModelId0);
            }
            if (definition.FemaleHeadModelId0 != -1) {
                writer.Write((byte)91);
                writer.WriteUInt16BigEndian((ushort)definition.FemaleHeadModelId0);
            }
            if (definition.MaleHeadModelId1 != -1) {
                writer.Write((byte)92);
                writer.WriteUInt16BigEndian((ushort)definition.MaleHeadModelId1);
            }
            if (definition.FemaleHeadModelId1 != -1) {
                writer.Write((byte)93);
                writer.WriteUInt16BigEndian((ushort)definition.FemaleHeadModelId1);
            }
            if (definition.IconRoll != 0) {
                writer.Write((byte)95);
                writer.WriteUInt16BigEndian((ushort)definition.IconRoll);
            }
            if (definition.LinkedId != -1) {
                writer.Write((byte)97);
                writer.WriteUInt16BigEndian((ushort)definition.LinkedId);
            }
            if (definition.CertificateId != -1) {
                writer.Write((byte)98);
                writer.WriteUInt16BigEndian((ushort)definition.CertificateId);
            }

            if (definition.StackId != null) {
                for (int i = 0; i < definition.StackId.Length; i++) {
                    if (definition.StackId[i] != 0 || definition.StackCount[i] != 0) {
                        writer.Write((byte)(100 + i));
                        writer.WriteUInt16BigEndian((ushort)definition.StackId[i]);
                        writer.WriteUInt16BigEndian((ushort)definition.StackCount[i]);
                    }
                }
            }
            
            if (definition.ScaleX != 128) {
                writer.Write((byte)110);
                writer.WriteUInt16BigEndian((ushort)definition.ScaleX);
            }
            if (definition.ScaleZ != 128) {
                writer.Write((byte)111);
                writer.WriteUInt16BigEndian((ushort)definition.ScaleZ);
            }
            if (definition.ScaleY != 128) {
                writer.Write((byte)112);
                writer.WriteUInt16BigEndian((ushort)definition.ScaleY);
            }
            if (definition.LightAmbient != 0) {
                writer.Write((byte)113);
                writer.Write((sbyte)definition.LightAmbient);
            }
            if (definition.LightAttenuation != 0) {
                writer.Write((byte)114);
                writer.Write((sbyte)(definition.LightAttenuation / 5));
            }
            if (definition.Team != 0) {
                writer.Write((byte)115);
                writer.Write((byte)definition.Team);
            }

            writer.Write((byte)0);

            return ms.ToArray();
        }
    }
}