namespace CacheLib;

public class ArchiveWriter
{
    private FileStream _dataFile;
    private int _nextAvailableBlock = 1;
    
    public void Initialize(string outputDataFilePath)
    {
        _dataFile = new FileStream(outputDataFilePath, FileMode.Create, FileAccess.ReadWrite);
        WriteEmptyBlock(0);
    }
    
    public IndexEntry WriteFile(byte[] fileData, int fileId)
    {
        int startBlock = _nextAvailableBlock;
        int currentBlock = startBlock;
        int bytesWritten = 0;
        int chunkNumber = 0;
        int totalSize = fileData.Length;
        
        Console.WriteLine($"Writing file {fileId} (size: {totalSize} bytes) starting at block {startBlock}");
        
        while (bytesWritten < totalSize)
        {
            int bytesToWrite = Math.Min(totalSize - bytesWritten, CacheConstants.ChunkSize);
            int nextBlock = (bytesWritten + bytesToWrite < totalSize) ? _nextAvailableBlock + 1 : 0;
            
            byte[] chunkData = new byte[bytesToWrite];
            Array.Copy(fileData, bytesWritten, chunkData, 0, bytesToWrite);
            
            WriteDataBlock(currentBlock, fileId, chunkNumber, nextBlock, chunkData);
            
            bytesWritten += bytesToWrite;
            chunkNumber++;
            currentBlock = _nextAvailableBlock;
            _nextAvailableBlock++;
        }
        
        return new IndexEntry(totalSize, startBlock);
    }
    
    private void WriteDataBlock(int blockNumber, int fileId, int chunkNumber, int nextBlock, byte[] data)
    {
        long blockPosition = blockNumber * CacheConstants.BlockSize;
        _dataFile.Seek(blockPosition, SeekOrigin.Begin);
        
        byte[] header = new byte[8];
        header[0] = (byte)((fileId >> 8) & 0xFF);     
        header[1] = (byte)(fileId & 0xFF);            
        header[2] = (byte)((chunkNumber >> 8) & 0xFF);
        header[3] = (byte)(chunkNumber & 0xFF);       
        header[4] = (byte)((nextBlock >> 16) & 0xFF); 
        header[5] = (byte)((nextBlock >> 8) & 0xFF);  
        header[6] = (byte)(nextBlock & 0xFF);         
        header[7] = (byte)CacheBlockType.Uncompressed;
        
        _dataFile.Write(header, 0, header.Length);
        
        // Write data (512 bytes max, pad with zeros if needed)
        _dataFile.Write(data, 0, data.Length);
        
        // Pad with zeros if data is less than 512 bytes
        if (data.Length < CacheConstants.ChunkSize)
        {
            byte[] padding = new byte[CacheConstants.ChunkSize - data.Length];
            _dataFile.Write(padding, 0, padding.Length);
        }
    }
    
    private void WriteEmptyBlock(int blockNumber)
    {
        long blockPosition = blockNumber * CacheConstants.BlockSize;
        _dataFile.Seek(blockPosition, SeekOrigin.Begin);
        
        byte[] emptyBlock = new byte[CacheConstants.BlockSize];
        _dataFile.Write(emptyBlock, 0, emptyBlock.Length);
    }
    
    public void Close()
    {
        _dataFile?.Close();
        _dataFile = null;
    }
}