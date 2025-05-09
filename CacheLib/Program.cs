using CacheLib;
using CacheLib.Items;

/* Place cache in bin folder that appears when building the project (RiderProjects\CacheLib\CacheLib\bin\Debug\net8.0\cache) */
var cachePath = "cache";
var cache = new CacheReader(cachePath);

try
{
    /* 1. Read existing archive */
    byte[] archiveData = cache.ReadFile(CacheConstants.ArchiveIndex, 2);
    var archive = Archive.Load(archiveData);

    /* 2. Get item definitions */
    var objIdx = archive.GetFiles()[Archive.Hash("obj.idx")].Data;
    var objDat = archive.GetFiles()[Archive.Hash("obj.dat")].Data;

    ItemDefDecoder decoder = new ItemDefDecoder();
    decoder.Run(objIdx, objDat);

    /* 3. Modify item definition */
    decoder.Definitions[1007].Name = "Cape of Exiles";

    /* Re encode item definitions */
    var (newObjIdx, newObjDat) = ItemDefEncoder.Encode(decoder.Definitions);

    /* 5. Compress the updated files */
    byte[] compressedObjIdx = BZip2Helper.Compress(newObjIdx);
    byte[] compressedObjDat = BZip2Helper.Compress(newObjDat);

    /* 6. Update the archive */
    archive.GetFiles()[Archive.Hash("obj.idx")].Data = compressedObjIdx;
    archive.GetFiles()[Archive.Hash("obj.idx")].CompressedSize = compressedObjIdx.Length;
    archive.GetFiles()[Archive.Hash("obj.idx")].DecompressedSize = newObjIdx.Length;

    archive.GetFiles()[Archive.Hash("obj.dat")].Data = compressedObjDat;
    archive.GetFiles()[Archive.Hash("obj.dat")].CompressedSize = compressedObjDat.Length;
    archive.GetFiles()[Archive.Hash("obj.dat")].DecompressedSize = newObjDat.Length;

    /* 7. Rebuild the archive data */
    byte[] newArchiveData = RebuildArchive(archive);

    /* 8. Write back to cache */
    cache.WriteFile(CacheConstants.ArchiveIndex, 2, newArchiveData);

    Console.WriteLine("Successfully updated item definition and repacked cache!");
}
finally
{
    cache.Close();
}

static byte[] RebuildArchive(Archive archive)
{
    using (var ms = new MemoryStream())
    using (var bw = new BinaryWriter(ms))
    {
        /* First write the file count */
        bw.WriteUInt16BigEndian((ushort)archive.GetFiles().Count);

        /*  Write all file entries first (id, decompressed size, compressed size) */
        foreach (var file in archive.GetFiles().Values.OrderBy(f => f.Id))
        {
            bw.WriteInt32BigEndian(file.Id);
            bw.WriteMedium(file.DecompressedSize);
            bw.WriteMedium(file.CompressedSize);
        }

        /* Then write all file data */
        foreach (var file in archive.GetFiles().Values.OrderBy(f => f.Id))
        {
            bw.Write(file.Data);
        }

        return ms.ToArray();
    }
}