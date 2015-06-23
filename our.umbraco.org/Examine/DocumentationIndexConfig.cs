﻿using System.Configuration;

namespace our.Examine
{
    public sealed class DocumentationIndexConfig : ConfigurationSection
    {
        private static DocumentationIndexConfig settings = ConfigurationManager.GetSection("DocumentationIndexConfig") as DocumentationIndexConfig;

        public static DocumentationIndexConfig Settings { get { return settings; } }
        
        [ConfigurationProperty("SupportedFileTypes", IsRequired = true)]
        public string SupportedFileTypes
        {
            get { return (string)this["SupportedFileTypes"]; }
            set { this["SupportedFileTypes"] = value; }
        }

        [ConfigurationProperty("DirectoryToIndex", IsRequired = true)]
        public string DirectoryToIndex
        {
            get { return (string)this["DirectoryToIndex"]; }
            set { this["DirectoryToIndex"] = value; }
        }

        [ConfigurationProperty("IgnoreFiles")]
        public string IgnoreFiles
        {
            get { return (string)this["IgnoreFiles"]; }
            set { this["IgnoreFiles"] = value; }
        }

        [ConfigurationProperty("Recursive")]
        public bool Recursive
        {
            get { return (bool)this["Recursive"]; }
            set { this["Recursive"] = value; }
        }
    }
}
