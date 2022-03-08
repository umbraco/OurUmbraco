using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Newtonsoft.Json;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// An abstraction for parsing JSON <see cref="Block"/>s.
    /// </summary>
    /// <typeparam name="TMain">The type of JSON <see cref="Block"/> this parser parsers.</typeparam>
    /// <typeparam name="TProxy">The type of <see cref="IProxyJsonBlock"/> to collect data for the JSON <see cref="Block"/>.</typeparam>
    public abstract class JsonBlockParser<TMain, TProxy> : ProxyBlockParser<TMain, TProxy>
        where TMain : Block
        where TProxy : LeafBlock, IProxyJsonBlock
    {
        private readonly IJsonBlockFactory<TMain, TProxy> _jsonBlockFactory;

        /// <summary>
        /// Creates a <see cref="JsonBlockParser{TMain, TProxy}"/>.
        /// </summary>
        /// <param name="JsonBlockFactory">The factory for creating JSON <see cref="Block"/>s.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="JsonBlockFactory"/> is <c>null</c>.</exception>
        protected JsonBlockParser(IJsonBlockFactory<TMain, TProxy> JsonBlockFactory)
        {
            _jsonBlockFactory = JsonBlockFactory ?? throw new ArgumentNullException(nameof(JsonBlockFactory));
        }

        /// <summary>
        /// Opens a <typeparamref name="TProxy"/> if a line begins with at least 3 fence characters.
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the document that contains a line with a fence character as its first character.</param>
        /// <returns>
        /// <see cref="BlockState.None"/> if the current line has code indent.
        /// <see cref="BlockState.None"/> if the current line does not contain an opening fence.
        /// <see cref="BlockState.ContinueDiscard"/> if the current line contains an opening fence and a <typeparamref name="TProxy"/> is opened.
        ///</returns>
        protected override BlockState TryOpenBlock(BlockProcessor blockProcessor)
        {
            if (blockProcessor.IsCodeIndent)
            {
                return BlockState.None;
            }

            // First line of a JSONBlock must begin with <opening char>{
            if (blockProcessor.PeekChar(1) != '{')
            {
                return BlockState.None;
            }

            // Create block
            TProxy proxyJsonBlock = _jsonBlockFactory.CreateProxyJsonBlock(blockProcessor, this);
            blockProcessor.NewBlocks.Push(proxyJsonBlock);

            // Dispose of first char (JSON starts at the curly bracket)
            blockProcessor.NextChar();

            return ParseLine(blockProcessor.Line, proxyJsonBlock);
        }

        /// <summary>
        /// Continues a <typeparamref name="TProxy"/> if the current line is not a closing fence.  
        /// </summary>
        /// <param name="blockProcessor">The <see cref="BlockProcessor"/> for the <typeparamref name="TProxy"/> to try continuing.</param>
        /// <param name="block">The <typeparamref name="TProxy"/> to try continuing.</param>
        /// <returns>
        /// <see cref="BlockState.BreakDiscard"/> if the current line contains a closing fence and the <typeparamref name="TProxy"/> is closed.
        /// <see cref="BlockState.Continue"/> if the current line has code indent or the current line does not contain a closing fence.
        /// </returns>
        protected override BlockState TryContinueBlock(BlockProcessor blockProcessor, TProxy block)
        {
            return ParseLine(blockProcessor.Line, block);
        }

        /// <inheritdoc />
        protected override TMain CloseProxy(BlockProcessor blockProcessor, TProxy proxyBlock)
        {
            if (proxyBlock.NumOpenObjects > 0) // JSON in proxy block isn't complete
            {
                // TODO integration test json missing closing curly brackets (will parse till end of doc then should throw here)
                throw new JsonException(string.Format(Strings.JsonException_Shared_InvalidJson, proxyBlock.Lines.ToString()));
            }

            return _jsonBlockFactory.Create(proxyBlock, blockProcessor);
        }

        /// <summary>
        /// <para>Parses the next line in the JSON block.</para>
        /// <para>Markdig parses markdown line by line, unfortunately, JSON.NET does not expose any efficient method for parsing JSON over several method calls. The commonly used 
        /// JsonSerializer.Deserialize is a static method that doesn't store any state. Deserialization using it must occur within a single call.</para>
        /// 
        /// <para>To test if JSON is complete (all objects closed), on each <see cref="BlockParser.TryContinue(BlockProcessor, Block)"/> call, we could append the line to a 
        /// string stored on the block and attempt to deserialize the string. Such an approach is inefficient, potentially allocating tons of strings. 
        /// On top of that we'd have to use exceptions as control flow.</para>
        /// 
        /// <para>Alternatively, we could attempt to deserialize the current line's <see cref="StringSlice.Text"/> value, starting from the point where the JSON begins and
        /// ignoring text after the JSON (referred to as additional text by JSON.NET). The issue with this approach is that we may not know what type of object
        /// to deserialize to. We'd have to create an inefficient intermediate JObject.</para>
        /// 
        /// <para>This method is a fairly efficient approach. On each <see cref="BlockParser.TryContinue(BlockProcessor, Block)"/> call, it iterates through each character to 
        /// determine whether the JSON is complete. It does this according to the <a href="https://www.json.org/">JSON specification</a>. Basically, braces have no semantic 
        /// meaning if they occur within strings, otherwise, { opens an object and } closes an object. This method simply tallies braces till opening
        /// and closing braces are balanced.</para>
        /// 
        /// <para>It does not catch syntactic errors. Also, if braces aren't balanced, it could well end up iterating through all characters
        /// in a document. Nonetheless, it is more efficient than other methods.</para>
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <param name="proxyJsonBlock"></param>
        /// <returns>The state of this block after parsing <paramref name="line"/>.</returns>
        internal virtual BlockState ParseLine(StringSlice line, TProxy proxyJsonBlock)
        {
            char previousChar = line.PeekCharExtra(-1);
            char currentChar = line.CurrentChar;

            while (currentChar != '\0')
            {
                if (!proxyJsonBlock.WithinString)
                {
                    if (currentChar == '{')
                    {
                        proxyJsonBlock.NumOpenObjects++;
                    }
                    else if (currentChar == '}')
                    {
                        // Braces balanced
                        if (--proxyJsonBlock.NumOpenObjects == 0)
                        {
                            proxyJsonBlock.UpdateSpanEnd(line.End);

                            return BlockState.Break;
                        }
                    }
                    else if (previousChar != '\\' && currentChar == '"')
                    {
                        proxyJsonBlock.WithinString = true;
                    }
                }
                else if (previousChar != '\\' && currentChar == '"')
                {
                    proxyJsonBlock.WithinString = false;
                }

                previousChar = currentChar;
                currentChar = line.NextChar(); // \0 if out of range
            }

            return BlockState.Continue;
        }
    }
}
