// -----------------------------------------------------------------------
// <copyright file="JobSource.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Model
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class JobSource
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "source", Required = Required.Always)]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "destination", Required = Required.Always)]
        public string Destination { get; set; }

        [JsonProperty(PropertyName = "folder_exceptions", Required = Required.Always)]
        public List<string> FolderExceptions { get; set; }

        [JsonProperty(PropertyName = "file_exceptions", Required = Required.Always)]
        public List<string> FileExceptions { get; set; }

    }
}