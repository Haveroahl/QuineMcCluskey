using System;
using System.Collections.Generic;
using System.Linq;

namespace QuineMcCluskey
{
    public interface IBooleanFunctionMinimizer
    {
        HashSet<string> MinimizeSOP();
        string MinimizePOS();
    }

    public interface IInputStrategy
    {
        HashSet<int> GetMinterms(int numVariables, HashSet<int> dontCares);
    }

    public abstract class BooleanFunctionMinimizer : IBooleanFunctionMinimizer
    {
        protected int NumVariables { get; }
        protected HashSet<int> Minterms { get; }
        protected HashSet<int> DontCares { get; }

        protected BooleanFunctionMinimizer(int numVariables, HashSet<int>? minterms, HashSet<int>? dontCares)
        {
            if (numVariables < 1 || numVariables > 31)
                throw new ArgumentException("Số lượng biến phải từ 1 đến 31.");
            NumVariables = numVariables;
            Minterms = minterms ?? new HashSet<int>();
            DontCares = dontCares ?? new HashSet<int>();
        }

        public abstract HashSet<string> MinimizeSOP();
        public abstract string MinimizePOS();

        protected string ConvertToVariables(string term)
        {
            if (string.IsNullOrEmpty(term))
                return string.Empty;

            HashSet<string> variables = GenerateVariableNames(NumVariables);
            List<string> result = new List<string>();

            for (int i = 0; i < term.Length; i++)
            {
                if (term[i] == '1')
                    result.Add(variables.ElementAt(i));
                else if (term[i] == '0')
                    result.Add(variables.ElementAt(i) + "'");
            }

            return string.Join("", result);
        }

        protected HashSet<string> GenerateVariableNames(int numVariables)
        {
            HashSet<string> variables = new HashSet<string>();
            for (int i = 0; i < numVariables; i++)
            {
                variables.Add(((char)('A' + i)).ToString());
            }
            return variables;
        }

        protected HashSet<int> GetMaxtermsFromMinterms(HashSet<int> minterms)
        {
            int maxValue = (1 << NumVariables) - 1;
            HashSet<int> maxterms = new HashSet<int>();

            for (int i = 0; i <= maxValue; i++)
            {
                if (!minterms.Contains(i) && !DontCares.Contains(i))
                    maxterms.Add(i);
            }

            return maxterms;
        }

        public HashSet<int> GetMintermsFromMaxterms(HashSet<int> maxterms)
        {
            int maxValue = (1 << NumVariables) - 1;
            HashSet<int> minterms = new HashSet<int>();

            for (int i = 0; i <= maxValue; i++)
            {
                if (!maxterms.Contains(i) && !DontCares.Contains(i))
                    minterms.Add(i);
            }

            return minterms;
        }

        protected string ConvertMintermsToPOS(HashSet<int> minterms)
        {
            HashSet<string> posTerms = new HashSet<string>();
            var variables = GenerateVariableNames(NumVariables);

            foreach (var minterm in minterms)
            {
                string binary = Convert.ToString(minterm, 2).PadLeft(NumVariables, '0');
                HashSet<string> posTerm = new HashSet<string>();

                for (int i = 0; i < binary.Length; i++)
                {
                    posTerm.Add(binary[i] == '0' ? variables.ElementAt(i) : variables.ElementAt(i) + "'");
                }

                posTerms.Add("(" + string.Join("+", posTerm) + ")");
            }

            return string.Join("", posTerms);
        }

        protected string SimplifyPOS(string posExpression)
        {
            if (string.IsNullOrEmpty(posExpression))
                return "";

            var posTerms = posExpression.Split(new[] { ")(" }, StringSplitOptions.None)
                                        .Select(term => term.Trim('(', ')'))
                                        .Select(term => new HashSet<string>(term.Split('+')))
                                        .ToList();

            var minimizedPos = MinimizePosTerms(posTerms);
            return string.Join("", minimizedPos.Select(term => "(" + string.Join("+", term) + ")"));
        }

