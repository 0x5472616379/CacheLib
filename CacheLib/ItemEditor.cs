using System.Text;
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

        //Parse idx0 and get all entries
        var entries = idxManager.GetEntries(0);

        //Grab the config entry
        var configEntry = entries[2];

        //Use that config entry to read the config data (the archive) from the main.dat file.
        var configBuffer = archiveManager.ReadFileBlocks(configEntry);

        //Doesn't actually decompress, it just gets the bytes because config is not compressed but the files inside are.
        //But now we have the entire archive buffer which is a byte array that contains all 24 files.
        var configArchiveBuffer = archiveManager.Decompress(configBuffer);

        //This parses said byte array so we can get all the files.
        //All the files inside the archive are compressed, so we need to keep that in mind when writing it back.
        //We want to make sure to compress all the files using BZip2Helper.Compress(..); before writing them back.
        archiveManager.ParseArchiveData(configArchiveBuffer);

        //From here's we're just decoding and encoding.
        var files = archiveManager.Files;
        var objIdx = files.FirstOrDefault(x => x.Id == StringUtil.Hash("obj.idx"));
        var objDat = files.FirstOrDefault(x => x.Id == StringUtil.Hash("obj.dat"));

        ItemDefDecoder decoder = new ItemDefDecoder();
        decoder.Run(objIdx.Data, objDat.Data);
        var defs = decoder.Definitions;

        defs[1007].Name = "Text"; //Encoding.GetEncoding("ISO-8859-1").GetString("Text"u8.ToArray());
        // defs[1007].Examine = "Hello there whats";
        //ItemDefEncoderHelpers.RebuildRawOpcodes(defs[1007]);

        var encoder = new ItemDefEncoder();
        (var newIdx, var newDat) = encoder.Encode(defs);

        var idxArchiveFile = CreateArchiveFile("obj.idx", newIdx);
        var datArchiveFile = CreateArchiveFile("obj.dat", newDat);

        var idx = files.IndexOf(objIdx);
        var dat = files.IndexOf(objDat);

        files[idx] = idxArchiveFile;
        files[dat] = datArchiveFile;

        //TODO:
        //Write back the data in the correct order and in the correct places.

        Console.WriteLine();
        // verify round-trip
        // var verify = new ItemDefDecoder();
        // verify.Run(newIdx, newDat); // should not throw Unknown opcode
        // var a = verify.Definitions[1006];
        // var b = verify.Definitions[1007];
        // var c = verify.Definitions[1008];
        // var d = verify.Definitions[4151];
        //
        // File.WriteAllBytes("old_obj.dat", objDat.Data);
        // File.WriteAllBytes("old_obj.idx", objIdx.Data);
        //
        // File.WriteAllBytes("new_obj.idx", newIdx);
        // File.WriteAllBytes("new_obj.dat", newDat);
    }

    ArchiveFile CreateArchiveFile(string fileName, byte[] data)
    {
        var id = StringUtil.Hash(fileName);
        var compressedData = BZip2Helper.Compress(data);
        return new ArchiveFile(id, data, data.Length, data.Length, true);
    }
}