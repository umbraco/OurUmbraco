﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Semver;

namespace OurUmbraco.Repository.Models
{
    public class DocumentationVersion
    {
        public string Path { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public bool IsCurrentVersion { get; set; }
        public bool IsCurrentPage { get; internal set; }
        public SemVersion VersionFrom { get; internal set; }
        public SemVersion VersionTo { get; internal set; }
    }
}
