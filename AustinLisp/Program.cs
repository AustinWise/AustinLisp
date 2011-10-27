using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AustinLisp
{
    static class Program
    {
        static void Main()
        {
            var top = new Environment();
            AddBuiltinFunctions(top);
            AddExtraFunctions(top);

            Console.Write("> ");
            var scan = new Scanner(Console.In);
            while (scan.Peek().Item1 != TokenType.EOF)
            {
                try
                {
                    Console.WriteLine(Parse(scan).Eval(top));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Error: {0}", ex.Message);
                }
                Console.Write("> ");
            }
        }

        static Value Parse(Scanner scan)
        {
            var tk = scan.Next();

            if (tk.Item1 == TokenType.Word)
                return new Word(tk.Item2);
            else if (tk.Item1 == TokenType.Nil)
                return List.Nil;
            else if (tk.Item1 == TokenType.True)
                return True.T;
            else if (tk.Item1 == TokenType.Number)
                return new Int(int.Parse(tk.Item2));
            else if (tk.Item1 == TokenType.String)
                return new String(tk.Item2);
            else if (tk.Item1 == TokenType.LParen)
            {
                var cons = new Stack<Value>();
                while ((tk = scan.Peek()).Item1 != TokenType.RParen && tk.Item1 != TokenType.EOF)
                {
                    cons.Push(Parse(scan));
                }

                if (tk.Item1 != TokenType.RParen)
                    throw new Exception("Missing RParen.");

                scan.Next();

                List ret = List.Nil;
                while (cons.Count != 0)
                {
                    ret = new List(cons.Pop(), ret);
                }
                return ret;
            }
            else if (tk.Item1 == TokenType.Quote)
                return new Quote(Parse(scan));
            else
                throw new Exception("Unknown token type.");
        }

        static void AddBuiltinFunctions(Environment top)
        {
            //Missing: cond, atom
            top.Add("+", new BuiltinFunction((env, args) =>
            {
                int accum = 0;
                while (args != List.Nil)
                {
                    accum += ((Int)args.Val.Eval(env)).Val;
                    args = args.Next;
                }
                return new Int(accum);
            }));
            top.Add("list", new BuiltinFunction((env, args) =>
            {
                return args.Map(v => v.Eval(env));
            }));
            top.Add("cons", new BuiltinFunction((env, args) =>
            {
                return new List(args.Val.Eval(env), (List)args.Next.Val.Eval(env));
            }));
            top.Add("quote", new BuiltinFunction((env, args) =>
            {
                return args.Val;
            }));
            top.Add("car", new BuiltinFunction((env, args) =>
            {
                var l = args.Val.Eval(env) as List;
                return l.Val;
            }));
            top.Add("cdr", new BuiltinFunction((env, args) =>
            {
                var l = args.Val.Eval(env) as List;
                return l.Next;
            }));
            top.Add("print", new BuiltinFunction((env, args) =>
            {
                Value v = args.Val.Eval(env);
                string str;
                if (v is String)
                    str = ((String)v).Val;
                else
                    str = v.ToString();
                Console.WriteLine(str);
                return List.Nil;
            }));
            top.Add("if", new BuiltinFunction((env, args) =>
            {
                if (args.Val.Eval(env).IsTrue())
                    return args.Next.Val.Eval(env);
                else
                    return args.Next.Next.Val.Eval(env);
            }));
            top.Add("listp", new BuiltinFunction((env, args) =>
            {
                if (args.Val.Eval(env) is List)
                    return True.T;
                else
                    return List.Nil;
            }));
            top.Add("defun", new BuiltinFunction((env, args) =>
            {
                var name = ((Word)args.Val).Val;
                var formalArgs = ((List)args.Next.Val);
                var body = args.Next.Next.Map(v => v.Eval(env));
                var newFun = new UserFunction(formalArgs, body);
                top[name] = newFun;
                return newFun;
            }));
            top.Add("defmacro", new BuiltinFunction((env, args) =>
            {
                var name = ((Word)args.Val).Val;
                var formalArgs = ((List)args.Next.Val);
                var body = (List)args.Next.Next.Val;
                var newFun = new UserMacro(formalArgs, body);
                top[name] = newFun;
                return newFun;
            }));
            top.Add("exit", new BuiltinFunction((env, args) =>
            {
                var exitArg = args.Val.Eval(env) as Int;
                int exitCode = 0;
                if (exitArg != null)
                    exitCode = exitArg.Val;
                System.Environment.Exit(exitCode);
                return List.Nil;
            }));
            top.Add("eq", new BuiltinFunction((env, args) =>
            {
                var v1 = args.Val.Eval(env);
                var v2 = args.Next.Val.Eval(env);
                if (v1.Equals(v2))
                    return True.T;
                else
                    return List.Nil;
            }));
            top.Add("code", new BuiltinFunction((env, args) =>
            {
                var val = args.Val.Eval(env);
                if (val is UserFunction)
                {
                    var fun = args.Val.Eval(env) as UserFunction;
                    return new List(fun.mArgNames, new List(fun.mFun, List.Nil));
                }
                else if (val is UserMacro)
                {
                    var fun = args.Val.Eval(env) as UserMacro;
                    return new List(fun.mArgNames, new List(fun.mFun, List.Nil));
                }
                else
                    throw new Exception("Could not find user function.");
            }));
            top.Add("reverse", new BuiltinFunction((env, args) =>
            {
                var l = (List)args.Val.Eval(env);
                var ret = new List<Value>();
                while (l != List.Nil)
                {
                    ret.Add(l.Val);
                    l = l.Next;
                }
                foreach (var v in ret)
                {
                    l = new List(v, l);
                }
                return l;
            }));
            top.Add("read", new BuiltinFunction((env, args) =>
            {
                var fileName = ((String)args.Val.Eval(env)).Val;
                var values = new Stack<Value>();
                using (var file = new StreamReader(fileName))
                {
                    var scan = new Scanner(file);
                    while (scan.Peek().Item1 != TokenType.EOF)
                    {
                        values.Push(Parse(scan));
                    }
                }
                var ret = List.Nil;
                foreach (var v in values)
                {
                    ret = new List(v, ret);
                }
                return ret;
            }));
            top.Add("eval", new BuiltinFunction((env, args) =>
            {
                return args.Val.Eval(env).Eval(env);
            }));
            top.Add("env", new BuiltinFunction((env, args) =>
            {
                return env.AsList();
            }));
        }

        static void Eval(this Environment env, string code)
        {
            var scan = new Scanner(new StringReader(code));
            var val = Parse(scan);
            val.Eval(env);
        }

        static void AddExtraFunctions(Environment top)
        {
            top.Eval("(defun map (fn l) '(if (eq l nil) () (cons (fn (car l)) (map fn (cdr l)))))");
            top.Eval("(defun load (file) '(map eval (read file)))");
            top.Eval("(load \"ExtraFunctions.lisp\")");
        }
    }
}
