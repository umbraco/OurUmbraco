using Markdig.Renderers;
using Markdig.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// <para>Extensions for <see cref="HtmlRenderer"/>.</para>
    /// <para>Overloads are used except when different methods would have the same signatures, e.g a WriteStartTag method that take an attributes string and 
    /// one that takes a classes string.</para>
    /// </summary>
    public static class HtmlRendererExtensions
    {
        /// <summary>
        /// https://www.w3.org/TR/html5/infrastructure.html#space-characters
        /// </summary>
        private static readonly ImmutableHashSet<char> _spaceChars = ImmutableHashSet.Create(new char[] { ' ', '\t', '\r', '\n', '\f' });

        private static readonly char[] _digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        #region Numbers
        /// <summary>
        /// Writes an integer.
        /// </summary>
        public static HtmlRenderer WriteInt(this HtmlRenderer htmlRenderer, int integer)
        {
            if (integer < 0)
            {
                htmlRenderer.Write('-');
                integer = -integer;
            }

            // https://stackoverflow.com/questions/4483886/how-can-i-get-a-count-of-the-total-number-of-digits-in-a-number
            int firstDivisor;
            if (integer < 10)
            {
                firstDivisor = 1;
            }
            else if (integer < 100)
            {
                firstDivisor = 10;
            }
            else if (integer < 1000)
            {
                firstDivisor = 100;
            }
            else if (integer < 10000)
            {
                firstDivisor = 1000;
            }
            else if (integer < 100000)
            {
                firstDivisor = 10000;
            }
            else if (integer < 1000000)
            {
                firstDivisor = 100000;
            }
            else if (integer < 10000000)
            {
                firstDivisor = 1000000;
            }
            else if (integer < 100000000)
            {
                firstDivisor = 10000000;
            }
            else if (integer < 1000000000)
            {
                firstDivisor = 100000000;
            }
            else
            {
                firstDivisor = 1000000000;
            }

            for (int i = firstDivisor; i > 0; i /= 10)
            {
                htmlRenderer.Write(_digits[integer / i]);
                integer %= i;
            }

            return htmlRenderer;
        }

        // TODO WriteDouble
        #endregion

        #region Elements
        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>__<paramref name="elementName"/>}\"&gt;{<paramref name="content"/>}&lt;/{<paramref name="tagName"/>}&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteElementLine(this HtmlRenderer htmlRenderer, string tagName, string blockName, string elementName, string content)
        {
            return htmlRenderer.
                WriteStartTag(tagName, blockName, elementName).
                Write(content).
                WriteEndTagLine(tagName);
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>__<paramref name="elementName"/>}\"&gt;{<paramref name="content"/>}&lt;/{<paramref name="tagName"/>}&gt;\n" if
        /// <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteElementLine(this HtmlRenderer htmlRenderer, bool condition, string tagName, string blockName, string elementName, string content)
        {
            return condition ? htmlRenderer.WriteElementLine(tagName, blockName, elementName, content) : htmlRenderer;
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>__<paramref name="elementName"/>}\"&gt;{<paramref name="content1"/>}{<paramref name="content2"/>}&lt;/{<paramref name="tagName"/>}&gt;" if
        /// <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteElement(this HtmlRenderer htmlRenderer, bool condition, string tagName, string blockName, string elementName, string content1, string content2)
        {
            return condition ? htmlRenderer.
                WriteStartTag(tagName, blockName, elementName).
                Write(content1).
                Write(content2).
                WriteEndTag(tagName) : htmlRenderer;
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>__<paramref name="elementName"/>}\"&gt;{<paramref name="leafBlock"/>}&lt;/{<paramref name="tagName"/>}&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteElementLine(this HtmlRenderer htmlRenderer, string tagName, string blockName, string elementName, LeafBlock leafBlock)
        {
            return htmlRenderer.
                WriteStartTag(tagName, blockName, elementName).
                WriteLeafInline(leafBlock).
                WriteEndTagLine(tagName);
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>__<paramref name="elementName"/>}\"&gt;\n{<paramref name="containerBlock"/>}&lt;/{<paramref name="tagName"/>}&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteElementLine(this HtmlRenderer htmlRenderer, string tagName, string blockName, string elementName, ContainerBlock containerBlock, bool implicitParagraphs)
        {
            return htmlRenderer.
                WriteStartTagLine(tagName, blockName, elementName).
                WriteChildren(containerBlock, implicitParagraphs).
                EnsureLine().
                WriteEndTagLine(tagName);
        }
        #endregion

        #region Attributes

        /// <summary>
        /// Writes " {<paramref name="attributeName"/>}=\"{<paramref name="value"/>}\"".
        /// </summary>
        public static HtmlRenderer WriteAttribute(this HtmlRenderer htmlRenderer, string attributeName, string value)
        {
            return htmlRenderer.
                Write(' '). // Never the first attribute (all elements have class attribute)
                Write(attributeName).
                Write("=\"").
                Write(value).
                Write('"');
        }

        /// <summary>
        /// Writes " {<paramref name="attributeName"/>}=\"{<paramref name="value"/>}\"" if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteAttribute(this HtmlRenderer htmlRenderer, bool condition, string attributeName, string value)
        {
            return condition ? htmlRenderer.WriteAttribute(attributeName, value) : htmlRenderer;
        }

        /// <summary>
        /// Writes " {<paramref name="attributeName"/>}=\"{<paramref name="value"/>}\"" if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteAttribute(this HtmlRenderer htmlRenderer, bool condition, string attributeName, int value)
        {
            return condition ? htmlRenderer.
                Write(' '). // Never the first attribute (all elements have class attribute)
                Write(attributeName).
                Write("=\"").
                WriteInt(value).
                Write('"') : htmlRenderer;
        }

        /// <summary>
        /// Writes " {<paramref name="attributeName"/>}=\"{<paramref name="value"/>}\"" if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteAttribute(this HtmlRenderer htmlRenderer, bool condition, string attributeName, double value)
        {
            return htmlRenderer.WriteAttribute(condition, attributeName, value.ToString());
        }

        /// <summary>
        /// Writes " style=\"{<paramref name="styleName"/>}:{<paramref name="valuePart1"/>}{<paramref name="valuePart2"/>}\"" if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteStyleAttribute(this HtmlRenderer htmlRenderer, bool condition, string styleName, double valuePart1, string valuePart2)
        {
            return condition ? htmlRenderer.
                Write(" style=\""). // Never the first attribute (all elements have class attribute)
                Write(styleName).
                Write(':').
                Write(valuePart1.ToString()). // TODO allocates
                Write(valuePart2).
                Write('"') : htmlRenderer;
        }

        /// <summary>
        /// Writes " {<paramref name="attributeName"/>}=\"{escaped <paramref name="value"/>}\"". 
        /// </summary>
        public static HtmlRenderer WriteEscapedUrlAttribute(this HtmlRenderer htmlRenderer, string attributeName, string value)
        {
            return htmlRenderer.
                Write(' '). // Never the first attribute (all elements have class attribute)
                Write(attributeName).
                Write("=\"").
                WriteEscapeUrl(value).
                Write('"');
        }

        /// <summary>
        /// Writes " {<paramref name="attributeName"/>}=\"{escaped <paramref name="value"/>}\"" if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteEscapedUrlAttribute(this HtmlRenderer htmlRenderer, bool condition, string attributeName, string value)
        {
            return condition ? htmlRenderer.WriteEscapedUrlAttribute(attributeName, value) : htmlRenderer;
        }

        /// <summary>
        /// If a value with key <paramref name="attributeKey"/> exists in <paramref name="attributes"/>, writes " {value}".
        /// </summary>
        public static HtmlRenderer WriteAttributeValue(this HtmlRenderer htmlRenderer, ReadOnlyDictionary<string, string> attributes, string attributeKey)
        {
            string value = null;

            return attributes?.TryGetValue(attributeKey, out value) == true ? htmlRenderer.Write(' ').Write(value) : htmlRenderer;
        }

        /// <summary>
        /// Writes each <see cref="KeyValuePair{TKey, TValue}"/> in <paramref name="attributes"/> as " {key}=\"{value}\"".
        /// </summary>
        public static HtmlRenderer WriteAttributes(this HtmlRenderer htmlRenderer, ReadOnlyDictionary<string, string> attributes)
        {
            if (attributes == null)
            {
                return htmlRenderer;
            }

            foreach (KeyValuePair<string, string> attribute in attributes)
            {
                htmlRenderer.
                    Write(' ').
                    Write(attribute.Key).
                    Write("=\"").
                    WriteEscape(attribute.Value).
                    Write('"');
            }

            return htmlRenderer;
        }

        /// <summary>
        /// Writes each <see cref="KeyValuePair{TKey, TValue}"/> in <paramref name="attributes"/>, excluding any with key <paramref name="excluded"/>, as " {key}=\"{value}\"".
        /// </summary>
        public static HtmlRenderer WriteAttributesExcept(this HtmlRenderer htmlRenderer, ReadOnlyDictionary<string, string> attributes, string excluded)
        {
            return htmlRenderer.WriteAttributesExcept(attributes, excluded, null); // Keys can't be null (Dictionary throws if you try to add a key-value pair with null key)
        }

        /// <summary>
        /// Writes each <see cref="KeyValuePair{TKey, TValue}"/> in <paramref name="attributes"/>, excluding any with key <paramref name="excluded1"/> or <paramref name="excluded2"/>, 
        /// as " {key}=\"{value}\"".
        /// </summary>
        public static HtmlRenderer WriteAttributesExcept(this HtmlRenderer htmlRenderer, ReadOnlyDictionary<string, string> attributes, string excluded1, string excluded2)
        {
            if (attributes == null)
            {
                return htmlRenderer;
            }

            foreach (KeyValuePair<string, string> attribute in attributes)
            {
                string key = attribute.Key;

                if (key == excluded1 || key == excluded2)
                {
                    continue;
                }

                htmlRenderer.
                    Write(' ').
                    Write(key).
                    Write("=\"").
                    WriteEscape(attribute.Value).
                    Write('"');
            }

            return htmlRenderer;
        }

        #endregion

        #region Classes
        /// <summary>
        /// If <paramref name="hasFeature"/> is <c>true</c>, writes " {<paramref name="blockName"/>}_has-{<paramref name="featureName"/>}". 
        /// Otherwise, writes " {<paramref name="blockName"/>}_no-{<paramref name="featureName"/>}". 
        /// </summary>
        public static HtmlRenderer WriteHasFeatureClass(this HtmlRenderer htmlRenderer, bool hasFeature, string blockName, string featureName)
        {
            return htmlRenderer.WriteBlockBooleanModifierClass(blockName, hasFeature ? "has" : "no", featureName);
        }

        /// <summary>
        /// If <paramref name="hasFeature"/> is <c>true</c>, writes " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_has-{<paramref name="featureName"/>}". 
        /// Otherwise, writes " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_no-{<paramref name="featureName"/>}". 
        /// </summary>
        public static HtmlRenderer WriteHasFeatureClass(this HtmlRenderer htmlRenderer, bool hasFeature, string blockName, string elementName, string featureName)
        {
            return htmlRenderer.WriteElementBooleanModifierClass(blockName, elementName, hasFeature ? "has" : "no", featureName);
        }

        /// <summary>
        /// If <paramref name="isType"/> is <c>true</c>, writes " {<paramref name="blockName"/>}_is-{<paramref name="typeName"/>}". 
        /// Otherwise, writes " {<paramref name="blockName"/>}__not-{<paramref name="typeName"/>}". 
        /// </summary>
        public static HtmlRenderer WriteIsTypeClass(this HtmlRenderer htmlRenderer, bool isType, string blockName, string typeName)
        {
            return htmlRenderer.WriteBlockBooleanModifierClass(blockName, isType ? "is" : "not", typeName);
        }

        /// <summary>
        /// If <paramref name="isType"/> is <c>true</c>, writes " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_is-{<paramref name="typeName"/>}". 
        /// Otherwise, writes " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_not-{<paramref name="typeName"/>}". 
        /// </summary>
        public static HtmlRenderer WriteIsTypeClass(this HtmlRenderer htmlRenderer, bool isType, string blockName, string elementName, string typeName)
        {
            return htmlRenderer.WriteElementBooleanModifierClass(blockName, elementName, isType ? "is" : "not", typeName);
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/naming-convention/#element-name">BEM element class</a>, " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}". 
        /// </summary>
        public static HtmlRenderer WriteElementClass(this HtmlRenderer htmlRenderer, string blockName, string elementName)
        {
            return htmlRenderer.
                Write(blockName).
                Write("__").
                Write(elementName);
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/quick-start/#boolean">BEM element boolean modifier class</a>, " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_{<paramref name="modifier"/>}". 
        /// </summary>
        public static HtmlRenderer WriteElementBooleanModifierClass(this HtmlRenderer htmlRenderer, string blockName, string elementName, string modifier)
        {
            return htmlRenderer.
                Write(' '). // Never the first class
                WriteElementClass(blockName, elementName).
                Write('_').
                Write(modifier);
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/quick-start/#boolean">BEM element boolean modifier class</a>, " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_{<paramref name="modifierPart1"/>}-{<paramref name="modifierPart2"/>}". 
        /// </summary>
        public static HtmlRenderer WriteElementBooleanModifierClass(this HtmlRenderer htmlRenderer, string blockName, string elementName, string modifierPart1, string modifierPart2)
        {
            return htmlRenderer.
                Write(' '). // Never the first class
                WriteElementClass(blockName, elementName).
                Write('_').
                Write(modifierPart1).
                Write('-').
                Write(modifierPart2);
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/quick-start/#boolean">BEM element boolean modifier class</a>, " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_{<paramref name="modifier"/>}" if 
        /// <paramref name="condition"/> is <c>true</c>. 
        /// </summary>
        public static HtmlRenderer WriteElementBooleanModifierClass(this HtmlRenderer htmlRenderer, bool condition, string blockName, string elementName, string modifier)
        {
            return condition ? htmlRenderer.WriteElementBooleanModifierClass(blockName, elementName, modifier) : htmlRenderer;
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/quick-start/#key-value">BEM element key-value modifier class</a>, " {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_{<paramref name="modifierKey"/>}_{<paramref name="modifierValue"/>}".
        /// </summary>
        public static HtmlRenderer WriteElementKeyValueModifierClass(this HtmlRenderer htmlRenderer,
            string blockName,
            string elementName,
            string modifierKey,
            string modifierValue)
        {
            return htmlRenderer.
                WriteElementBooleanModifierClass(blockName, elementName, modifierKey).
                Write('_').
                Write(modifierValue);
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/quick-start/#key-value">BEM block boolean modifier class</a>, " {<paramref name="blockName"/>}_{<paramref name="modifierPart1"/>}-{<paramref name="modifierPart2"/>}".
        /// </summary>
        public static HtmlRenderer WriteBlockBooleanModifierClass(this HtmlRenderer htmlRenderer, string blockName, string modifierPart1, string modifierPart2)
        {
            return htmlRenderer.
                Write(' '). // Never the first class
                Write(blockName).
                Write('_').
                Write(modifierPart1).
                Write('-').
                Write(modifierPart2);
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/quick-start/#key-value">BEM block key-value modifier class</a>, " {<paramref name="blockName"/>}_{<paramref name="modifierKey"/>}_{<paramref name="modifierValue"/>}" if
        /// <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteBlockKeyValueModifierClass(this HtmlRenderer htmlRenderer, bool condition, string blockName, string modifierKey, string modifierValue)
        {
            return condition ? htmlRenderer.WriteBlockKeyValueModifierClass(blockName, modifierKey, modifierValue) : htmlRenderer;
        }

        /// <summary>
        /// Writes a <a href="https://en.bem.info/methodology/quick-start/#key-value">BEM block key-value modifier class</a>, " {<paramref name="blockName"/>}_{<paramref name="modifierKey"/>}_{<paramref name="modifierValue"/>}".
        /// </summary>
        public static HtmlRenderer WriteBlockKeyValueModifierClass(this HtmlRenderer htmlRenderer, string blockName, string modifierKey, char modifierValue)
        {
            return htmlRenderer.
                Write(' '). // Never the first class
                Write(blockName).
                Write('_').
                Write(modifierKey).
                Write('_').
                Write(modifierValue);
        }

        /// <summary>
        /// Writes <a href="https://en.bem.info/methodology/quick-start/#key-value">BEM key-value modifier class, " {<paramref name="blockName"/>}_{<paramref name="modifierKey"/>}_{<paramref name="modifierValue"/>}".</a>
        /// </summary>
        public static HtmlRenderer WriteBlockKeyValueModifierClass(this HtmlRenderer htmlRenderer, string blockName, string modifierKey, string modifierValue)
        {
            return htmlRenderer.
                Write(' '). // Never the first class
                Write(blockName).
                Write('_').
                Write(modifierKey).
                Write('_').
                Write(modifierValue);
        }
        #endregion

        #region Tags
        /// <summary>
        /// Writes "&lt;/{<paramref name="tagName"/>}&gt;".
        /// </summary>
        public static HtmlRenderer WriteEndTag(this HtmlRenderer htmlRenderer, string tagName)
        {
            return htmlRenderer.
                Write("</").
                Write(tagName).
                Write(">");
        }

        /// <summary>
        /// Writes "&lt;/{<paramref name="tagName"/>}&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteEndTagLine(this HtmlRenderer htmlRenderer, string tagName)
        {
            return htmlRenderer.
                WriteEndTag(tagName).
                WriteLine();
        }

        /// <summary>
        /// Writes "&lt;/{<paramref name="tagName"/>}&gt;\n" if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteEndTagLine(this HtmlRenderer htmlRenderer, bool condition, string tagName)
        {
            return condition ? htmlRenderer.WriteEndTagLine(tagName) : htmlRenderer;
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>}\"&gt;".
        /// </summary>
        public static HtmlRenderer WriteStartTag(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName)
        {
            return htmlRenderer.
                Write('<').
                Write(tagName).
                Write(" class=\"").
                WriteElementClass(blockName, elementName).
                Write("\">");
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>}\"&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteStartTagLine(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName)
        {
            return htmlRenderer.
                WriteStartTag(tagName, blockName, elementName).
                WriteLine();
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>} {<paramref name="classes"/>}\"&gt;".
        /// </summary>
        public static HtmlRenderer WriteStartTagWithClasses(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string classes)
        {
            return htmlRenderer.
                Write('<').
                Write(tagName).
                Write(" class=\"").
                WriteElementClass(blockName, elementName).
                Write(' ').
                Write(classes).
                Write("\">");
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>} {<paramref name="classes"/>}\"&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteStartTagLineWithClasses(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string classes)
        {
            return htmlRenderer.
                WriteStartTagWithClasses(tagName, blockName, elementName, classes).
                WriteLine();
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>}\" {<paramref name="attributes"/>}&gt;".
        /// </summary>
        public static HtmlRenderer WriteStartTagWithAttributes(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string attributes)
        {
            return htmlRenderer.
                Write('<').
                Write(tagName).
                Write(" class=\"").
                WriteElementClass(blockName, elementName).
                Write("\" ").
                Write(attributes).
                Write(">");
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>}\" {<paramref name="attributes"/>}&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteStartTagLineWithAttributes(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string attributes)
        {
            return htmlRenderer.
                WriteStartTagWithAttributes(tagName, blockName, elementName, attributes).
                WriteLine();
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>} {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_{<paramref name="modifier"/>}\" {<paramref name="attributes"/>}&gt;".
        /// </summary>
        public static HtmlRenderer WriteStartTagWithModifierClassAndAttributes(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string modifier,
            string attributes)
        {
            return htmlRenderer.
                Write('<').
                Write(tagName).
                Write(" class=\"").
                WriteElementClass(blockName, elementName).
                WriteElementBooleanModifierClass(blockName, elementName, modifier).
                Write("\" ").
                Write(attributes).
                Write(">");
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>} {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_{<paramref name="modifier"/>}\" {<paramref name="attributes"/>}&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteStartTagLineWithModifierClassAndAttributes(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string modifier,
            string attributes)
        {
            return htmlRenderer.
                WriteStartTagWithModifierClassAndAttributes(tagName, blockName, elementName, modifier, attributes).
                WriteLine();
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>}__{<paramref name="elementName"/>} {<paramref name="blockName"/>}__{<paramref name="elementName"/>}_{<paramref name="modifier"/>}\"&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteStartTagLineWithModifierClass(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string modifier)
        {
            return htmlRenderer.
                Write('<').
                Write(tagName).
                Write(" class=\"").
                WriteElementClass(blockName, elementName).
                WriteElementBooleanModifierClass(blockName, elementName, modifier).
                WriteLine("\">");
        }

        /// <summary>
        /// Writes "&lt;{<paramref name="tagName"/>} class=\"{<paramref name="blockName"/>__<paramref name="elementName"/>} {<paramref name="classes"/>}\" {<paramref name="attributes"/>}&gt;\n".
        /// </summary>
        public static HtmlRenderer WriteStartTagLineWithClassesAndAttributes(this HtmlRenderer htmlRenderer,
            string tagName,
            string blockName,
            string elementName,
            string classes,
            string attributes)
        {
            return htmlRenderer.
                Write('<').
                Write(tagName).
                Write(" class=\"").
                WriteElementClass(blockName, elementName).
                Write(' ').
                Write(classes).
                Write("\" ").
                Write(attributes).
                WriteLine(">");
        }
        #endregion

        #region Raw
        /// <summary>
        /// Writes <paramref name="part"/> if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer Write(this HtmlRenderer htmlRenderer,
            bool condition,
            string part)
        {
            return condition ? htmlRenderer.Write(part) : htmlRenderer;
        }

        /// <summary>
        /// Writes <paramref name="part1"/> and <paramref name="part2"/> sequentially if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer Write(this HtmlRenderer htmlRenderer,
            bool condition,
            char part1,
            string part2)
        {
            return condition ? htmlRenderer.Write(part1).Write(part2) : htmlRenderer;
        }

        /// <summary>
        /// Writes <paramref name="part1"/> and <paramref name="part2"/> sequentially if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer Write(this HtmlRenderer htmlRenderer,
            bool condition,
            string part1,
            string part2)
        {
            return condition ? htmlRenderer.Write(part1).Write(part2) : htmlRenderer;
        }

        /// <summary>
        /// Writes <paramref name="part1"/>, <paramref name="part2"/> and <paramref name="part3"/> sequentially if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer Write(this HtmlRenderer htmlRenderer,
            bool condition,
            string part1,
            string part2,
            string part3)
        {
            if (!condition)
            {
                return htmlRenderer;
            }

            htmlRenderer.
                Write(part1).
                Write(part2).
                Write(part3);

            return htmlRenderer;
        }

        /// <summary>
        /// Writes <paramref name="part1"/>, <paramref name="part2"/>, <paramref name="part3"/> and <paramref name="part4"/> sequentially if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer Write(this HtmlRenderer htmlRenderer,
            bool condition,
            char part1,
            string part2,
            string part3,
            string part4)
        {
            if (!condition)
            {
                return htmlRenderer;
            }

            htmlRenderer.
                Write(part1).
                Write(part2).
                Write(part3).
                Write(part4);

            return htmlRenderer;
        }

        /// <summary>
        /// Writes "{<paramref name="part"/>}\n" if <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        public static HtmlRenderer WriteLine(this HtmlRenderer htmlRenderer,
            bool condition,
            string part)
        {
            return condition ? htmlRenderer.WriteLine(part) : htmlRenderer;
        }
        #endregion

        #region HTML Fragments
        /// <summary>
        /// If <paramref name="condition"/> is <c>true</c>, writes <paramref name="htmlFragment"/>, adding a class to its first tag.
        /// </summary>
        public static HtmlRenderer WriteHtmlFragment(this HtmlRenderer htmlRenderer,
            bool condition,
            string htmlFragment,
            string blockName,
            string elementName)
        {
            if (!condition)
            {
                return htmlRenderer;
            }

            // TODO class attribute start index instead
            (int tagStart, int _, int nameEnd, int classValueStart) = FindFirstTag(htmlFragment);

            // Invalid fragment or fragment has no tags or no class value
            if (tagStart == -1)
            {
                return htmlRenderer.Write(htmlFragment);
            }

            if (classValueStart > -1)
            {
                return htmlRenderer.
                    Write(htmlFragment, 0, classValueStart).
                    WriteElementClass(blockName, elementName).
                    Write(' ').
                    Write(htmlFragment, classValueStart, htmlFragment.Length - classValueStart);
            }
            else
            {
                int indexAfterTagName = nameEnd + 1;

                return htmlRenderer.
                    Write(htmlFragment, 0, indexAfterTagName).
                    Write(" class=\"").
                    WriteElementClass(blockName, elementName).
                    Write("\"").
                    Write(htmlFragment, indexAfterTagName, htmlFragment.Length - indexAfterTagName);
            }
        }

        /// <summary>
        /// If <paramref name="condition"/> is <c>true</c>, writes "{<paramref name="htmlFragment"/>}\n", adding a class to its first tag.
        /// </summary>
        public static HtmlRenderer WriteHtmlFragmentLine(this HtmlRenderer htmlRenderer,
            bool condition,
            string htmlFragment,
            string blockName,
            string elementName)
        {
            return condition ? htmlRenderer.WriteHtmlFragment(condition, htmlFragment, blockName, elementName).EnsureLine() : htmlRenderer;
        }

        /// <summary>
        /// <para>Finds the first tag in an HTML fragment.</para>
        /// </summary>
        /// <param name="htmlFragment">
        /// <para>The HTML fragment to search.</para>
        /// <para>If this value is not a valid HTML fragment (https://www.w3.org/TR/html5/syntax), the values returned by this method may not be valid.</para>
        /// </param>
        /// <returns>
        /// <para>If <paramref name="htmlFragment"/> contains at least 1 tag, returns (int startIndex, int endIndex, int nameEnd).</para>
        /// <para>Otherwise, returns (-1, -1, -1).</para>
        /// </returns>
        internal static (int tagStart, int tagEnd, int nameEnd, int classValueStart) FindFirstTag(string htmlFragment)
        {
            int fragmentLength = htmlFragment.Length;
            int tagStart = -1;
            int nameEnd = -1;
            int classValueStart = -1;
            for (int i = 0; i < fragmentLength; i++)
            {
                char c = htmlFragment[i];

                if (c == '<')
                {
                    tagStart = i;
                }
                else if (tagStart > -1)
                {
                    if (nameEnd == -1 &&
                        (_spaceChars.Contains(c) ||
                            c == '/')) // Self-closing tag
                    {
                        nameEnd = i - 1;
                    }
                    else if (c == '"') // Skip attribute values
                    {
                        while (++i < fragmentLength && htmlFragment[i] != '"') ;// Note that in attributes, double quotes are escapsed as &quot;, so we don't need to check for '\' before '"'s
                    }
                    else if (c == '>')
                    {
                        return (tagStart, i, nameEnd == -1 ? i - 1 : nameEnd, classValueStart); // Wrong if tag is empty "<>" but that would be invalid HTML
                    }
                    else if (classValueStart == -1 &&
                        c == 'c' &&
                        fragmentLength - i >= 8 && // At least class="" (class is not a boolean attribute so just "class" alone is invalid)
                        htmlFragment[++i] == 'l' &&
                        htmlFragment[++i] == 'a' &&
                        htmlFragment[++i] == 's' &&
                        htmlFragment[++i] == 's' &&
                        htmlFragment[++i] == '=' &&
                        // TODO should allow single quotes too: '
                        htmlFragment[++i] == '"')
                    {
                        classValueStart = i + 1;

                        while (++i < fragmentLength && htmlFragment[i] != '"') ;// Note that in attributes, double quotes are escapsed as &quot;, so we don't need to check for '\' before '"'s
                    }
                }
            }

            // Invalid fragment or fragment has no tags
            return (-1, -1, -1, -1);
        }
        #endregion

        /// <summary>
        /// Writes children with the specified implicit paragraphs setting.
        /// </summary>
        public static HtmlRenderer WriteChildren(this HtmlRenderer htmlRenderer, ContainerBlock containerBlock, bool implicitParagraphs)
        {
            bool initialImplicitParagraph = htmlRenderer.ImplicitParagraph;
            htmlRenderer.ImplicitParagraph = implicitParagraphs;

            htmlRenderer.WriteChildren(containerBlock);

            htmlRenderer.ImplicitParagraph = initialImplicitParagraph;

            return htmlRenderer;
        }

        /// <summary>
        /// Writes <see cref="LeafBlock.Inline"/> with the specified enable HTML for inlinesetting.
        /// </summary>
        public static HtmlRenderer WriteLeafInline(this HtmlRenderer htmlRenderer, LeafBlock leafBlock, bool enableHtmlForInline)
        {
            bool initialEnableHtmlForInline = htmlRenderer.EnableHtmlForInline;
            htmlRenderer.EnableHtmlForInline = enableHtmlForInline;

            htmlRenderer.WriteLeafInline(leafBlock);

            htmlRenderer.EnableHtmlForInline = initialEnableHtmlForInline;

            return htmlRenderer;
        }
    }
}
