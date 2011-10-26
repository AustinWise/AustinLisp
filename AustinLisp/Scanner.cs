using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AustinLisp
{
    enum TokenType
    {
        EOF,
        LParen,
        RParen,
        Word,
        String,
        Number,
        Nil,
        True,
        Quote,
    }
    class Scanner
    {
        private TextReader mTr;
        private IEnumerator<Tuple<TokenType, string>> mTokens;
        private Tuple<TokenType, string> mCurrent = null;
        public Scanner(TextReader tr)
        {
            this.mTr = tr;
            this.mTokens = GetTokens2().GetEnumerator();
        }

        private void EnsureToken()
        {
            if (mCurrent != null)
                return;
            if (!mTokens.MoveNext())
                throw new Exception("MoveNext failed.");
            mCurrent = mTokens.Current;
        }

        public Tuple<TokenType, string> Next()
        {
            EnsureToken();
            var cur = mCurrent;
            mCurrent = null;
            return cur;
        }

        public Tuple<TokenType, string> Peek()
        {
            EnsureToken();
            return mCurrent;
        }

        private static Tuple<TokenType, string> Tk(TokenType t, string v)
        {
            return new Tuple<TokenType, string>(t, v);
        }

        private static Tuple<TokenType, string> Tk(TokenType t)
        {
            return new Tuple<TokenType, string>(t, null);
        }

        private Tuple<TokenType, string> TkWord(char firstChar)
        {
            StringBuilder sb = new StringBuilder(firstChar.ToString());

            int intChar;
            while ((intChar = mTr.Peek()) != -1)
            {
                char ch = (char)intChar;
                if (char.IsWhiteSpace(ch) || ch == '(' || ch == ')' || ch == '\'' || ch == '"')
                    break;
                sb.Append(ch);
                mTr.Read();
            }

            return new Tuple<TokenType, string>(TokenType.Word, sb.ToString());
        }

        private Tuple<TokenType, string> TkString()
        {
            StringBuilder sb = new StringBuilder();

            int intChar;
            while ((intChar = mTr.Read()) != -1)
            {
                char ch = (char)intChar;
                if (ch == '"')
                    break;
                sb.Append(ch);
            }

            if (intChar == -1)
                throw new Exception("EOF reached with no end of string literal.");

            return new Tuple<TokenType, string>(TokenType.Word, sb.ToString());
        }

        private IEnumerable<Tuple<TokenType, string>> GetTokens2()
        {
            int i;
            foreach (var tk in GetTokens())
            {
                if (tk.Item1 == TokenType.Word)
                {
                    if (tk.Item2 == "nil")
                        yield return Tk(TokenType.Nil);
                    else if (tk.Item2 == "t")
                        yield return Tk(TokenType.True);
                    else if (int.TryParse(tk.Item2, out i))
                        yield return Tk(TokenType.Number, tk.Item2);
                    else
                        yield return tk;
                }
                else
                    yield return tk;
            }
        }

        private IEnumerable<Tuple<TokenType, string>> GetTokens()
        {
            int intChar;
            while ((intChar = mTr.Read()) != -1)
            {
                char ch = (char)intChar;
                if (char.IsWhiteSpace(ch))
                    continue;

                if (ch == '(')
                    yield return Tk(TokenType.LParen);
                else if (ch == ')')
                    yield return Tk(TokenType.RParen);
                else if (ch == '\'')
                    yield return Tk(TokenType.Quote);
                else if (ch == '"')
                    yield return TkString();
                else
                    yield return TkWord(ch);
            }

            while (true)
            {
                yield return Tk(TokenType.EOF);
            }
        }
    }
}
