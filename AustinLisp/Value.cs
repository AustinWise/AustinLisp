using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AustinLisp
{
    interface IFunction
    {
        Value Execute(Environment env, List args);
    }

    abstract class Value
    {
        public abstract void ToString(StringBuilder sb);
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        public abstract Value Eval(Environment env);

        public bool IsTrue()
        {
            return this != List.Nil;
        }
    }

    class BuiltinFunction : Value, IFunction
    {
        private readonly Func<Environment, List, Value> Fun;
        public BuiltinFunction(Func<Environment, List, Value> fun)
        {
            this.Fun = fun;
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("#'bultinFunc");
            sb.Append(GetHashCode());
        }

        public override Value Eval(Environment env)
        {
            return this;
        }

        public Value Execute(Environment env, List args)
        {
            return Fun(env, args);
        }
    }

    class Int : Value
    {
        public readonly int Val;
        public Int(int v)
        {
            this.Val = v;
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append(Val);
        }

        public override Value Eval(Environment env)
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            Int other = obj as Int;
            if (other == null)
                return false;
            return this.Val == other.Val;
        }

        public override int GetHashCode()
        {
            return Val;
        }
    }

    class Word : Value
    {
        public readonly string Val;
        public Word(string v)
        {
            this.Val = v;
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append(Val);
        }

        public override Value Eval(Environment env)
        {
            return env[Val];
        }

        public override bool Equals(object obj)
        {
            var other = obj as Word;
            if (other == null)
                return false;
            return this.Val == other.Val;
        }

        public override int GetHashCode()
        {
            return Val.GetHashCode();
        }
    }

    class String : Value
    {
        public readonly string Val;
        public String(string v)
        {
            this.Val = v;
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append('"');
            sb.Append(Val);
            sb.Append('"');
        }

        public override Value Eval(Environment env)
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            var other = obj as String;
            if (other == null)
                return false;
            return this.Val == other.Val;
        }

        public override int GetHashCode()
        {
            return Val.GetHashCode();
        }
    }

    class List : Value
    {
        public static readonly List Nil = new List();

        public readonly Value Val;
        public readonly List Next;
        private List()
        {
            this.Next = this;
            this.Val = this;
        }

        public List(Value v, List n)
        {
            if (v == null || n == null)
                throw new ArgumentNullException();

            this.Val = v;
            this.Next = n;
        }

        public override Value Eval(Environment env)
        {
            if (this == Nil)
                return this;
            IFunction fun = Val as IFunction;
            if (fun == null)
                fun = (IFunction)env[((Word)Val).Val];
            return fun.Execute(env, Next);
        }

        private static void ToStringInner(List l, StringBuilder sb)
        {
            if (l == Nil)
                return;

            l.Val.ToString(sb);
            if (l.Next != Nil)
                sb.Append(' ');

            ToStringInner(l.Next, sb);
        }

        public override void ToString(StringBuilder sb)
        {
            if (this == Nil)
                sb.Append("nil");
            else
            {
                sb.Append('(');
                ToStringInner(this, sb);
                sb.Append(')');
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as List;
            if (other == null)
                return false;

            //base case when we get to the end of both lists
            if (this == Nil && other == Nil)
                return true;

            if (!this.Val.Equals(other.Val))
                return false;
            else
                return this.Next.Equals(other.Next);
        }

        public List Map(Func<Value, Value> f)
        {
            return Map(this, f);
        }

        private static List Map(List l, Func<Value, Value> f)
        {
            if (l == Nil)
                return Nil;
            return new List(f(l.Val), Map(l.Next, f));
        }
    }

    class Quote : Value
    {
        public readonly Value Val;
        public Quote(Value v)
        {
            if (v == null)
                throw new ArgumentNullException();
            Val = v;
        }
        public override void ToString(StringBuilder sb)
        {
            sb.Append('\'');
            Val.ToString(sb);
        }

        public override Value Eval(Environment env)
        {
            return Val;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Quote;
            if (other == null)
                return false;
            return this.Val.Equals(other.Val);
        }
    }

    class True : Value
    {
        public static readonly True T = new True();
        private True()
        {
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("t");
        }

        public override Value Eval(Environment env)
        {
            return this;
        }
    }

    class UserFunction : Value, IFunction
    {
        public readonly List mArgNames;
        public readonly List mFun;

        public UserFunction(List args, List fun)
        {
            this.mFun = fun;
            this.mArgNames = args;
        }

        public override void ToString(StringBuilder sb)
        {
            sb.Append("#'userFun" + this.GetHashCode());
        }

        public override Value Eval(Environment env)
        {
            return this;
        }

        public Value Execute(Environment env, List args)
        {
            var oldEnv = env;
            env = new Environment(env);
            var argNames = mArgNames;
            while (argNames != List.Nil && args != List.Nil)
            {
                var name = ((Word)argNames.Val).Val;
                env.Add(name, args.Val.Eval(oldEnv));

                args = args.Next;
                argNames = argNames.Next;
            }
            if (argNames != List.Nil || args != List.Nil)
                throw new Exception("Wrong number of arguments.");

            var fun = mFun;
            Value ret = List.Nil;
            while (fun != List.Nil)
            {
                ret = fun.Val.Eval(env);
                fun = fun.Next;
            }
            return ret;
        }

        public override bool Equals(object obj)
        {
            var other = obj as UserFunction;
            if (other == null)
                return false;
            return this.mFun.Equals(other.mFun);
        }
    }
}
