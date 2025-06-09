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

    /* Modify the name and save */
    var exampleItem = decoder.Definitions[1007];
    Console.WriteLine(exampleItem.Name);
}
finally
{
    cache.Close();
}