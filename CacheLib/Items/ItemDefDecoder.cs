using System.IO;
using CacheLib;
using CacheLib.Items;

namespace CacheLib.Items
{
    public class ItemDefDecoder
    {
        public ItemDefinition[] Definitions { get; private set; }

        public void Run(byte[] idx, byte[] data)
        {
            try
            {
                using var msData = new MemoryStream(data);
                using var msIdx = new MemoryStream(idx);
                using var dataReader = new BinaryReader(msData);
                using var idxReader = new BinaryReader(msIdx);

                
                int count = idxReader.ReadUInt16BigEndian();
                var indices = new int[count];
                int index = 2; // data files start after a 2-byte count in dat
                for (var i = 0; i < count; i++)
                {
                    indices[i] = index;
                    index += idxReader.ReadUInt16BigEndian();
                }

                Definitions = new ItemDefinition[count];
                for (var i = 0; i < count; i++)
                {
                    if (indices[i] <= 0 || indices[i] >= dataReader.BaseStream.Length)
                    {
                        // empty entry
                        Definitions[i] = new ItemDefinition { Id = i };
                        Definitions[i].Reset();
                        continue;
                    }

                    dataReader.BaseStream.Position = indices[i];
                    Definitions[i] = Decode(i, dataReader);
                }
            }
            catch (IOException e)
            {
                throw new Exception("Error decoding ItemDefinitions.", e);
            }
        }

