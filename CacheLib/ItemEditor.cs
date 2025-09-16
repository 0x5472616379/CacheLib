using CacheLib.Items;

namespace CacheLib;

public class ItemEditor
{
    string cachePath = "cache";
    ArchiveManager archiveManager = new();

    public ItemEditor()
    {
        TestEncoderIntegrity();
        Console.WriteLine();
    }

    private void TestEncoderIntegrity()
    {
        using var idxManager = new IndexManager();
        idxManager.Initialize(cachePath);
        archiveManager.Initialize(cachePath, idxManager);
        var entries = idxManager.GetEntries(0);
        var configEntry = entries[2];
        var configBuffer = archiveManager.ReadFileBlocks(configEntry);
        var configFileBuffer = archiveManager.Decompress(configBuffer);
        archiveManager.ParseArchiveData(configFileBuffer);
        var files = archiveManager.Files;
        var objIdx = files.FirstOrDefault(x => x.Id == StringUtil.Hash("obj.idx"));
        var objDat = files.FirstOrDefault(x => x.Id == StringUtil.Hash("obj.dat"));
    
        ItemDefDecoder decoder = new ItemDefDecoder();
        decoder.Run(objIdx!.Data, objDat!.Data);
        var defs = decoder.Definitions;
    
        File.WriteAllBytes("old_obj.dat", objDat.Data);
        File.WriteAllBytes("old_obj.idx", objIdx.Data);
    
        ItemDefEncoder encoder = new ItemDefEncoder();
        encoder.Run(defs, "new_obj");
    
        byte[] originalDat = File.ReadAllBytes("old_obj.dat");
        byte[] newDat = File.ReadAllBytes("new_obj.dat");
        byte[] originalIdx = File.ReadAllBytes("old_obj.idx");
        byte[] newIdx = File.ReadAllBytes("new_obj.idx");

    }
}