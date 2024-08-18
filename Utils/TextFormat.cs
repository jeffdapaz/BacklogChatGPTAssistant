using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JeffPires.BacklogChatGPTAssistant.Utils
{
    /// <summary>
    /// Internal static utility class containing methods for text formatting. 
    /// </summary>
    internal static class TextFormat
    {
        /// <summary>
        /// Removes all whitespace and break lines from a given string.
        /// </summary>
        /// <param name="textToMinify">The string to minify.</param>
        /// <returns>A minified version of the given string.</returns>
        public static string MinifyText(string textToMinify)
        {
            return Regex.Replace(textToMinify, @"\s+", " ");
        }

        /// <summary>
        /// Removes the specified characters from the given text.
        /// </summary>
        /// <param name="text">The text from which to remove the characters.</param>
        /// <param name="charsToRemove">The characters to remove from the text.</param>
        /// <returns>The text with the specified characters removed.</returns>
        public static string RemoveCharactersFromText(string text, params string[] charsToRemove)
        {
            foreach (string character in charsToRemove)
            {
                if (!string.IsNullOrEmpty(character))
                {
                    text = text.Replace(character, string.Empty);
                }
            }

            return text;
        }

        /// <summary>
        /// Removes blank lines from the given string result.
        /// </summary>
        /// <param name="result">The string from which blank lines should be removed.</param>
        /// <returns>A string with blank lines removed.</returns>
        public static string RemoveBlankLinesFromResult(string result)
        {
            return result.TrimPrefix("\r\n").TrimPrefix("\n\n").TrimPrefix("\n").TrimPrefix("\r").TrimSuffix("\r\n").TrimSuffix("\n\n").TrimSuffix("\n").TrimSuffix("\r");
        }

        /// <summary>
        /// Splits a given text into an array of strings, each representing a line. The method supports different line endings: "\r\n" (Windows), "\r" (old Mac), and "\n" (Unix/Linux).
        /// </summary>
        /// <param name="text">The text to be split into lines.</param>
        /// <returns>
        /// An array of strings, where each string is a line from the input text.
        /// </returns>
        public static string[] SplitTextByLine(string text)
        {
            return text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Removes lines from a given text that start with code tags
        /// </summary>
        /// <param name="text">Original text</param>
        /// <returns>Text without the code tags</returns>
        private static string RemoveLinesStartingWithCodeTags(string text)
        {
            string[] lines = SplitTextByLine(text);

            IEnumerable<string> filteredLines = lines.Where(line => !line.StartsWith("```"));

            string result = string.Join(Environment.NewLine, filteredLines);

            return RemoveBlankLinesFromResult(result);
        }

        /// <summary>
        /// Removes the language identifier from the code content if it exists.
        /// </summary>
        /// <param name="codeContent">The code content with a potential language identifier.</param>
        /// <returns>The code content without the language identifier.</returns>
        public static string RemoveLanguageIdentifier(string codeContent)
        {
            // Regex to match common language identifiers (e.g., "csharp", "javascript", etc.)
            Regex languageIdentifierRegex = new(@"```[^\s]+|```");

            return languageIdentifierRegex.Replace(codeContent, string.Empty).TrimStart();
        }
    }
}
