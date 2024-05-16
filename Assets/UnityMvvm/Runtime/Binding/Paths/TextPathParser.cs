

using System;
using System.Linq;

namespace Fusion.Mvvm
{
    public struct TextPathParser
    {
        public static Path Parse(string text)
        {
            return new TextPathParser(text).Parse();
        }

        private readonly string text;
        private readonly int total;
        private int pos;

        public TextPathParser(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Invalid argument", "text");

            this.text = text.IndexOf(' ') == -1 ? text : text.Replace(" ", "");
            if (string.IsNullOrEmpty(this.text) || this.text[0] == '.')
                throw new ArgumentException("Invalid argument", "text");

            total = this.text.Length;
            pos = -1;
        }

        public char Current => text[pos];

        public bool MoveNext()
        {
            if (pos++ < total - 1)
                return true;
            return false;
        }

        private bool IsEOF()
        {
            return pos >= total;
        }

        public Path Parse()
        {
            Path path = new Path();
            MoveNext();
            do
            {
                SkipWhiteSpaceAndCharacters('.');

                if (IsEOF())
                    break;

                if (Current.Equals('['))
                {
                    //parse index
                    ParseIndex(path);
                    SkipWhiteSpace();
                    if (!Current.Equals(']'))
                        throw new BindingException("Error parsing indexer , unterminated in text {0}", text);

                    if (MoveNext())
                    {
                        if (!Current.Equals('.'))
                            throw new BindingException("Error parsing path , unterminated in text {0}", text);
                    }
                }
                else if (char.IsLetter(Current) || Current == '_')
                {
                    //read member name
                    string memberName = ReadMemberName();
                    path.Append(new MemberNode(memberName));
                    if (!IsEOF() && !Current.Equals('.') && !Current.Equals('[') && !char.IsWhiteSpace(Current))
                        throw new BindingException("Error parsing path , unterminated in text {0}", text);
                }
                else
                {
                    throw new BindingException("Error parsing path , unterminated in text {0}", text);
                }
            } while (!IsEOF());
            return path;
        }

        private void ParseIndex(Path path)
        {
            if (!MoveNext())
                throw new BindingException("Error parsing string indexer , unterminated in text {0}", text);

            var ch = Current;
            if (ch == '\'' || ch == '\"')
            {
                var index = ReadQuotedString();
                path.AppendIndexed(index);
                MoveNext();
                return;
            }

            if (char.IsDigit(ch))
            {
                uint index = ReadUnsignedInteger();
                path.AppendIndexed((int)index);
                return;
            }

            throw new BindingException("Error parsing indexer , unterminated in text {0}", text);
        }

        private unsafe string ReadMemberName()
        {
            char* buffer = stackalloc char[128];
            int i = 0;
            do
            {
                var ch = Current;
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                    break;

                buffer[i++] = ch;

            } while (MoveNext());

            if (i <= 0)
                throw new BindingException("Error parsing member name , unterminated in text {0}", text);

            return new string(buffer, 0, i);
        }

        private unsafe uint ReadUnsignedInteger()
        {
            char* buffer = stackalloc char[128];
            int i = 0;
            do
            {
                var ch = Current;
                if (!char.IsDigit(ch))
                    break;

                buffer[i++] = ch;

            } while (MoveNext());

            uint index;
            string num = new string(buffer, 0, i);
            if (!uint.TryParse(num, out index))
                throw new BindingException("Unable to parse integer text from {0} in {1}", num, text);
            return index;
        }

        private unsafe string ReadQuotedString()
        {
            char ch = Current;
            if (ch != '\'' && ch != '\"')
                throw new BindingException("Error parsing string indexer , unexpected quote character {0} in text {1}",
                                       ch, text);

            if (!MoveNext())
                throw new BindingException("Error parsing string indexer , unterminated in text {0}", text);

            char* buffer = stackalloc char[128];
            int i = 0;
            do
            {
                ch = Current;
                if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '-')
                    break;

                buffer[i++] = ch;
            } while (MoveNext());

            if (i <= 0 || (ch != '\'' && ch != '\"'))
                throw new BindingException("Error parsing string indexer , unexpected quote character {0} in text {1}",
                                       ch, text);
            return new string(buffer, 0, i);
        }

        private void SkipWhiteSpace()
        {
            while (char.IsWhiteSpace(Current) && MoveNext())
            {
            }
        }

        private bool IsWhiteSpaceOrCharacter(char ch, params char[] characters)
        {
            return char.IsWhiteSpace(ch) || characters.Contains(ch);
        }

        private void SkipWhiteSpaceAndCharacters(params char[] characters)
        {
            while (IsWhiteSpaceOrCharacter(Current, characters) && MoveNext())
            {
            }
        }
    }
}