using CacheLib;
using CacheLib.Items;

/* Place cache in bin folder that appears when building the project (RiderProjects\CacheLib\CacheLib\bin\Debug\net8.0\cache) */
var cachePath = "cache";
var cache = new CacheReader(cachePath);

try
{
    // 1. Read existing archive
    byte[] archiveData = cache.ReadFile(CacheConstants.ArchiveIndex, 2);
    var archive = Archive.Load(archiveData);

    // 2. Get item definitions
    var objIdx = archive.GetFiles()[Archive.Hash("obj.idx")].Data;
    var objDat = archive.GetFiles()[Archive.Hash("obj.dat")].Data;

    ItemDefDecoder decoder = new ItemDefDecoder();
    decoder.Run(objIdx, objDat);

    Printer.PrintArchive(archive.GetFiles());
    
    // 3. Modify item definition
    decoder.Definitions[1007].Name = "Cape of Exiles";
    
    // 4. Re-encode item definitions
    var (newObjIdx, newObjDat) = ItemDefEncoder.Encode(decoder.Definitions);

}
finally
{
    cache.Close();
}