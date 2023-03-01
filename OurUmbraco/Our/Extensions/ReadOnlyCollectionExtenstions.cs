using System.Collections.Generic;
using System.Linq;
using System.Text;
using OurUmbraco.Our.Examine;
using OurUmbraco.Our.Models;
using Umbraco.Core;
using Umbraco.Core.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OurUmbraco.Our.Extensions
{
    public static class ReadOnlyCollectionExtenstions
    {
        public static YamlMetaData GetYamlMetaData(this IReadOnlyCollection<string> lines, out int secondYamlMarker)
        {
            // Check if the first line is a YAML marker
            // YAML is only accepted if it's on the top of the document
            // because empty lines are already removed, the index needs to be 0
            var hasYaml = lines.ElementAt(0).TrimEnd() == "---";
            secondYamlMarker = 0;

            if (!hasYaml) 
                return null;
            
            // Find the "next" triple dash starting from the second line
            // But first trim all trailing spaces as this only creates issues which are hard to debug
            // and unclear for users. Make sure you have a ToList because IEnumerable has no IndexOf()
            secondYamlMarker = lines
                .Select(l => l.TrimEnd())
                .ToList()
                .IndexOf("---", 1);

            // add all yaml together and parse YAML meta data
            var yamlMetaData = new YamlMetaData();
            if (secondYamlMarker <= 0) 
                return yamlMetaData;
            
            // we found a second marker, so we have YAML data available
            var yamlInput = new StringBuilder();
            for (var i = 1; i < secondYamlMarker; i++)
            {
                // the line must contain some valid yaml, key-value pairs are
                // separated by a ":" so only add lines that have that
                var line = lines.ElementAt(i);
                if (line.InvariantContains(":"))
                {
                    yamlInput.AppendLine(line);
                }
            };

            // Try to convert the YAML text to a strongly typed model using YamlDotNet
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention())
                .IgnoreUnmatchedProperties()
                .Build();
            try
            {
                yamlMetaData = deserializer.Deserialize<YamlMetaData>(yamlInput.ToString());
            }
            catch (SemanticErrorException ex)
            {
                LogHelper.Error(typeof(ExamineHelper), "Could not parse the YAML meta data {0}" + yamlInput, ex);
                yamlMetaData.Tags = "yamlissue";
            }

            return yamlMetaData;
        }
    }
}