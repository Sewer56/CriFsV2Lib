using CriFsV2Lib.Definitions;
using CriFsV2Lib.Definitions.Structs;
using CriFsV2Lib.Definitions.Utilities;
using CriFsV2Lib.Utilities.Parsing;

namespace CriFsV2Lib;

/// <inheritdoc />
public class CpkReader : ICpkReader
{
    /// <inheritdoc />
    public InPlaceDecryptionFunction? Decrypt { get; }

    /// <summary>
    /// The underlying stream.
    /// </summary>
    private Stream _stream;
    
    /// <summary>
    /// Position stream was when instance was created.
    /// </summary>
    private long _initialStreamPosition;

    private bool _ownsStream;

    /// <summary>
    /// Creates a CPK reader used for decryption of data.
    /// </summary>
    /// <param name="decrypt">The function used to decrypt game files.</param>
    /// <param name="stream">The stream to read data from.</param>
    /// <param name="ownsStream">True to dispose stream along with reader, else false.</param>
    public CpkReader(Stream stream, bool ownsStream, InPlaceDecryptionFunction? decrypt)
    {
        Decrypt = decrypt;
        _stream = stream;
        _initialStreamPosition = _stream.Position;
        _ownsStream = ownsStream;
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsStream)
            _stream.Dispose();
    }

    /// <inheritdoc />
    public ArrayRental ExtractFileNoDecompression(in CpkFile file, out bool needsDecompression)
    {
        _stream.Position = _initialStreamPosition;
        return CpkHelper.ExtractFileNoDecompression(file, _stream, out needsDecompression, Decrypt);
    }

    /// <inheritdoc />
    public ArrayRental ExtractFile(in CpkFile file)
    {
        _stream.Position = _initialStreamPosition;
        return CpkHelper.ExtractFile(file, _stream, Decrypt);
    }

    /// <inheritdoc />
    public CpkFile[] GetFiles()
    {
        _stream.Position = _initialStreamPosition;
        return CpkHelper.GetFilesFromStream(_stream);
    }
}