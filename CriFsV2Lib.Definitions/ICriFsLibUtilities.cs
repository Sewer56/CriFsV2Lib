using CriFsV2Lib.Definitions.Structs;

namespace CriFsV2Lib.Definitions;

/// <summary>
/// Utility functions that don't belong in any sort of category.
/// </summary>
public interface ICriFsLibUtilities
{
    /// <summary>
    /// Gets all of the files present within a CPK file from a byte array.  
    /// Use when entire CPK is in memory.  
    /// </summary>
    /// <param name="dataPtr">Pointer to the start of the CPK data in memory.</param>
    public unsafe CpkFile[] GetFiles(byte* dataPtr);
}