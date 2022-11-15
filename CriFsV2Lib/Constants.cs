namespace CriFsV2Lib;

/// <summary>
/// Various constant values for faster reading.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Names of fields. (Prevents unnecessary string conversion)
    /// </summary>
    public static unsafe class Fields
    {
        /// <summary>
        /// 'DirName', stores directory name for file.
        /// </summary>
        public static bool IsDirName(byte* ptr) => *(long*) ptr == 0x656D614E726944;

        /// <summary>
        /// 'FileName', stores name of file.
        /// </summary>
        public static bool IsFileName(byte* ptr) => 
            *(long*)ptr == 0x656D614E656C6946 // 'FileName'
            && *(ptr + 0x08) == 0x00; // \0

        /// <summary>
        /// 'FileSize', stores size of file in CPK (i.e. compressed).
        /// </summary>
        public static bool IsFileSize(byte* ptr) =>
            *(long*)ptr == 0x657A6953656C6946 // 'FileSize'
            && *(ptr + 0x08) == 0x00; // \0.

        /// <summary>
        /// 'ExtractSize', stores size of file after extracting (i.e. uncompressed).
        /// </summary>
        public static bool IsExtractSize(byte* ptr) =>
            *(long*)ptr == 0x5374636172747845 // 'ExtractSi'
            && *(int*)(ptr + 0x08) == 0x00657A69; // 'ize\0'

        /// <summary>
        /// 'FileOffset', offset of file data in CPK file.  
        /// </summary>
        public static bool IsFileOffset(byte* ptr) =>
            *(long*)ptr == 0x7366664F656C6946 // 'FileOffs'
            && *(int*)(ptr + 0x07) == 0x746573; // 'set\0'

        /// <summary>
        /// 'UserString', custom string assigned to file by devs. Sometimes used for custom encryption.
        /// </summary>
        public static bool IsUserString(byte* ptr) =>
            *(long*)ptr == 0x6972745372657355 // 'UserStri'
            && *(int*)(ptr + 0x07) == 0x676E69; // 'ing\0'

        /// <summary>
        /// 'ContentOffset', offset of custom file data in CPK.
        /// </summary>
        public static bool IsContentOffset(byte* ptr) =>
            *(long*)ptr == 0x4F746E65746E6F43 // 'ContentO'
            && *(long*)(ptr + 0x06) == 0x74657366664F74; // 'tOffset\0'

        /// <summary>
        /// 'TocOffset', offset of table of contents relative to start of file.
        /// </summary>
        public static bool IsTocOffset(byte* ptr) =>
            *(long*)ptr == 0x657366664F636F54 // 'TocOffse'
            && *(short*)(ptr + 0x08) == 0x74; // 't\0'
    }
}