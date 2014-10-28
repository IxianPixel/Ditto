// -----------------------------------------------------------------------
// <copyright file="MirrorInfo.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Model
{
    using Ditto.Core;

    public class MirrorInfo
    {
        public int FoldersCreated { get; set; }

        public int FoldersDeleted { get; set; }

        public int FoldersSkipped { get; set; }

        public int FolderErrors { get; set; }

        public int FilesCreated { get; set; }

        public int FilesUpdated { get; set; }

        public int FilesDeleted { get; set; }

        public int FilesSkipped { get; set; }

        public int FileErrors { get; set; }

        public int SourceFolders { get; set; }
        
        public int SourceFiles { get; set; }

        public int DestinationFolders { get; set; }

        public int DestinationFiles { get; set; }

        public long SourceSize;

        public long DestinationSize;

        public string SourceSizeString
        {
            get
            {
                return SourceSize.ToSizeString();
            }
        }

        public string DestinationSizeString
        {
            get
            {
                return DestinationSize.ToSizeString();
            }
        }
    }
}