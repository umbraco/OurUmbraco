using Jering.Markdig.Extensions.FlexiBlocks.ContextObjects;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiBannerBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiCardsBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiCodeBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiFigureBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiPictureBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiQuoteBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiTabsBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiVideoBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Markdig;
using Markdig.Extensions.Citations;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para><see cref="MarkdownPipelineBuilder"/> extensions for adding FlexiBlocks extensions.</para>
    /// </summary>
    public static class MarkdownPipelineBuilderExtensions
    {
        private static volatile IServiceCollection _services;
        private static volatile ServiceProvider _serviceProvider;
        private static readonly object _createLock = new object();

        private static ServiceProvider GetOrCreateServiceProvider()
        {
            if (_serviceProvider == null || _services != null)
            {
                lock (_createLock)
                {
                    if (_serviceProvider == null || _services != null)
                    {
                        // Dispose of service provider
                        _serviceProvider?.Dispose();

                        // Create new service provider
                        (_services ?? (_services = new ServiceCollection())).AddFlexiBlocks();
                        _serviceProvider = _services.BuildServiceProvider();
                        _services = null;
                    }
                }
            }

            return _serviceProvider;
        }

        /// <summary>
        /// <para>Disposes the underlying <see cref="IServiceProvider"/> used to resolve FlexiBlocks services.</para>
        /// <para>This method is not thread safe.</para>
        /// </summary>
        public static void DisposeServiceProvider()
        {
            _serviceProvider?.Dispose();
            _serviceProvider = null;
        }

        /// <summary>
        /// <para>Configures options.</para>
        /// <para>This method is not thread safe.</para>
        /// </summary>
        /// <typeparam name="T">The type of options to configure.</typeparam>
        /// <param name="configureOptions">The action that configures the options.</param>
        public static void Configure<T>(Action<T> configureOptions) where T : class
        {
            (_services ?? (_services = new ServiceCollection())).Configure(configureOptions);
        }

        /// <summary>
        /// Adds all FlexiBlocks extensions to the specified <see cref="MarkdownPipelineBuilder"/>.
        /// </summary>
        /// <param name="pipelineBuilder">The <see cref="MarkdownPipelineBuilder"/> to add the extensions to.</param>
        /// <param name="flexiIncludeBlocksExtensionOptions">Options for the <see cref="FlexiIncludeBlocksExtensionOptions"/>.</param>
        /// <param name="flexiAlertBlocksExtensionOptions">Options for the <see cref="FlexiAlertBlocksExtension"/>.</param>
        /// <param name="flexiBannerBlocksExtensionOptions">Options for the <see cref="FlexiBannerBlocksExtension"/>.</param>
        /// <param name="flexiCardsBlocksExtensionOptions">Options for the <see cref="FlexiCardsBlocksExtension"/>.</param>
        /// <param name="flexiCodeBlocksExtensionOptions">Options for the <see cref="FlexiCodeBlocksExtension"/>.</param>
        /// <param name="flexiFigureBlocksExtensionOptions">Options for the <see cref="FlexiFigureBlocksExtension"/>.</param>
        /// <param name="flexiPictureBlocksExtensionOptions">Options for the <see cref="FlexiPictureBlocksExtension"/>.</param>
        /// <param name="flexiQuoteBlocksExtensionOptions">Options for the <see cref="FlexiQuoteBlocksExtension"/>.</param>
        /// <param name="flexiSectionBlocksExtensionOptions">Options for the <see cref="FlexiSectionBlocksExtension"/>.</param>
        /// <param name="flexiTableBlocksExtensionOptions">Options for the <see cref="FlexiTableBlocksExtension"/>.</param>
        /// <param name="flexiTabsBlocksExtensionOptions">Options for the <see cref="FlexiTabsBlocksExtension"/>.</param>
        /// <param name="flexiVideoBlocksExtensionOptions">Options for the <see cref="FlexiVideoBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiBlocks(this MarkdownPipelineBuilder pipelineBuilder,
            IFlexiIncludeBlocksExtensionOptions flexiIncludeBlocksExtensionOptions = null,
            IFlexiAlertBlocksExtensionOptions flexiAlertBlocksExtensionOptions = null,
            IFlexiBannerBlocksExtensionOptions flexiBannerBlocksExtensionOptions = null,
            IFlexiCardsBlocksExtensionOptions flexiCardsBlocksExtensionOptions = null,
            IFlexiCodeBlocksExtensionOptions flexiCodeBlocksExtensionOptions = null,
            IFlexiFigureBlocksExtensionOptions flexiFigureBlocksExtensionOptions = null,
            IFlexiPictureBlocksExtensionOptions flexiPictureBlocksExtensionOptions = null,
            IFlexiQuoteBlocksExtensionOptions flexiQuoteBlocksExtensionOptions = null,
            IFlexiSectionBlocksExtensionOptions flexiSectionBlocksExtensionOptions = null,
            IFlexiTableBlocksExtensionOptions flexiTableBlocksExtensionOptions = null,
            IFlexiTabsBlocksExtensionOptions flexiTabsBlocksExtensionOptions = null,
            IFlexiVideoBlocksExtensionOptions flexiVideoBlocksExtensionOptions = null)
        {
            return pipelineBuilder.
                UseContextObjects().
                UseFlexiIncludeBlocks(flexiIncludeBlocksExtensionOptions).
                UseFlexiOptionsBlocks().
                UseFlexiAlertBlocks(flexiAlertBlocksExtensionOptions).
                UseFlexiBannerBlocks(flexiBannerBlocksExtensionOptions).
                UseFlexiCardsBlocks(flexiCardsBlocksExtensionOptions).
                UseFlexiCodeBlocks(flexiCodeBlocksExtensionOptions).
                UseFlexiFigureBlocks(flexiFigureBlocksExtensionOptions).
                UseFlexiPictureBlocks(flexiPictureBlocksExtensionOptions).
                UseFlexiQuoteBlocks(flexiQuoteBlocksExtensionOptions).
                UseFlexiSectionBlocks(flexiSectionBlocksExtensionOptions).
                UseFlexiTableBlocks(flexiTableBlocksExtensionOptions).
                UseFlexiTabsBlocks(flexiTabsBlocksExtensionOptions).
                UseFlexiVideoBlocks(flexiVideoBlocksExtensionOptions);
        }

        /// <summary>
        /// Adds the <see cref="ContextObjectsExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        public static MarkdownPipelineBuilder UseContextObjects(this MarkdownPipelineBuilder pipelineBuilder)
        {
            if (!pipelineBuilder.Extensions.Contains<ContextObjectsExtension>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<ContextObjectsExtension>());
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiIncludeBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiIncludeBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiIncludeBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiIncludeBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiIncludeBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiIncludeBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiOptionsBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param> 
        public static MarkdownPipelineBuilder UseFlexiOptionsBlocks(this MarkdownPipelineBuilder pipelineBuilder)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiOptionsBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiOptionsBlock>>());
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiAlertBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiAlertBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiAlertBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiAlertBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiAlertBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiAlertBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiBannerBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiBannerBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiBannerBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiBannerBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiBannerBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiBannerBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiCardsBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiCardsBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiCardsBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiCardsBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiCardsBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiCardsBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiCodeBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiCodeBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiCodeBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiCodeBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiCodeBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiCodeBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiFigureBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiFigureBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiFigureBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiFigureBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiFigureBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiFigureBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiPictureBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiPictureBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiPictureBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiPictureBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiPictureBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiPictureBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiQuoteBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiQuoteBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiQuoteBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiQuoteBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiQuoteBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiQuoteBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            pipelineBuilder.Extensions.AddIfNotAlready<CitationExtension>();

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiSectionBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiSectionBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiSectionBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiSectionBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiSectionBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiSectionBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiTableBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiTableBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiTableBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiTableBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiTableBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiTableBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiTabsBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiTabsBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiTabsBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiTabsBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiTabsBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiTabsBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        /// <summary>
        /// Adds the <see cref="FlexiVideoBlocksExtension"/> to the pipeline.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder for the pipeline.</param>
        /// <param name="options">Options for the <see cref="FlexiVideoBlocksExtension"/>.</param>
        public static MarkdownPipelineBuilder UseFlexiVideoBlocks(this MarkdownPipelineBuilder pipelineBuilder, IFlexiVideoBlocksExtensionOptions options = null)
        {
            if (!pipelineBuilder.Extensions.Contains<IBlockExtension<FlexiVideoBlock>>())
            {
                pipelineBuilder.Extensions.Add(GetOrCreateServiceProvider().GetRequiredService<IBlockExtension<FlexiVideoBlock>>());
            }

            if (options != null)
            {
                AddContextObjectWithTypeAsKey(pipelineBuilder, options);
            }

            return pipelineBuilder;
        }

        private static void AddContextObjectWithTypeAsKey<T>(MarkdownPipelineBuilder pipelineBuilder, T contextObject)
        {
            ContextObjectsExtension contextObjectsExtension = pipelineBuilder.Extensions.Find<ContextObjectsExtension>();

            if (contextObjectsExtension == null)
            {
                contextObjectsExtension = GetOrCreateServiceProvider().GetRequiredService<ContextObjectsExtension>();
                pipelineBuilder.Extensions.Add(contextObjectsExtension);
            }

            contextObjectsExtension.ContextObjectsStore.Add(typeof(T), contextObject);
        }
    }
}
