using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace List
{
    static class Program
    {
        static void Main()
        {
            //const string Code = "(list \"asdf asdf as' ( \"  (++ 3 4) 1 2 'aa ver)";
            const string Code = "(+ 2 3 (+ 2 2 4 56 7) )";
            //var input = new StringReader(Code);
            var input = Console.In;


            var top = new Environment();
            AddBuiltinFunctions(top);
            AddExtraFunctions(top);

            Console.Write("> ");
            var scan = new Scanner(input);
            while (scan.Peek().Item1 != TokenType.EOF)
            {
                try
                {
                    var val = Parse(scan);
                    //Console.WriteLine(val);
                    Console.WriteLine(val.Eval(top));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Error: {0}", ex.Message);
                }
                Console.Write("> ");
            }


            //foreach (var tk in scan.GetTokens2().Take(15))
            //{
            //    Console.WriteLine("{0,10}: {1}", tk.Item1, tk.Item2);
            //}
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
            top.Add(new[] { "cons", "list" }, new BuiltinFunction((env, args) =>
            {
                return args.Map(v => v.Eval(env));
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
                Console.Write(args.Val.Eval(env));
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
                env[name] = newFun;
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
                var fun = args.Val.Eval(env) as UserFunction;
                if (fun == null)
                    throw new Exception("Could not find user function.");
                return new List(fun.mArgNames, new List(fun.mFun, List.Nil));
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
            top.Eval("(defun second (l) '(car (cdr l)))");
            top.Eval("(defun third (l) '(car (cdr (cdr l))))");
        }
    }
}