        private List<HashSet<string>> MinimizePosTerms(List<HashSet<string>> posTerms)
        {
            List<HashSet<string>> result = new List<HashSet<string>>(posTerms);

            bool simplified;
            do
            {
                simplified = false;
                for (int i = 0; i < result.Count - 1; i++)
                {
                    for (int j = i + 1; j < result.Count; j++)
                    {
                        HashSet<string>? combined = CombineTerms(result[i], result[j]);
                        if (combined != null && !result.Any(t => t.SetEquals(combined)))
                        {
                            result.RemoveAt(j);
                            result.RemoveAt(i);
                            result.Add(combined);
                            simplified = true;
                            break;
                        }
                    }
                    if (simplified) break;
                }
            } while (simplified);

            return result;
        }

        private HashSet<string>? CombineTerms(HashSet<string> term1, HashSet<string> term2)
        {
            var differences = term1.Where(x => !term2.Contains(x)).Union(term2.Where(x => !term1.Contains(x))).ToList();

            if (differences.Count == 2 && IsNegation(differences[0], differences[1]))
            {
                var combined = new HashSet<string>(term1);
                combined.IntersectWith(term2);
                return combined;
            }
            return null;
        }

        private bool IsNegation(string var1, string var2)
        {
            return var1 == var2 + "'" || var2 == var1 + "'";
        }

        protected HashSet<int> GetMintermsFromSOP(HashSet<string> sopTerms)
        {
            HashSet<int> minterms = new HashSet<int>();
            if (sopTerms == null || !sopTerms.Any())
                return minterms;

            var variables = GenerateVariableNames(NumVariables);

            foreach (var term in sopTerms)
            {
                string binary = new string(variables
                    .Select(v => term.Contains(v + "'") ? '0' : term.Contains(v) ? '1' : '-')
                    .ToArray());

                if (binary.Contains("-"))
                {
                    int dashCount = binary.Count(c => c == '-');
                    for (int i = 0; i < (1 << dashCount); i++)
                    {
                        string tempBinary = binary;
                        int dashIndex = 0;
                        for (int j = 0; j < tempBinary.Length; j++)
                        {
                            if (tempBinary[j] == '-')
                            {
                                tempBinary = tempBinary.Substring(0, j) + ((i >> dashIndex) & 1) + tempBinary.Substring(j + 1);
                                dashIndex++;
                            }
                        }
                        minterms.Add(Convert.ToInt32(tempBinary, 2));
                    }
                }
                else
                {
                    minterms.Add(Convert.ToInt32(binary, 2));
                }
            }

            return minterms;
        }
    }

    public class QuineMcCluskeyMinimizer : BooleanFunctionMinimizer
    {
        public QuineMcCluskeyMinimizer(int numVariables, HashSet<int>? minterms, HashSet<int>? dontCares)
            : base(numVariables, minterms, dontCares)
        {
        }

        public override HashSet<string> MinimizeSOP()
        {
            if (!Minterms.Any())
                return new HashSet<string>();

            HashSet<int> allTerms = new HashSet<int>(Minterms);
            allTerms.UnionWith(DontCares);
            List<string> binaryMinterms = allTerms.Select(m => Convert.ToString(m, 2).PadLeft(NumVariables, '0')).ToList();
            HashSet<string> primeImplicants = new HashSet<string>();

            while (binaryMinterms.Count > 0)
            {
                List<string> newMinterms = new List<string>();
                bool[] combined = new bool[binaryMinterms.Count];

                for (int i = 0; i < binaryMinterms.Count; i++)
                {
                    for (int j = i + 1; j < binaryMinterms.Count; j++)
                    {
                        string? combinedTerm = Combine(binaryMinterms[i], binaryMinterms[j]);
                        if (combinedTerm != null)
                        {
                            newMinterms.Add(combinedTerm);
                            combined[i] = true;
                            combined[j] = true;
                        }
                    }
                }

                for (int i = 0; i < binaryMinterms.Count; i++)
                {
                    if (!combined[i])
                    {
                        primeImplicants.Add(binaryMinterms[i]);
                    }
                }

                binaryMinterms = newMinterms.Distinct().ToList();
            }

            return ExtractEssentialPrimeImplicants(primeImplicants, Minterms);
        }

