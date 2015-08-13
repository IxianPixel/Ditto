// -----------------------------------------------------------------------
// <copyright file="FileComparer.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using Alphaleonis.Win32.Filesystem;

    public class FileComparer
    {
        public FileInfo SourceFile { get; set; }

        public FileInfo DestinationInfo { get; set; }


        public FileComparer(FileInfo sourceFile, FileInfo destinationFile)
        {
            this.SourceFile = sourceFile;
            this.DestinationInfo = destinationFile;

        }

        public bool Compare()
        {
            var filesAreEqual = false;

            var datesAreEqual = this.DatesAreEqual();
            var sizesAreEqual = this.SizeIsEqual();

            if (datesAreEqual && sizesAreEqual)
            {
                filesAreEqual = true;
            }

            return filesAreEqual;
        }

        private bool DatesAreEqual()
        {
            return !(this.SourceFile.LastWriteTime != this.DestinationInfo.LastWriteTime);
        }

        private bool SizeIsEqual()
        {
            return this.SourceFile.Length == this.DestinationInfo.Length;
        }
    }
}