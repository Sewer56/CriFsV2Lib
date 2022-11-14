using CriFsV2Lib.Structs;
using CriFsV2Lib.Utilities;

namespace CriFsLib.GUI.Model;

internal class CpkFileExtractorItemModel : IBatchFileExtractorItem
{
    /// <inheritdoc/>
    public string FullPath { get; set; }

    /// <inheritdoc/>
    public CpkFile File => Model.File;

    /// <inheritdoc/>
    public CpkFileModel Model { get; init; }

    public CpkFileExtractorItemModel(string fullPath, CpkFileModel model)
    {
        FullPath = fullPath;
        Model = model;
    }
}
