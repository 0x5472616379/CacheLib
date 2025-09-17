namespace CacheLib.Items
{
    public class ItemDefEncoder
    {
        /// <summary>
        /// Encodes an array of ItemDefinition into (obj.idx, obj.dat) pair.
        /// The encoder replays RawOpcodes for each item (lossless round-trip).
        /// </summary>
        public (byte[] idx, byte[] dat) Encode(ItemDefinition[] definitions)
        {
            using var idxStream = new MemoryStream();
            using var idxWriter = new BinaryWriter(idxStream);

            using var datStream = new MemoryStream();
            using var datWriter = new BinaryWriter(datStream);

            idxWriter.WriteUInt16BigEndian((ushort)definitions.Length);
            datWriter.WriteUInt16BigEndian((ushort)definitions.Length);

            foreach (var def in definitions)
            {
                if (def.RawOpcodes == null || def.RawOpcodes.Count == 0)
                {
                    idxWriter.WriteUInt16BigEndian(0);
                    continue;
                }

                long startPos = datStream.Position;

                foreach (var (opcode, value) in def.RawOpcodes)
                {
                    datWriter.Write(opcode);

                    switch (opcode)
                    {
                        case 0:
                            // terminator has no payload
                            break;

                        case 1: // model id (ushort)
                            datWriter.WriteUInt16BigEndian(Convert.ToUInt16(value));
                            break;

                        case 2: // name
                        case 3: // examine
                            datWriter.WriteCacheString((string)value);
                            break;

                        case 4:
                        case 5:
                        case 6:
                            datWriter.WriteUInt16BigEndian(Convert.ToUInt16(value));
                            break;

                        case 7:
                        case 8:
                            datWriter.WriteInt16BigEndian(Convert.ToInt16(value));
                            break;

                        case 10:
                            datWriter.WriteUInt16BigEndian(Convert.ToUInt16(value));
                            break;

                        case 11: // stackable, no payload
                        case 16: // members, no payload (note: original uses 16 for members)
                            break;

                        case 12:
                            datWriter.WriteInt32BigEndian(Convert.ToInt32(value));
                            break;

                        case 23:
                        case 25:
                            {
                                var tup = ((ushort, sbyte))value;
                                datWriter.WriteUInt16BigEndian(tup.Item1);
                                datWriter.Write(tup.Item2);
                                break;
                            }

                        case 24:
                        case 26:
                        case 78:
                        case 79:
                        case 90:
                        case 91:
                        case 92:
                        case 93:
                        case 95:
                        case 97:
                        case 98:
                        case 110:
                        case 111:
                        case 112:
                            datWriter.WriteUInt16BigEndian(Convert.ToUInt16(value));
                            break;

                        // 30..34 ground actions, 35..39 inventory actions
                        case byte n when (n >= 30 && n < 40):
                            datWriter.WriteCacheString((string)value);
                            break;

                        case 40:
                            {
                                // value is a ValueTuple<ushort[], ushort[]>
                                var tup = ((ushort[] src, ushort[] dst))value;
                                var src = tup.src;
                                var dst = tup.dst;
                                datWriter.Write((byte)src.Length);
                                for (int i = 0; i < src.Length; i++)
                                {
                                    datWriter.WriteUInt16BigEndian(src[i]);
                                    datWriter.WriteUInt16BigEndian(dst[i]);
                                }
                                break;
                            }

                        case byte s when (s >= 100 && s < 110):
                            {
                                // value is ValueTuple<ushort, ushort>
                                var tup = ((ushort id, ushort cnt))value;
                                datWriter.WriteUInt16BigEndian(tup.id);
                                datWriter.WriteUInt16BigEndian(tup.cnt);
                                break;
                            }

                        case 113:
                            datWriter.Write((sbyte)value);
                            break;

                        case 114:
                            datWriter.Write((sbyte)value);
                            break;

                        case 115:
                            datWriter.Write((byte)value);
                            break;

                        default:
                            throw new Exception($"Encoder missing handler for opcode {opcode}");
                    }
                }

                long endPos = datStream.Position;
                int length = (int)(endPos - startPos);
                idxWriter.WriteUInt16BigEndian((ushort)length);
            }

            return (idxStream.ToArray(), datStream.ToArray());
        }
    }
}
