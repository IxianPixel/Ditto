﻿// -----------------------------------------------------------------------
// <copyright file="FileComparer.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using System.IO;

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
            bool areEqual;

            areEqual = this.DatesAreEqual();
            areEqual = this.SizeIsEqual();

            return areEqual;
        }

        private bool DatesAreEqual()
        {
            var areEqual = true;

            if (this.SourceFile.LastWriteTime != this.DestinationInfo.LastWriteTime)
            {
                areEqual = false;
            }

            if (this.SourceFile.CreationTime != this.DestinationInfo.CreationTime)
            {
                areEqual = false;
            }

            return areEqual;
        }

        private bool SizeIsEqual()
        {
            return this.SourceFile.Length == this.DestinationInfo.Length;
        }
    }
}