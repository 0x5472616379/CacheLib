﻿namespace CacheLib.Items;

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
                int count = idxReader.ReadInt16BigEndian(), index = 2;
                var indices = new int[count];
                for (var i = 0; i < count; i++)
                {
                    indices[i] = index;
                    index += idxReader.ReadInt16BigEndian();
                }

                Definitions = new ItemDefinition[count];
                for (var i = 0; i < count; i++)
                {
                    dataReader.BaseStream.Position = indices[i];
                    Definitions[i] = Decode(dataReader);
                }
            }
        }
        catch (IOException e)
        {
            throw new Exception("Error decoding ItemDefinitions.", e);
        }
    }
    
    public static ItemDefinition Decode(BinaryReader buffer)
    {
        var definition = new ItemDefinition();

        while (true)
        {
            var opcode = buffer.ReadByte() & 0xFF;

            if (opcode == 0)
            {
                return definition;
            }

            if (opcode == 1)
            {
                definition.ModelId = buffer.ReadInt16();
            }
            else if (opcode == 2)
            {
                definition.Name = buffer.ReadSafeString();
            }
            else if (opcode == 3)
            {
                definition.Examine = buffer.ReadSafeString();
            }
            else if (opcode == 4)
            {
                definition.IconZoom = buffer.ReadInt16();
            }
            else if (opcode == 5)
            {
                definition.IconPitch = buffer.ReadInt16();
            }
            else if (opcode == 6)
            {
                definition.IconYaw = buffer.ReadInt16();
            }
            else if (opcode == 7)
            {
                definition.IconOffsetX = buffer.ReadInt16();
                if (definition.IconOffsetX > 32767)
                {
                    definition.IconOffsetX -= 0x10000;
                }
            }
            else if (opcode == 8)
            {
                definition.IconOffsetY = buffer.ReadInt16();
                if (definition.IconOffsetY > 32767)
                {
                    definition.IconOffsetY -= 0x10000;
                }
            }
            else if (opcode == 10)
            {
                buffer.ReadInt16();
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
                definition.MaleModelId0 = buffer.ReadInt16();
                definition.MaleOffsetY = buffer.ReadByte();
            }
            else if (opcode == 24)
            {
                definition.MaleModelId1 = buffer.ReadInt16();
            }
            else if (opcode == 25)
            {
                definition.FemaleModelId0 = buffer.ReadInt16();
                definition.FemaleOffsetY = buffer.ReadByte();
            }
            else if (opcode == 26)
            {
                buffer.ReadInt16();
                definition.FemaleModelId1 = buffer.ReadInt16();
            }
            else if (opcode >= 30 && opcode < 35)
            {
                var str = buffer.ReadSafeString();
                if (str.Equals("hidden", StringComparison.OrdinalIgnoreCase)) str = null;

                var idx = opcode - 30;
                var action = str;
                definition.Options[idx] = action;
            }
            else if (opcode >= 35 && opcode < 40)
            {
                var idx = opcode - 35;
                var action = buffer.ReadSafeString();
                definition.InventoryOptions[idx] = action;
            }
            else if (opcode == 40)
            {
                int recolorCount = buffer.ReadByte();
                definition.SrcColor = new int[recolorCount];
                definition.DstColor = new int[recolorCount];
                for (var i = 0; i < recolorCount; i++)
                {
                    definition.SrcColor[i] = buffer.ReadInt16();
                    definition.DstColor[i] = buffer.ReadInt16();
                }
            }
            else if (opcode == 78)
            {
                definition.MaleModelId2 = buffer.ReadInt16();
            }
            else if (opcode == 79)
            {
                definition.FemaleModelId2 = buffer.ReadInt16();
            }
            else if (opcode == 90)
            {
                definition.MaleHeadModelId0 = buffer.ReadInt16();
            }
            else if (opcode == 91)
            {
                definition.FemaleModelId0 = buffer.ReadInt16();
            }
            else if (opcode == 92)
            {
                definition.MaleHeadModelId1 = buffer.ReadInt16();
            }
            else if (opcode == 93)
            {
                definition.FemaleHeadModelId1 = buffer.ReadInt16();
            }
            else if (opcode == 95)
            {
                definition.IconRoll = buffer.ReadInt16();
            }
            else if (opcode == 97)
            {
                definition.LinkedId = buffer.ReadInt16();
            }
            else if (opcode == 98)
            {
                definition.CertificateId = buffer.ReadInt16();
            }
            else if (opcode >= 100 && opcode < 110)
            {
                if (definition.StackId == null) {
                    definition.StackId = new int[10];
                    definition.StackCount = new int[10];
                }
                
                definition.StackId[opcode - 100] = buffer.ReadInt16();
                definition.StackCount[opcode - 100] = buffer.ReadInt16();
            }
            else if (opcode == 110)
            {
                definition.ScaleX = buffer.ReadInt16();
            }
            else if (opcode == 111)
            {
                definition.ScaleZ = buffer.ReadInt16();
            }
            else if (opcode == 112)
            {
                definition.ScaleY = buffer.ReadInt16();
            }
            else if (opcode == 113)
            {
                definition.LightAmbient = buffer.ReadByte();
            }
            else if (opcode == 114)
            {
                definition.LightAttenuation = buffer.ReadByte();
            }
            else if (opcode == 115)
            {
                definition.Team = buffer.ReadByte();
            }
        }
    }
}