        private string? Combine(string a, string b)
        {
            int diffCount = 0;
            char[] result = new char[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    diffCount++;
                    result[i] = '-';
                }
                else
                {
                    result[i] = a[i];
                }

                if (diffCount > 1)
                    return null;
            }

            return new string(result);
        }

        private HashSet<string> ExtractEssentialPrimeImplicants(HashSet<string> primeImplicants, HashSet<int> minterms)
        {
            if (!primeImplicants.Any() || !minterms.Any())
                return new HashSet<string>();

            var chart = new Dictionary<string, HashSet<int>>();

            foreach (var implicant in primeImplicants)
            {
                chart[implicant] = new HashSet<int>();
                foreach (var minterm in minterms)
                {
                    if (Covers(implicant, Convert.ToString(minterm, 2).PadLeft(NumVariables, '0')))
                    {
                        chart[implicant].Add(minterm);
                    }
                }
            }

            var essentialPrimeImplicants = new HashSet<string>();
            var coveredMinterms = new HashSet<int>();

            foreach (var minterm in minterms)
            {
                var coveringImplicants = chart.Where(c => c.Value.Contains(minterm)).Select(c => c.Key).ToList();
                if (coveringImplicants.Count == 1)
                {
                    var essentialImplicant = coveringImplicants.First();
                    if (!essentialPrimeImplicants.Contains(essentialImplicant))
                    {
                        essentialPrimeImplicants.Add(essentialImplicant);
                    }
                    coveredMinterms.UnionWith(chart[essentialImplicant]);
                }
            }

            foreach (var implicant in essentialPrimeImplicants)
            {
                chart.Remove(implicant);
            }

            var remainingMinterms = minterms.Except(coveredMinterms).ToHashSet();
            while (remainingMinterms.Count > 0 && chart.Count > 0)
            {
                var bestImplicantEntry = chart.OrderByDescending(c => c.Value.Count(v => remainingMinterms.Contains(v))).FirstOrDefault();
                if (bestImplicantEntry.Key == null) break;

                var bestImplicant = bestImplicantEntry.Key;
                if (!essentialPrimeImplicants.Contains(bestImplicant))
                {
                    essentialPrimeImplicants.Add(bestImplicant);
                }
                coveredMinterms.UnionWith(chart[bestImplicant]);
                remainingMinterms = minterms.Except(coveredMinterms).ToHashSet();
                chart.Remove(bestImplicant);
            }

            return new HashSet<string>(essentialPrimeImplicants.Select(ConvertToVariables));
        }

        private bool Covers(string implicant, string minterm)
        {
            for (int i = 0; i < implicant.Length; i++)
            {
                if (implicant[i] != '-' && implicant[i] != minterm[i])
                    return false;
            }
            return true;
        }

