// -----------------------------------------------------------------------
// <copyright file="JobDetails.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Model
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class JobDetails
    {
        [JsonProperty(PropertyName = "sources", Required = Required.Always)]
        public List<JobSource> Sources { get; set; }

        [JsonProperty(PropertyName = "destination", Required = Required.Always)]
        public string Destination { get; set; }

        [JsonProperty(PropertyName = "subject", Required = Required.Default)]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "smtp", Required = Required.Default)]
        public string Smtp { get; set; }

        [JsonProperty(PropertyName = "identify", Required = Required.Always)]
        public bool Identify { get; set; }

        [JsonProperty(PropertyName = "job_name", Required = Required.Always)]
        public string JobName { get; set; }

        [JsonProperty(PropertyName = "sender", Required = Required.Default)]
        public string Sender { get; set; }

        [JsonProperty(PropertyName = "recipients", Required = Required.Default)]
        public List<string> Recipients { get; set; }
    }
}