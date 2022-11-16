using CriFsV2Lib.Definitions.Interfaces;
using CriFsV2Lib.Definitions.Structs;

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
