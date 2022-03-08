using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks
{
    /// <summary>
    /// <para>The default implementation of <see cref="IFlexiAlertBlocksExtensionOptions"/>.</para>
    /// 
    /// <para>This class is immutable.</para>
    /// </summary>
    public class FlexiAlertBlocksExtensionOptions : ExtensionOptions<IFlexiAlertBlockOptions>, IFlexiAlertBlocksExtensionOptions
    {
        private static readonly ReadOnlyDictionary<string, string> _defaultIcons = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "info", MaterialDesignIcons.MATERIAL_DESIGN_INFO },
                { "warning", MaterialDesignIcons.MATERIAL_DESIGN_WARNING},
                { "critical-warning", MaterialDesignIcons.MATERIAL_DESIGN_ERROR}
            }
        );

        /// <summary>
        /// Creates a <see cref="FlexiAlertBlocksExtensionOptions"/>.
        /// </summary>
        /// <param name="defaultBlockOptions">
        /// <para>Default <see cref="IFlexiAlertBlockOptions"/> for all <see cref="FlexiAlertBlock"/>s.</para>
        /// <para>If this value is <c>null</c>, a <see cref="FlexiAlertBlockOptions"/> with default values is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        /// <param name="icons">
        /// <para>A map of <see cref="FlexiAlertBlock"/> types to icon HTML fragments.</para>
        /// <para>If this value is <c>null</c>, a map of icon HTML fragments containing types "info", "warning" and "critical-warning" is used.</para>
        /// <para>Defaults to <c>null</c>.</para>
        /// </param>
        public FlexiAlertBlocksExtensionOptions(IFlexiAlertBlockOptions defaultBlockOptions = null, IDictionary<string, string> icons = null) :
            base(defaultBlockOptions ?? new FlexiAlertBlockOptions())
        {
            Icons = icons is ReadOnlyDictionary<string, string> ? icons as ReadOnlyDictionary<string, string> :
                icons != null ? new ReadOnlyDictionary<string, string>(icons) :
                _defaultIcons;
        }

        /// <summary>
        /// Creates a <see cref="FlexiAlertBlocksExtensionOptions"/>.
        /// </summary>
        public FlexiAlertBlocksExtensionOptions() : this(null, null)
        {
        }

        /// <inheritdoc />
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public virtual ReadOnlyDictionary<string, string> Icons { get; private set; }
    }
}
