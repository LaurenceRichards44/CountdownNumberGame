using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Linq;

namespace CountdownNumberGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }


        class Game
        {
            static int score = 0;

            public void Start()
            {
                Console.Write("Press Y for practice mode, any other key for the main game: ");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    PracticeMode();
                }
                else
                {
                    MainGame();
                }
            }

            private void MainGame()
            {
                Console.Clear();
                Console.WriteLine("Main game mode is not implemented yet.");
            }

            private void PracticeMode()
            {
                GameNumbers numbers = new GameNumbers(10, 50);
                bool gameOver = false;

                Console.Clear();

                do
                {
                    numbers.ShowAllData();

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Current Score: " + score);

                    string equation = RequestExpression();
                    Console.Clear();

                    bool shifted = false;

                    if (!string.IsNullOrEmpty(equation))
                    {
                        int? result = EvaluateExpression(equation, numbers.guessNumbers);

                        if(result.HasValue)
                        {
                            int removedNums = numbers.RemoveNumber((int)result);
                            if (result != null && removedNums != 0)
                            {
                                Console.WriteLine("You deleted {0} number(s)! +{1} points!", removedNums, 10 * removedNums);
                                Console.WriteLine();
                            }else
                            {
                                Console.WriteLine("the number {0} doesn't exist. -15 points.", result);
                            }
                        }
                        Console.WriteLine();
                    }else
                    {
                        score -= 15;
                        Console.WriteLine("You didn't enter an expression. -15 points.");
                        Console.WriteLine();
                    }

                    shifted = numbers.Shift();

                    numbers.guessNumbers = GenerateRandomList(1, 10, 6);

                    if (!shifted)
                    {
                        gameOver = true;
                    }
                } while (!gameOver);


                Console.WriteLine("Game Over!!");
                Console.WriteLine();
                Console.WriteLine("Final score: {0}", score);
            }

            public class GameNumbers
            {
                Random rand = new Random();

                const int len = 20;
                const int spaces = 5;
                public int minValue;
                public int maxValue;

                public List<int?> numbers = new List<int?>();
                public List<int> guessNumbers = new List<int>();

                public GameNumbers(int min, int max)
                {
                    minValue = min;
                    maxValue = max;

                    for (int i = 0; i < len; i++)
                    {
                        if (i < spaces)
                            numbers.Add(null);
                        else
                            numbers.Add(rand.Next(minValue, maxValue));
                    }

                    guessNumbers = GenerateRandomList(1, 10, 6);
                }

                public bool Shift()
                {
                    int lastIndex = numbers.LastIndexOf(null);

                    if (lastIndex > -1)
                    {
                        numbers.RemoveAt(lastIndex);
                        numbers.Add(rand.Next(minValue, maxValue));
                    }
                    else
                        return false;

                    return true;

                }

                public void ShowAllData()
                {
                    Console.Write("|");
                    for (int i = 0; i < numbers.Count; i++)
                    {
                        if (numbers[i] != null)
                            Console.Write(numbers[i]);
                        else
                            Console.Write(" ");
                        Console.Write("|");
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.Write("Usable numbers: ");
                    foreach (int i in guessNumbers)
                    {
                        Console.Write("{0}, ", i);
                    }
                }

                public int RemoveNumber(int number)
                {
                    bool exists = numbers.Exists(x => x == number);

                    int count = 0;
                    while (numbers.Exists(x => x == number))
                    {
                        int index = numbers.IndexOf(number);
                        numbers[index] = null;
                        score += 10;
                        count++;
                    }

                    return count;
                }
            }

            static List<int> GenerateRandomList(int min, int max, int len)
            {
                Random rand = new Random();
                List<int> list = new List<int>();

                for (int i = 0; i < len; i++)
                {
                    list.Add(rand.Next(min, max));
                }

                return list;
            }

            static string RequestExpression()
            {
                Console.WriteLine();
                Console.Write("Enter expression: ");
                string input = Console.ReadLine();

                return input;
            }

            static int? EvaluateExpression(string expression, List<int> guessNumbers)
            {
                if (!CheckValidity(expression))
                    return null;

                try
                {
                    string noSpacesExpression = RemoveSpaces(expression);
                    List<string> tokens = Tokenize(noSpacesExpression);

                    if (!CheckApplicableNumbers(tokens, guessNumbers))
                        return null;

                    List<string> postFix = InfixToPostfix(tokens);
                    int? result = EvaluatePostfix(postFix);

                    return result;
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            static bool CheckValidity(string input)
            {
                string[] operators = { "+", "-", "*", "/" };
                string[] applicatableCharacters = { "+", "-", "*", "/", "(", ")" };

                bool lastCharacterOperator = false;
                bool currentCharacterOperator = false;
                for (int i = 0; i < input.Length; i++)
                {
                    string character = input[i].ToString();

                    currentCharacterOperator = operators.Contains(character) ? true : false;

                    if (!applicatableCharacters.Contains(character) && !int.TryParse(character, out _))
                    {
                        Console.WriteLine("Invalid expression, not applicable character: " + character);
                        return false;
                    }

                    if (currentCharacterOperator && lastCharacterOperator)
                    {
                        Console.WriteLine("Invalid expression, not allowed two or more consecutive operators: " + input[i - 1] + character);
                        return false;
                    }

                    lastCharacterOperator = currentCharacterOperator;
                }

                return true;
            }

            static string RemoveSpaces(string input)
            {
                string output = "";

                foreach (char s in input)
                    if (s != ' ')
                        output += s;

                return output;
            }

            static List<string> Tokenize(string expression)
            {
                List<string> tokens = new List<string>();
                string number = "";

                foreach (char c in expression)
                {
                    if (char.IsDigit(c))
                        number += c;
                    else
                    {
                        if (number != "")
                        {
                            tokens.Add(number);
                            number = "";
                        }
                        tokens.Add(c.ToString());
                    }
                }
                if (number != "")
                    tokens.Add(number);

                return tokens;
            }

            static List<string> InfixToPostfix(List<string> tokens)
            {
                Dictionary<string, int> hierarchy = new Dictionary<string, int>()
                {
                    { "+", 1 }, { "-", 1 }, { "*", 2 }, { "/", 2 }
                };

                Stack<string> stack = new Stack<string>();
                List<string> output = new List<string>();

                foreach (string token in tokens)
                {
                    bool isNumerical = int.TryParse(token, out int t);

                    if (isNumerical)
                        output.Add(token);
                    else if (token == "(")
                        stack.Push(token);
                    else if (token == ")")
                    {
                        while (stack.Count > 0 && stack.Peek() != "(")
                            output.Add(stack.Pop());

                        if (stack.Count == 0)
                            throw new InvalidOperationException("Mismatched parentheses: No opening parenthesis found.");

                        stack.Pop();
                    }
                    else
                    {
                        while (stack.Count > 0 &&
                               stack.Peek() != "(" &&
                               hierarchy[stack.Peek()] >= hierarchy[token])
                        {
                            output.Add(stack.Pop());
                        }
                        stack.Push(token);
                    }
                }

                while (stack.Count > 0)
                {
                    string top = stack.Pop();
                    if (top == "(")
                    {
                        throw new InvalidOperationException("Mismatched parentheses: No closing parenthesis found.");
                    }
                    output.Add(top);
                }

                return output;
            }

            static int? EvaluatePostfix(List<string> postfix)
            {
                Stack<int> stack = new Stack<int>();

                foreach (string token in postfix)
                {
                    if (int.TryParse(token, out int number))
                    {
                        stack.Push(number);
                    }
                    else
                    {
                        if (stack.Count < 2)
                        {
                            Console.WriteLine("Insufficient operands for operator: " + token);

                            throw new InvalidOperationException("Invalid postfix expression");
                        }

                        double b = stack.Pop();
                        double a = stack.Pop();

                        double t;
                        switch (token)
                        {
                            case "/":
                                t = a / b;
                                break;
                            case "*":
                                t = a * b;
                                break;
                            case "+":
                                t = a + b;
                                break;
                            case "-":
                                t = a - b;
                                break;
                            default:
                                Console.WriteLine("Invalid operator: " + token);
                                throw new InvalidOperationException("Invalid operator");
                        }

                        if (t % 1 == 0)
                            stack.Push(((int)t));
                        else
                        {
                            Console.WriteLine("Invalid expression, all operations must result in integers");
                            return null;
                        }
                    }
                }

                if (stack.Count != 1)
                    throw new InvalidOperationException("Invalid postfix expression");

                return stack.Pop();
            }

            static bool CheckApplicableNumbers(List<string> tokens, List<int> guessNumbers)
            {
                List<int> numbersInA = new List<int>();
                List<int> numbersInB = new List<int>(guessNumbers);
                List<int> removedNums = new List<int>();

                foreach (var item in tokens)
                {
                    if (int.TryParse(item, out int num))
                        numbersInA.Add(num);
                }

                foreach (int num in numbersInA)
                {
                    if (!numbersInB.Contains(num))
                    {
                        Console.Write("Invalid Expression, you may not use the number {0}", num);

                        if(removedNums.Contains(num))
                            Console.WriteLine(" more than {0} time(s).", removedNums.Count(x => x == num));

                        return false;
                    }
                    numbersInB.Remove(num);
                    removedNums.Add(num);
                }

                return true;
            }
        }
    }
}