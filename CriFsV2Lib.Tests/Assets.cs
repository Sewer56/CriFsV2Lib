namespace FileEmulationFramework.Tests;

public static class Assets
{
    public static string AssetsFolder = "Assets";
    public static string SampleCpkFolder = $"{AssetsFolder}/SampleData";
    public static string SampleCpkFile = $"{AssetsFolder}/SampleData.cpk";
    public static string SampleCpkEncryptedFile = $"{AssetsFolder}/SampleData-Encrypted.cpk";
    public static string SampleCompressedTextFile = $"{AssetsFolder}/CompressedText.crilayla";
    public static string SampleCompressedModelFile = $"{AssetsFolder}/Compressed3dModel.crilayla";
    public static string SampleUncompressedTextFile = $"{AssetsFolder}/SampleData/Text-Compressed.txt";
    public static string SampleUncompressedImageFile = $"{AssetsFolder}/SampleData/Image-NoCompression.jpg";
    public static string SampleUncompressedModelFile = $"{AssetsFolder}/Uncompressed3DModel.dff";
    
    public static string SampleDecryptedTable = $"{AssetsFolder}/DecyptedTable.@utf";
    public static string SampleEncryptedTable = $"{AssetsFolder}/EncryptedTable.@utf";
}