        private ItemDefinition Decode(int id, BinaryReader buffer)
        {
            var definition = new ItemDefinition();
            definition.Id = id;
            definition.Reset();

            while (true)
            {
                var opcode = buffer.ReadByte();

                if (opcode == 0)
                {
                    // terminator store and return
                    definition.RawOpcodes.Add((opcode, null));
                    return definition;
                }

                switch (opcode)
                {
                    case 1:
                        {
                            ushort model = buffer.ReadUInt16BigEndian();
                            definition.ModelId = model;
                            definition.RawOpcodes.Add((opcode, model));
                            break;
                        }
                    case 2:
                        {
                            string name = buffer.ReadCacheString();
                            definition.Name = name;
                            definition.RawOpcodes.Add((opcode, name));
                            break;
                        }
                    case 3:
                        {
                            string examine = buffer.ReadCacheString();
                            definition.Examine = examine;
                            definition.RawOpcodes.Add((opcode, examine));
                            break;
                        }
                    case 4:
                        {
                            ushort zoom = buffer.ReadUInt16BigEndian();
                            definition.IconZoom = zoom;
                            definition.RawOpcodes.Add((opcode, zoom));
                            break;
                        }
                    case 5:
                        {
                            ushort pitch = buffer.ReadUInt16BigEndian();
                            definition.IconPitch = pitch;
                            definition.RawOpcodes.Add((opcode, pitch));
                            break;
                        }
                    case 6:
                        {
                            ushort yaw = buffer.ReadUInt16BigEndian();
                            definition.IconYaw = yaw;
                            definition.RawOpcodes.Add((opcode, yaw));
                            break;
                        }
                    case 7:
                        {
                            short x = (short)buffer.ReadInt16BigEndian();
                            definition.IconOffsetX = x;
                            definition.RawOpcodes.Add((opcode, x));
                            break;
                        }
                    case 8:
                        {
                            short y = (short)buffer.ReadInt16BigEndian();
                            definition.IconOffsetY = y;
                            definition.RawOpcodes.Add((opcode, y));
                            break;
                        }
                    case 10:
                        {
                            ushort v = buffer.ReadUInt16BigEndian();
                            definition.UnusedOpCode10 = v;
                            definition.RawOpcodes.Add((opcode, v));
                            break;
                        }
                    case 11:
                        {
                            definition.Stackable = true;
                            definition.RawOpcodes.Add((opcode, null));
                            break;
                        }
                    case 12:
                        {
                            int cost = buffer.ReadInt32BigEndian();
                            definition.Cost = cost;
                            definition.RawOpcodes.Add((opcode, cost));
                            break;
                        }
                    case 16:
                        {
                            definition.Members = true;
                            definition.RawOpcodes.Add((opcode, null));
                            break;
                        }
                    case 23:
                        {
                            ushort m = buffer.ReadUInt16BigEndian();
                            sbyte off = buffer.ReadSByte();
                            definition.MaleModelId0 = m;
                            definition.MaleOffsetY = off;
                            definition.RawOpcodes.Add((opcode, (m, off)));
                            break;
                        }
                    case 24:
                        {
                            ushort m = buffer.ReadUInt16BigEndian();
                            definition.MaleModelId1 = m;
                            definition.RawOpcodes.Add((opcode, m));
                            break;
                        }
                    case 25:
                        {
                            ushort f = buffer.ReadUInt16BigEndian();
                            sbyte off = buffer.ReadSByte();
                            definition.FemaleModelId0 = f;
                            definition.FemaleOffsetY = off;
                            definition.RawOpcodes.Add((opcode, (f, off)));
                            break;
                        }
                    case 26:
                        {
                            ushort f = buffer.ReadUInt16BigEndian();
                            definition.FemaleModelId1 = f;
                            definition.RawOpcodes.Add((opcode, f));
                            break;
                        }
                    // ground options 30-34
                    case byte n when (n >= 30 && n < 35):
                        {
                            string opt = buffer.ReadCacheString();
                            // store raw string so encoder can replay exactly (some entries use "hidden")
                            definition.RawOpcodes.Add((n, opt));
                            if (definition.Options == null) definition.Options = new string[5];
                            definition.Options[n - 30] = string.Equals(opt, "hidden", StringComparison.OrdinalIgnoreCase) ? null : opt;
                            break;
                        }
                    // inventory options 35-39
                    case byte n2 when (n2 >= 35 && n2 < 40):
                        {
                            string iopt = buffer.ReadCacheString();
                            definition.RawOpcodes.Add((n2, iopt));
                            if (definition.InventoryOptions == null) definition.InventoryOptions = new string[5];
                            definition.InventoryOptions[n2 - 35] = iopt;
                            break;
                        }
                    case 40:
                        {
                            int recolorCount = buffer.ReadByte();
                            var src = new ushort[recolorCount];
                            var dst = new ushort[recolorCount];
                            for (int i = 0; i < recolorCount; i++)
                            {
                                src[i] = buffer.ReadUInt16BigEndian();
                                dst[i] = buffer.ReadUInt16BigEndian();
                            }
                            definition.SrcColor = src;
                            definition.DstColor = dst;
                            definition.RawOpcodes.Add((opcode, (src, dst)));
                            break;
                        }
                    case 78:
                        {
                            definition.MaleModelId2 = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.MaleModelId2));
                            break;
                        }
                    case 79:
                        {
                            definition.FemaleModelId2 = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.FemaleModelId2));
                            break;
                        }
                    case 90:
                        {
                            definition.MaleHeadModelId0 = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.MaleHeadModelId0));
                            break;
                        }
                    case 91:
                        {
                            definition.FemaleHeadModelId0 = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.FemaleHeadModelId0));
                            break;
                        }
                    case 92:
                        {
                            definition.MaleHeadModelId1 = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.MaleHeadModelId1));
                            break;
                        }
                    case 93:
                        {
                            definition.FemaleHeadModelId1 = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.FemaleHeadModelId1));
                            break;
                        }
                    case 95:
                        {
                            definition.IconRoll = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.IconRoll));
                            break;
                        }
                    case 97:
                        {
                            definition.LinkedId = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.LinkedId));
                            break;
                        }
                    case 98:
                        {
                            definition.CertificateId = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.CertificateId));
                            break;
                        }
                    // stack table 100-109
                    case byte s when (s >= 100 && s < 110):
                        {
                            int slot = s - 100;
                            if (definition.StackId == null)
                            {
                                definition.StackId = new int[10];
                                definition.StackCount = new int[10];
                            }
                            int sid = buffer.ReadUInt16BigEndian();
                            int scount = buffer.ReadUInt16BigEndian();
                            definition.StackId[slot] = sid;
                            definition.StackCount[slot] = scount;
                            definition.RawOpcodes.Add((s, ((ushort)sid, (ushort)scount)));
                            break;
                        }
                    case 110:
                        {
                            definition.ScaleX = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.ScaleX));
                            break;
                        }
                    case 111:
                        {
                            definition.ScaleZ = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.ScaleZ));
                            break;
                        }
                    case 112:
                        {
                            definition.ScaleY = buffer.ReadUInt16BigEndian();
                            definition.RawOpcodes.Add((opcode, (ushort)definition.ScaleY));
                            break;
                        }
                    case 113:
                        {
                            sbyte amb = buffer.ReadSByte();
                            definition.LightAmbient = amb;
                            definition.RawOpcodes.Add((opcode, amb));
                            break;
                        }
                    case 114:
                        {
                            sbyte attRaw = buffer.ReadSByte(); // store raw
                            definition.LightAttenuation = attRaw * 5;
                            definition.RawOpcodes.Add((opcode, attRaw));
                            break;
                        }
                    case 115:
                        {
                            definition.Team = buffer.ReadByte();
                            definition.RawOpcodes.Add((opcode, (byte)definition.Team));
                            break;
                        }

                    default:
                        throw new Exception($"Unknown opcode: {opcode} at item {id}");
                }
            }
        }
    }
}
