// -----------------------------------------------------------------------
// <copyright file="ByteUtility.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using Ditto.Properties;

    public static class ByteUtility
    {
        private static readonly string[] SizeFormats =
        {
            Resources.SizeB, Resources.SizeKB, Resources.SizeMB, Resources.SizeGB,
            Resources.SizeTB, Resources.SizePB, Resources.SizeEB, Resources.SizeZB,
            Resources.SizeYB
        };

        public static string ToSizeString(this long size)
        {
            var i = 0;
            double s = size;

            while (i < SizeFormats.Length && s > 1024)
            {
                i++;
                s /= 1024;
            }

            return string.Format(SizeFormats[i], s);
        }
    }
}