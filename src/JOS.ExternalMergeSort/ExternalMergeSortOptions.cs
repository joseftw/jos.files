using System.Collections.Generic;

namespace JOS.ExternalMergeSort
{
    public class ExternalMergeSortOptions
    {
        public ExternalMergeSortOptions()
        {
            Split = new ExternalMergeSortSplitOptions();
            Sort = new ExternalMergeSortSortOptions();
            Merge = new ExternalMergeSortMergeOptions();
        }

        public string FileLocation { get; init; } = "c:\\temp\\files";
        public ExternalMergeSortSplitOptions Split { get; init; }
        public ExternalMergeSortSortOptions Sort { get; init; }
        public ExternalMergeSortMergeOptions Merge { get; init; }
    }

    public class ExternalMergeSortSplitOptions
    {
        /// <summary>
        /// Size of unsorted file (chunk) (in bytes)
        /// </summary>
        public int ChunkSize { get; init; } = 2 * 1024 * 1024;
        public char NewLineSeparator { get; init; } = '\n';
    }

    public class ExternalMergeSortSortOptions
    {
        public IComparer<string> Comparer { get; init; } = Comparer<string>.Default;
        public int InputBufferSize { get; init; } = 65536;
        public int OutputBufferSize { get; init; } = 65536;
    }

    public class ExternalMergeSortMergeOptions
    {
        public int ChunkSize { get; init; } = 10;
        /// <summary>
        /// Buffer size (in bytes) for input StreamReaders
        /// </summary>
        public int InputBufferSize { get; init; } = 65536;
        /// <summary>
        /// Buffer size (in bytes) for output StreamWriter
        /// </summary>
        public int OutputBufferSize { get; init; } = 65536;
    }

}