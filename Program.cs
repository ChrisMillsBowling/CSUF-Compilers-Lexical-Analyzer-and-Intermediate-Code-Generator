using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MMALexerCSharp
{
    class Program
    {
        public static string[] Keywords = {"int", "float", "bool", "if", "else", "then", "do", "while", "whileend", "do", "doend", "for", "and", "or", "function"};
        public static string[] Operators = { "*", "+", "-", "=", "/", ">", "<", "%"};
        public static string[] Seperators = { "'", "(", ")", "{", "}", "[", "]", ",", ".", ":", ";", "!" };
        public static char[] Integers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static int[,] DFATable =
            {
            //Id
           //1,2,3,4,5,6
            {4,5,5,5,5,5},
            //Operators
            {3,5,5,5,5,5},
            //Keywords
            {2,5,5,5,5,5},
            //Separators
            {1,5,5,5,5,5}
            };

        public static int[,] DFAFloatIntTable =
        {
                
            //state 3 is float, state 2 is int. 
            //state 4 is a final go back state. 
           //int, . , 0
            //1
            {2,4,4},
            //2
            {2,3,4},
            //3
            {3,4,4},
            //4
            {4,4,4}

        };
        public static int MachineState = 0;


        static void Lexer(List<String> Token_Holder, StreamWriter sw, List<String> Token_Eval_Holder)
        {
            bool comment = false;

            foreach (string s in Token_Holder)
            {
                MachineState = 0;
                while (MachineState == 0)
                {
                    string word = s.Trim();
                    if (word == "!") { comment = !comment; break; }
                    if (comment) { break; }
                    if (word == " " || word == "" || word == "  ")
                    {
                        if (s == "\n" || s == "\r" || s =="\r\n") {
                            TheNumber++;
                        }
                        break;
                    }

                    LineNumber.Add(TheNumber);
                    Main_Token_Holder.Add(word);
                    if (Keywords.Contains(word))
                    {
                        MachineState = DFATable[2, MachineState];
                    }
                    else if (Seperators.Contains(word))
                    {
                        MachineState = DFATable[3, MachineState];
                    }
                    else if (Operators.Contains(word))
                    {
                        MachineState = DFATable[1, MachineState];
                    }
                    else
                    {
                        MachineState = DFATable[0, MachineState];
                    }


                    switch (MachineState)
                    {
                        case 1:
                            Console.Write("\t\t\t SEPARATOR");

                            Token_Eval_Holder.Add("SEPARATOR");
                            break;
                        case 2:
                            Console.Write("\t\t\t KEYWORDS");

                            Token_Eval_Holder.Add("KEYWORDS");
                            break;
                        case 3:
                            Console.Write("\t\t\t OPERATORS");
                            
                            Token_Eval_Holder.Add("OPERATORS");
                            break;
                        case 4:
                            int intstate = 1;
                            //Seperate check for int vs. float
                            if (Integers.Contains(word[0]))
                            {
                                foreach (char c in word)
                                {
                                    if (Integers.Contains(c))
                                    {
                                        intstate = DFAFloatIntTable[intstate - 1, 0];
                                    }
                                    else if (c == '.')
                                    {
                                        intstate = DFAFloatIntTable[intstate - 1, 1];
                                    }
                                    else
                                    {
                                        intstate = DFAFloatIntTable[intstate - 1, 2];
                                    }

                                }
                                if (intstate == 3)
                                {
                                    Console.Write("\t\t\t FLOAT");

                                    Token_Eval_Holder.Add("FLOAT");
                                    break;
                                }
                                Console.Write("\t\t\t INT");

                                Token_Eval_Holder.Add("INT");
                                break;
                            }
                            Console.Write("\t\t\t IDENTIFIERS");

                            Token_Eval_Holder.Add("IDENTIFIERS");
                            break;
                        default:
                            break;
                    }

                    Console.Write("\n");

                }
            }
        }

        public static bool ThereAreErrors = false;
        public static int Lexer_Location = 0;
        static int LexerTreeCallLocation(List<String> Token_Holder, StreamWriter sw, List<String> Token_Eval_Holder) {
            Lexer_Location++;
            return Lexer_Location - 1;
        }

        static void ParseTree(List<String> Token_Holder, StreamWriter sw, List<String> Token_Eval_Holder) {
            bool IFPARSE = false;
            int WHILEPARSE = 0;
            for (int i = 1; i < Token_Holder.Count; i++) {
                if (Token_Holder[i] == "if") { IFPARSE = true; }
                if (Token_Holder[i] == "while") { WHILEPARSE++; }
                if (Token_Holder[i] == ";")
                { continue; } // Skip reckongizing the  ; for now.
                    switch (Token_Holder[i]) {
                    case "+":
                        if (Q(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (T(Token_Holder[i - 1], Token_Eval_Holder[i - 1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: Q -> +TQ. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: Q -> +TQ. Line - " + LineNumber[i]);
                        }
                        else if (R(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (F(Token_Holder[i - 1], Token_Eval_Holder[i - 1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> Epsilon. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> Epsilon. Line - " + LineNumber[i]);
                        }
                        continue;
                        break;
                    case "-":
                        if (Q(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (T(Token_Holder[i - 1], Token_Eval_Holder[i - 1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: Q -> -TQ. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: Q -> -TQ. Line - " + LineNumber[i]);
                        }
                        else if (R(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (F(Token_Holder[i - 1], Token_Eval_Holder[i - 1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> Epsilon. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> Epsilon. Line - " + LineNumber[i]);
                        }
                        continue;
                        break;
                    case "/":
                        if (R(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (F(Token_Holder[i - 1], Token_Eval_Holder[i - 1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> /FR. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> /FR. Line - " + LineNumber[i]);
                        }
                        continue;
                        break;
                    case "*":
                        if (R(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (F(Token_Holder[i - 1], Token_Eval_Holder[i - 1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> +FR. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> +FR. Line - " + LineNumber[i]);
                        }
                        continue;
                        break;
                    case "(":
                        if (E(Token_Holder[i], Token_Eval_Holder[i]))
                        {
                            if (Q(Token_Holder[i + 1], Token_Eval_Holder[i + 1]) &&
                                T(Token_Holder[i - 1], Token_Eval_Holder[i - 1]))
                            {
                                Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: E -> TQ. Line - " + LineNumber[i]);
                                sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: E -> TQ. Line - " + LineNumber[i]);
                                continue;
                            }
                        }
                        else if (T(Token_Holder[i], Token_Eval_Holder[i]))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: T -> FR. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: T -> FR. Line - " + LineNumber[i]);
                            continue;
                        }
                        Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: F -> (E) | i. Line - " + LineNumber[i]);
                        sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: F -> (E) | i. Line - " + LineNumber[i]);
                        continue;

                        break;
                    case ")":
                        if (Q(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (T(Token_Holder[i - 1], Token_Eval_Holder[i - 1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: Q -> Epsilon. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: Q -> Epsilon. Line - " + LineNumber[i]);
                            continue;
                        }
                        else if (R(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (F(Token_Holder[i-1], Token_Eval_Holder[i-1])))
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> Epsilon. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: R -> Epsilon. Line - " + LineNumber[i]);
                            continue;
                        }
                        
                        break;
                    case "$":
                        break;
                    default:
                        if (Token_Eval_Holder[i] == "IDENTIFIERS")
                        {
                            if (E(Token_Holder[i], Token_Eval_Holder[i]) && 
                                (Token_Holder[i-1] == "=" || Token_Holder[i - 1] == "("))
                            {
                                Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: E -> TQ. Line - " + LineNumber[i]);
                                sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: E -> TQ. Line - " + LineNumber[i]);
                                continue;
                            }
                            else if (T(Token_Holder[i], Token_Eval_Holder[i]) &&
                                (Token_Holder[i - 1] == "+" || Token_Holder[i - 1] == "-"))
                            {
                                Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: T -> FR. Line - " + LineNumber[i]);
                                sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: T -> FR. Line - " + LineNumber[i]);
                                continue;
                            }
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: S-> i = E. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: S-> i = E. Line - " + LineNumber[i]);
                            continue;
                        }
                        else if (Token_Eval_Holder[i] == "EPSILON")
                        {
                            
                        }
                        else if (Token_Eval_Holder[i - 1] == "IDENTIFIERS" && Token_Holder[i] == "=")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: S-> i = E. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: S-> i = E. Line - " + LineNumber[i]);
                            continue;
                        }
                        else if (IFPARSE && Token_Holder[i] == "then")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <if Non-Terminal> -> if <if statement body> then Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <if Non-Terminal> -> if <if statement body> then Line - " + LineNumber[i]);
                            IFPARSE = false;
                            continue;
                        }
                        else if (WHILEPARSE > 0 && Token_Holder[i] == "while")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <While Non-Terminal> -> While <while statement body> do Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <While Non-Terminal> -> While <While statement body> do Line - " + LineNumber[i]);
                            continue;
                        }
                        else if (WHILEPARSE > 0 && Token_Holder[i] == "do")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <while Non-Terminal> -> while <while statement body> do Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <while Non-Terminal> -> while <while statement body> do Line - " + LineNumber[i]);
                            WHILEPARSE--;
                            continue;
                        }
                        else if (IFPARSE && Token_Holder[i] == "if")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <if Non-Terminal> -> if <if statement body> then Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <if Non-Terminal> -> if <if statement body> then Line - " + LineNumber[i]);
                            IFPARSE = true;
                            continue;
                        }
                        else if (Token_Holder[i] == "int")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <int Non-Terminal> -> int S. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <int non-Terminal> -> int S. Line - " + LineNumber[i]);

                            continue;
                        }
                        else if (Token_Holder[i] == "bool")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <bool Non-Terminal> -> bool S. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: <bool non-Terminal> -> bool S. Line - " + LineNumber[i]);
                            continue;
                        }
                        else if (Token_Eval_Holder[i] == "INT")
                        {
                            Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: S -> i = E. Line - " + LineNumber[i]);
                            sw.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Rule Used: S -> i = E. Line - " + LineNumber[i]);
                            continue;
                        }
                        break;
                }

                ThereAreErrors = true;
                Console.WriteLine("\n\t" + Token_Holder[i] + " | " + Token_Eval_Holder[i] + " | Error: No Rule Selected: Line - " + LineNumber[i]);
            }
        }

        /// <summary>
        /// Production rules evaluating the first and follow sets below.
        /// Rules: T, Q, F, R, S(root), E
        /// </summary>
        /// 

        public static bool T(string Token, string Token_Eval) {
            //Check the First Set of T
            if (Token == "(" || Token_Eval == "IDENTIFIERS") {
                return true;
            }
            //Check the Follow Set of T
            if (Token == "+" || Token == "-" || Token == "$" || Token == ")") {
                return true;
            }
            return false;
        }

        public static bool Q(string Token, string Token_Eval)
        {
            //Check the First Set of Q
            if (Token == "+" || Token == "-" || Token_Eval == "EPSILON")
            {
                return true;
            }
            //Check the Follow Set of Q
            if (Token == "$" || Token == ")")
            {
                return true;
            }
            return false;
        }

        public static bool F(string Token, string Token_Eval)
        {
            //Check the First Set of F
            if (Token == "(" ||  Token_Eval == "IDENTIFIERS")
            {
                return true;
            }
            //Check the Follow Set of F
            if (Token == "$" || Token == ")" || Token == "*" || Token == "/" || Token == "+" || Token == "-")
            {
                return true;
            }
            return false;
        }

        public static bool R(string Token, string Token_Eval)
        {
            //Check the First Set of R
            if (Token == "*" || Token == "/" || Token_Eval == "EPSILON")
            {
                return true;
            }
            //Check the Follow Set of R
            if (Token == "$" || Token == ")" || Token == "+" || Token == "-")
            {
                return true;
            }
            return false;
        }

        public static bool rootS(string Token, string Token_Eval)
        {
            //Check the First Set of S
            if (Token_Eval == "IDENTIFIERS")
            {
                return true;
            }
            //Check the Follow Set of S
            if (Token == "$")
            {
                return true;
            }
            return false;
        }

        public static bool E(string Token, string Token_Eval)
        {
            //Check the First Set of E
            if (Token == "(" || Token_Eval == "IDENTIFIERS")
            {
                return true;
            }
            //Check the Follow Set of E
            if (Token == "$" || Token == ")")
            {
                return true;
            }
            return false;
        }


        public static List<int> LineNumber = new List<int>();
        public static List<String> Main_Token_Holder = new List<string>();
        public static int TheNumber = 1;
        static void Main(string[] args)
        {
            
            Console.WriteLine("Compilers: Arteaga, Marcos, Mills-Bowling - Part 2 - Final Iteration");


            string text = System.IO.File.ReadAllText("input.txt");
            
            List<String> Token_Holder = Regex.Split(text, @"(\()|(\))|(\,)|(\;)|(\!)|(\{)|(\})|(\[)|(\])|(\:)|(\*)|(\+)|(\-)|(\=)|(\/)|(\<)|(\>)|(\%)|(\r)| ", RegexOptions.None).ToList();
            Token_Holder.RemoveAll(T => T == "");
            List<String> Token_Eval_Holder = new List<string>();
            FileStream FILE = new FileStream("input.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            FileStream OUTPUT = new FileStream("output.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(OUTPUT);
            LineNumber.Add(1);
            Lexer(Token_Holder, sw, Token_Eval_Holder);

            ParseTree(Main_Token_Holder, sw, Token_Eval_Holder);
            if (ThereAreErrors)
            {
                Console.WriteLine("\n\nError: There were errors in this code."); 
            }
            else { Console.WriteLine("\n\nSuccess! No Errors in this code!.");
                sw.WriteLine("\n\nSuccess! No Errors in this code!."); }
            sw.Close();
            Console.ReadKey();
        }
    }
}
