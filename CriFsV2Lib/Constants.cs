namespace CriFsV2Lib;

/// <summary>
/// Various constant values for faster reading.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Names of fields. (Prevents unnecessary string conversion)
    /// </summary>
    public static class FieldNames
    {
        /// <summary>
        /// 'DirName', stores directory name for file.
        /// </summary>
        public const long DirName = 0x656D614E726944;
        
        /// <summary>
        /// 'FileName', stores name of file.
        /// </summary>
        public const long FileName = 0x656D614E656C6946;
        
        /// <summary>
        /// 'FileSize', stores size of file in CPK (i.e. compressed).
        /// </summary>
        public const long FileSize = 0x657A6953656C6946;
        
        /// <summary>
        /// 'ExtractSi', stores size of file after extracting (i.e. uncompressed).
        /// </summary>
        public const long ExtractSi = 0x5374636172747845;

        /// <summary>
        /// 'FileOffs', offset of file data in CPK file.  
        /// </summary>
        public const long FileOffs = 0x7366664F656C6946;

        /// <summary>
        /// 'UserStri', custom string assigned to file by devs. Sometimes used for custom encryption.
        /// </summary>
        public const long UserStri = 0x6972745372657355;
        
        /// <summary>
        /// 'ContentO', offset of custom file data in CPK.
        /// </summary>
        public const long ContentO = 0x4F746E65746E6F43;
        
        /// <summary>
        /// 'TocOffse', offset of table of contents relative to start of file.
        /// </summary>
        public const long TocOffse = 0x657366664F636F54;
    }
}