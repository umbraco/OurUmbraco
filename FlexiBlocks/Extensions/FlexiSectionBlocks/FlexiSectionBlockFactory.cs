using Markdig.Parsers;
using Markdig.Syntax;
using System;
using System.Collections.Generic;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// The default implementation of <see cref="IFlexiSectionBlockFactory"/>.
    /// </summary>
    public class FlexiSectionBlockFactory : IFlexiSectionBlockFactory
    {
        internal const string OPEN_PROXY_SECTION_BLOCKS_KEY = "openFlexiSectionBlocksKey";

        private readonly IOptionsService<IFlexiSectionBlockOptions, IFlexiSectionBlocksExtensionOptions> _optionsService;
        private readonly IFlexiSectionHeadingBlockFactory _flexiSectionHeadingBlockFactory;

        /// <summary>
        /// Creates a <see cref="FlexiSectionBlockFactory"/>.
        /// </summary>
        /// <param name="optionsService">The service for creating <see cref="IFlexiSectionBlockOptions"/> and <see cref="IFlexiSectionBlocksExtensionOptions"/>.</param>
        /// <param name="flexiSectionHeadingBlockFactory">The factory for building <see cref="FlexiSectionHeadingBlock"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="optionsService"/>is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flexiSectionHeadingBlockFactory"/> is <c>null</c>.</exception>
        public FlexiSectionBlockFactory(IOptionsService<IFlexiSectionBlockOptions, IFlexiSectionBlocksExtensionOptions> optionsService,
            IFlexiSectionHeadingBlockFactory flexiSectionHeadingBlockFactory)
        {
            _optionsService = optionsService ?? throw new ArgumentNullException(nameof(optionsService));
            _flexiSectionHeadingBlockFactory = flexiSectionHeadingBlockFactory ?? throw new ArgumentNullException(nameof(flexiSectionHeadingBlockFactory));
        }

        /// <inheritdoc />
        public FlexiSectionBlock Create(int level, BlockProcessor blockProcessor, BlockParser blockParser)
        {
            (IFlexiSectionBlockOptions flexiSectionBlockOptions, IFlexiSectionBlocksExtensionOptions _) = _optionsService.CreateOptions(blockProcessor);

            // Level
            ValidateLevel(level);

            // Block name
            string blockName = ResolveBlockName(flexiSectionBlockOptions.BlockName);

            // Element
            SectioningContentElement element = flexiSectionBlockOptions.Element;
            ValidateElement(element);

            // Rendering mode
            FlexiSectionBlockRenderingMode renderingMode = flexiSectionBlockOptions.RenderingMode;
            ValidateRenderingMode(renderingMode);

            // Create FlexiSectionBlock
            var flexiSectionBlock = new FlexiSectionBlock(blockName,
                element,
                flexiSectionBlockOptions.LinkIcon,
                renderingMode,
                level,
                flexiSectionBlockOptions.Attributes,
                blockParser)
            {
                Column = blockProcessor.Column,
                Span = new SourceSpan(blockProcessor.Start, blockProcessor.Line.End), // TODO span should include children
                // BlockProcessor will assign Line
            };

            // Create FlexiSectionHeadingBlock
            FlexiSectionHeadingBlock flexiSectionHeadingBlock = _flexiSectionHeadingBlockFactory.Create(blockProcessor, flexiSectionBlockOptions, blockParser);
            flexiSectionBlock.Add(flexiSectionHeadingBlock);

            // Close FlexiSectionBlocks with same or lower levels
            UpdateOpenFlexiSectionBlocks(blockProcessor, flexiSectionBlock);

            return flexiSectionBlock;
        }

        internal virtual void ValidateLevel(int level)
        {
            if (level < 1 || level > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(level), string.Format(Strings.ArgumentOutOfRangeException_Shared_ValueMustBeWithinRange, "[1, 6]", level));
            }
        }

        internal virtual string ResolveBlockName(string blockName)
        {
            return string.IsNullOrWhiteSpace(blockName) ? "flexi-section" : blockName;
        }

        internal virtual void ValidateElement(SectioningContentElement element)
        {
            if (!Enum.IsDefined(typeof(SectioningContentElement), element))
            {
                throw new OptionsException(nameof(IFlexiSectionBlockOptions.Element),
                        string.Format(Strings.OptionsException_Shared_ValueMustBeAValidEnumValue,
                            element,
                            nameof(SectioningContentElement)));
            }
        }

        internal virtual void ValidateRenderingMode(FlexiSectionBlockRenderingMode renderingMode)
        {
            if (!Enum.IsDefined(typeof(FlexiSectionBlockRenderingMode), renderingMode))
            {
                throw new OptionsException(nameof(IFlexiSectionBlockOptions.RenderingMode),
                    string.Format(Strings.OptionsException_Shared_ValueMustBeAValidEnumValue,
                        renderingMode,
                        nameof(FlexiSectionBlockRenderingMode)));
            }
        }

        // Handles closing of FlexiSectionBlocks. A FlexiSectionBlock is closed when another FlexiSectionBlock in the same tree with the same or 
        // lower level opens.
        internal virtual void UpdateOpenFlexiSectionBlocks(BlockProcessor processor, FlexiSectionBlock flexiSectionBlock)
        {
            // Since sectioning content roots like blockquotes have their own discrete section trees, 
            // we maintain a stack of stacks. Each stack represents the open branch of a tree.
            Stack<Stack<FlexiSectionBlock>> openFlexiSectionBlocks = GetOrCreateOpenFlexiSectionBlocks(processor.Document);

            // Discard stacks for closed branches
            while (openFlexiSectionBlocks.Count > 0)
            {
                // When a sectioning content root is closed, all of its children are closed, so the last section in its branch
                // will be closed. Under no circumstance will the section at the tip of a branch be closed without its ancestors 
                // being closed as well.
                if (openFlexiSectionBlocks.Peek().Peek().IsOpen)
                {
                    break;
                }

                openFlexiSectionBlocks.Pop();
            }

            // Find parent container block - processor.CurrentContainer may be closed. processor.CurrentContainer is only updated when
            // BlockProcessor.ProcessNewBlocks calls BlockProcessor.CloseAll, so at this point, processor.CurrentContainer may not be the eventual
            // parent of our new FlexiSectionBlock.
            ContainerBlock parentContainerBlock = processor.CurrentContainer;
            while (!parentContainerBlock.IsOpen) // We will eventually reach the root MarkdownDocument (gauranteed to be open) if all other containers aren't open
            {
                parentContainerBlock = parentContainerBlock.Parent;
            }

            if (!(parentContainerBlock is FlexiSectionBlock)) // parentContainerBlock is a sectioning content root, for example, a blockquote. Create a new stack for a new tree
            {
                var newStack = new Stack<FlexiSectionBlock>();
                newStack.Push(flexiSectionBlock);
                openFlexiSectionBlocks.Push(newStack);
            }
            else
            {
                Stack<FlexiSectionBlock> currentBranch = openFlexiSectionBlocks.Peek(); // If parentContainerBlock is a FlexiSectionBlock, at least 1 branch of open FlexiSectionBlocks exists
                // Close open FlexiSectionBlocks that have the same or higher levels
                FlexiSectionBlock flexiSectionBlockToClose = null;
                while (currentBranch.Count > 0)
                {
                    if (currentBranch.Peek().Level < flexiSectionBlock.Level)
                    {
                        break;
                    }

                    flexiSectionBlockToClose = currentBranch.Pop();
                }
                if (flexiSectionBlockToClose != null)
                {
                    processor.Close(flexiSectionBlockToClose);
                }

                // Add new FlexiSectionBlock to current stack
                currentBranch.Push(flexiSectionBlock);
            }
        }

        // We use stacks to traverse section trees, in a DFS like manner.
        internal virtual Stack<Stack<FlexiSectionBlock>> GetOrCreateOpenFlexiSectionBlocks(MarkdownDocument markdownDocument)
        {
            if (!(markdownDocument.GetData(OPEN_PROXY_SECTION_BLOCKS_KEY) is Stack<Stack<FlexiSectionBlock>> openFlexiSectionBlocks))
            {
                openFlexiSectionBlocks = new Stack<Stack<FlexiSectionBlock>>();
                markdownDocument.SetData(OPEN_PROXY_SECTION_BLOCKS_KEY, openFlexiSectionBlocks);
            }

            return openFlexiSectionBlocks;
        }
    }
}
