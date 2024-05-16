/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Loxodon.Framework.Utilities
{
    public class StringSpliter : IEnumerator<char>
    {
        private static readonly char[] SEPARATOR = new char[] { ',' };
        [ThreadStatic]
        private static StringSpliter spliter;
        private static StringSpliter Spliter
        {
            get
            {
                if (spliter == null)
                    spliter = new StringSpliter();
                return spliter;
            }
        }

        public static string[] Split(string input, params char[] characters)
        {
            return Split(input, characters, StringSplitOptions.None);
        }

        public static string[] Split(string input, char[] characters, StringSplitOptions options)
        {
            if (string.IsNullOrEmpty(input))
                return new string[0];

            var spliter = Spliter;
            try
            {
                spliter.Reset(input, characters);
                return spliter.Split(options);
            }
            finally
            {
                spliter.Clear();
            }
        }

        private string text;
        private char[] separators;
        private int total = 0;
        private int pos = -1;
        private readonly List<string> items = new List<string>();
        private StringSpliter()
        {
        }

        public void Reset(string text, char[] separators)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Invalid argument", "text");

            if (separators == null || separators.Length == 0)
                this.separators = SEPARATOR;
            else
                this.separators = separators;

            this.text = text;
            total = this.text.Length;
            pos = -1;
            items.Clear();
        }

        public char Current => text[pos];

        object IEnumerator.Current => text[pos];

        public void Dispose()
        {
            text = null;
            pos = -1;
        }

        public bool MoveNext()
        {
            if (pos < total - 1)
            {
                pos++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            pos = -1;
            items.Clear();
        }

        public void Clear()
        {
            text = null;
            separators = null;
            pos = -1;
            total = 0;
            items.Clear();
        }

        public string[] Split(StringSplitOptions options)
        {
            while (MoveNext())
            {
                char ch = Current;
                if (separators.Contains(ch))
                {
                    if (options == StringSplitOptions.None)
                        items.Add("");
                    continue;
                }

                string content = ReadString(separators);
                items.Add(content);
            }

            if (separators.Contains(Current) && options == StringSplitOptions.None)
                items.Add("");

            return items.ToArray();
        }

        private bool IsEOF()
        {
            return pos >= total;
        }

        private void ReadStructString(StringBuilder buf, char start, char end)
        {
            char ch = Current;
            if (ch != start)
                throw new Exception($"Error parsing string , unexpected quote character {ch} in text {text}");

            buf.Append(ch);

            while (MoveNext())
            {
                ch = Current;
                if (ch == '(')
                {
                    ReadStructString(buf, '(', ')');
                    continue;
                }
                else if (ch == '[')
                {
                    ReadStructString(buf, '[', ']');
                    continue;
                }
                else if (ch == '{')
                {
                    ReadStructString(buf, '{', '}');
                    continue;
                }
                else if (ch == '<')
                {
                    ReadStructString(buf, '<', '>');
                    continue;
                }

                buf.Append(ch);
                if (ch == end)
                    return;
            }

            throw new Exception($"Not found the end character '{end}' in the text {text}.");
        }

        private void ReadQuotedString(StringBuilder buf, char start, char end)
        {
            char prev = '\0';
            char ch = Current;
            if (ch != start)
                throw new Exception($"Error parsing string , unexpected quote character {ch} in text {text}");

            while (MoveNext())
            {
                prev = ch;
                ch = Current;
                if (prev != '\\' && ch == end)
                    return;

                buf.Append(ch);
            }

            throw new Exception($"Not found the end character '{end}' in the text {text}.");
        }

        private string ReadString(char[] separators)
        {
            StringBuilder buf = new StringBuilder();
            char ch = Current;
            do
            {
                ch = Current;
                if (ch == '(')
                {
                    ReadStructString(buf, '(', ')');
                }
                else if (ch == '[')
                {
                    ReadStructString(buf, '[', ']');
                }
                else if (ch == '{')
                {
                    ReadStructString(buf, '{', '}');
                }
                else if (ch == '<')
                {
                    ReadStructString(buf, '<', '>');
                }
                else if (ch == '\'')
                {
                    ReadQuotedString(buf, '\'', '\'');
                }
                else if (ch == '\"')
                {
                    ReadQuotedString(buf, '\"', '\"');
                }
                else
                {
                    if (separators.Contains(ch))
                        break;

                    buf.Append(ch);
                }
            } while (MoveNext());

            buf.Replace("&quot;", "\"");
            buf.Replace("\\\"", "\"");
            return buf.ToString();
        }
    }
}