        public override string MinimizePOS()
        {
            HashSet<string> sopTerms = MinimizeSOP();
            if (!sopTerms.Any())
                return "";

            HashSet<int> sopMinterms = GetMintermsFromSOP(sopTerms);
            HashSet<int> maxterms = GetMaxtermsFromMinterms(sopMinterms);
            string posStandard = ConvertMintermsToPOS(maxterms);
            return SimplifyPOS(posStandard);
        }
    }

    public class MintermInputStrategy : IInputStrategy
    {
        public HashSet<int> GetMinterms(int numVariables, HashSet<int> dontCares)
        {
            Console.WriteLine("Nhập các Minterms (dấu cách):");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return new HashSet<int>();

            string[] mintermsInput = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            HashSet<int> minterms = new HashSet<int>(mintermsInput.Select(int.Parse));
            ValidateInput(numVariables, minterms, dontCares);
            return minterms;
        }

        private void ValidateInput(int numVariables, HashSet<int> minterms, HashSet<int> dontCares)
        {
            int maxValue = (1 << numVariables) - 1;
            foreach (var term in minterms)
            {
                if (term < 0 || term > maxValue)
                    throw new ArgumentException($"Minterm {term} không hợp lệ với {numVariables} biến.");
            }
        }
    }

    public class MaxtermInputStrategy : IInputStrategy
    {
        private readonly BooleanFunctionMinimizer minimizer;

        public MaxtermInputStrategy(int numVariables)
        {
            minimizer = new QuineMcCluskeyMinimizer(numVariables, null, null);
        }

        public HashSet<int> GetMinterms(int numVariables, HashSet<int> dontCares)
        {
            Console.WriteLine("Nhập các Maxterms (dấu cách):");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return new HashSet<int>();

            string[] maxtermsInput = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            HashSet<int> maxterms = new HashSet<int>(maxtermsInput.Select(int.Parse));
            ValidateInput(numVariables, maxterms, dontCares);
            return minimizer.GetMintermsFromMaxterms(maxterms);
        }

        private void ValidateInput(int numVariables, HashSet<int> maxterms, HashSet<int> dontCares)
        {
            int maxValue = (1 << numVariables) - 1;
            foreach (var term in maxterms)
            {
                if (term < 0 || term > maxValue)
                    throw new ArgumentException($"Maxterm {term} không hợp lệ với {numVariables} biến.");
            }
        }
    }

    public static class MinimizerFactory
    {
        public static IBooleanFunctionMinimizer CreateMinimizer(int numVariables, IInputStrategy inputStrategy, HashSet<int> dontCares)
        {
            HashSet<int> minterms = inputStrategy.GetMinterms(numVariables, dontCares);
            return new QuineMcCluskeyMinimizer(numVariables, minterms, dontCares);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            try
            {
                Console.WriteLine("Nhập số lượng biến (1-31):");
                string? numVariablesInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(numVariablesInput))
                    throw new ArgumentException("Số lượng biến không được để trống.");
                int numVariables = int.Parse(numVariablesInput);

                Console.WriteLine("Chọn loại đầu vào (1: Minterms, 2: Maxterms):");
                string? choiceInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(choiceInput))
                    throw new ArgumentException("Lựa chọn không được để trống.");
                int choice = int.Parse(choiceInput);

                IInputStrategy inputStrategy;
                switch (choice)
                {
                    case 1:
                        inputStrategy = new MintermInputStrategy();
                        break;
                    case 2:
                        inputStrategy = new MaxtermInputStrategy(numVariables);
                        break;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ!");
                        return;
                }

                Console.WriteLine("Nhập các Don't care (dấu cách or Enter nếu rỗng):");
                string? dontCareInput = Console.ReadLine();
                HashSet<int> dontCares = string.IsNullOrWhiteSpace(dontCareInput)
                    ? new HashSet<int>()
                    : new HashSet<int>(dontCareInput.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse));

                int maxValue = (1 << numVariables) - 1;
                foreach (var dc in dontCares)
                {
                    if (dc < 0 || dc > maxValue)
                        throw new ArgumentException($"Don't care {dc} không hợp lệ với {numVariables} biến.");
                }

                IBooleanFunctionMinimizer minimizer = MinimizerFactory.CreateMinimizer(numVariables, inputStrategy, dontCares);

                HashSet<string> minimizedSOP = minimizer.MinimizeSOP() ?? new HashSet<string>();
                Console.WriteLine("Hàm SOP tối giản:");
                Console.WriteLine("Y = " + (minimizedSOP.Any() ? string.Join(" + ", minimizedSOP) : "0"));

                string minimizedPOS = minimizer.MinimizePOS() ?? "";
                Console.WriteLine("Hàm POS tối giản:");
                Console.WriteLine(minimizedPOS.Length > 0 ? minimizedPOS : "1");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Đã xảy ra lỗi: {ex.Message}");
            }
        }
    }
}
