using System.IO;
using CacheLib;
using CacheLib.Items;

public class ItemDefDecoder
{
    public ItemDefinition[] Definitions { get; set; }

    public void Run(byte[] idx, byte[] data)
    {
        try
        {
            using (var msData = new MemoryStream(data))
            using (var msIdx = new MemoryStream(idx))

            using (var dataReader = new BinaryReader(msData))
            using (var idxReader = new BinaryReader(msIdx))
            {
                int count = idxReader.ReadUInt16BigEndian();
                var indices = new int[count];
                int index = 2;

                for (var i = 0; i < count; i++)
                {
                    indices[i] = index;
                    index += idxReader.ReadUInt16BigEndian();
                }

                Definitions = new ItemDefinition[count];
                for (var i = 0; i < count; i++)
                {
                    dataReader.BaseStream.Position = indices[i];
                    Definitions[i] = Decode(i, dataReader);
                }
            }
        }
        catch (IOException e)
        {
            throw new Exception("Error decoding ItemDefinitions.", e);
        }
    }

    public static ItemDefinition Decode(int id, BinaryReader buffer)
    {
        var definition = new ItemDefinition();
        definition.Id = id;
        definition.Reset();

        while (true)
        {
            var opcode = buffer.ReadByte();
            if (opcode == 0)
            {
                return definition;
            }
            else if (opcode == 1)
            {
                definition.ModelId = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 2)
            {
                definition.Name = buffer.ReadCacheString();
            }
            else if (opcode == 3)
            {
                definition.Examine = buffer.ReadCacheString();
            }
            else if (opcode == 4)
            {
                definition.IconZoom = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 5)
            {
                definition.IconPitch = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 6)
            {
                definition.IconYaw = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 7)
            {
                definition.IconOffsetX = buffer.ReadInt16BigEndian();
            }
            else if (opcode == 8)
            {
                definition.IconOffsetY = buffer.ReadInt16BigEndian();
            }
            else if (opcode == 10)
            {
                definition.UnusedOpCode10 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 11)
            {
                definition.Stackable = true;
            }
            else if (opcode == 12)
            {
                definition.Cost = buffer.ReadInt32BigEndian();
            }
            else if (opcode == 16)
            {
                definition.Members = true;
            }
            else if (opcode == 23)
            {
                definition.MaleModelId0 = buffer.ReadUInt16BigEndian();
                definition.MaleOffsetY = buffer.ReadSByte();
            }
            else if (opcode == 24)
            {
                definition.MaleModelId1 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 25)
            {
                definition.FemaleModelId0 = buffer.ReadUInt16BigEndian();
                definition.FemaleOffsetY = buffer.ReadSByte();
            }
            else if (opcode == 26)
            {
                definition.FemaleModelId1 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode >= 30 && opcode < 35)
            {
                if (definition.Options == null)
                {
                    definition.Options = new string[5];
                }

                var str = buffer.ReadCacheString();
                if (str.Equals("hidden", StringComparison.OrdinalIgnoreCase)) str = null;
                definition.Options[opcode - 30] = str;
            }
            else if (opcode >= 35 && opcode < 40)
            {
                if (definition.InventoryOptions == null)
                {
                    definition.InventoryOptions = new string[5];
                }

                definition.InventoryOptions[opcode - 35] = buffer.ReadCacheString();
            }
            else if (opcode == 40)
            {
                int recolorCount = buffer.ReadByte();
                definition.SrcColor = new ushort[recolorCount];
                definition.DstColor = new ushort[recolorCount];
                for (var i = 0; i < recolorCount; i++)
                {
                    definition.SrcColor[i] = buffer.ReadUInt16BigEndian();
                    definition.DstColor[i] = buffer.ReadUInt16BigEndian();
                }
            }
            else if (opcode == 78)
            {
                definition.MaleModelId2 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 79)
            {
                definition.FemaleModelId2 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 90)
            {
                definition.MaleHeadModelId0 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 91)
            {
                definition.FemaleHeadModelId0 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 92)
            {
                definition.MaleHeadModelId1 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 93)
            {
                definition.FemaleHeadModelId1 = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 95)
            {
                definition.IconRoll = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 97)
            {
                definition.LinkedId = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 98)
            {
                definition.CertificateId = buffer.ReadUInt16BigEndian();
            }
            else if (opcode >= 100 && opcode < 110)
            {
                if (definition.StackId == null)
                {
                    definition.StackId = new int[10];
                    definition.StackCount = new int[10];
                }

                definition.StackId[opcode - 100] = buffer.ReadUInt16BigEndian();
                definition.StackCount[opcode - 100] = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 110)
            {
                definition.ScaleX = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 111)
            {
                definition.ScaleZ = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 112)
            {
                definition.ScaleY = buffer.ReadUInt16BigEndian();
            }
            else if (opcode == 113)
            {
                definition.LightAmbient = buffer.ReadSByte();
            }
            else if (opcode == 114)
            {
                definition.LightAttenuation = buffer.ReadSByte() * 5;
            }
            else if (opcode == 115)
            {
                definition.Team = buffer.ReadByte();
            }
            else
            {
                throw new Exception($"Unknown opcode: {opcode}");
            }
        }
    }
}