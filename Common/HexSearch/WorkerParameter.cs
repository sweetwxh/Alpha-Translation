#region ÒıÓÃ

using System.IO;

#endregion

namespace HexSearch
{
    public class WorkerParameter
    {
        public int StartIndex { get; set; }

        public FileInfo[] Files { get; set; }

        public byte[] SearchData { get; set; }

        public int[] Next { get; set; }
    }
}