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
    }

    public class QuineMcCluskeyMinimizer : BooleanFunctionMinimizer
    {
        public QuineMcCluskeyMinimizer(int numVariables, HashSet<int>? minterms, HashSet<int>? dontCares)
            : base(numVariables, minterms, dontCares)
        {
        }

        public override HashSet<string> MinimizeSOP()
        {
            int maxValue = (1 << NumVariables) - 1;
            if (Minterms.Count == maxValue + 1) // Nếu tất cả minterms được chọn
                return new HashSet<string> { "1" };

            if (!Minterms.Any())
                return new HashSet<string>();

            HashSet<int> allTerms = new HashSet<int>(Minterms);
            allTerms.UnionWith(DontCares);
            List<string> binaryMinterms = allTerms.Select(m => Convert.ToString(m, 2).PadLeft(NumVariables, '0')).ToList();
            HashSet<string> primeImplicants = FindPrimeImplicants(binaryMinterms);
            return ExtractEssentialPrimeImplicants(primeImplicants, Minterms, ConvertToVariablesSOP);
        }

        public override string MinimizePOS()
        {
            int maxValue = (1 << NumVariables) - 1;
            HashSet<int> maxterms = GetMaxtermsFromMinterms(Minterms);
            if (maxterms.Count == 0) // Nếu không còn maxterms, POS là 0
                return "0";
            if (maxterms.Count == maxValue + 1) // Nếu tất cả là maxterms, POS là 1
                return "1";

            HashSet<int> allTerms = new HashSet<int>(maxterms);
            allTerms.UnionWith(DontCares);
            List<string> binaryMaxterms = allTerms.Select(m => Convert.ToString(m, 2).PadLeft(NumVariables, '0')).ToList();
            HashSet<string> primeImplicants = FindPrimeImplicants(binaryMaxterms);
            HashSet<string> essentialPrimeImplicants = ExtractEssentialPrimeImplicants(primeImplicants, maxterms, ConvertToVariablesPOS);

            // Nối các essential prime implicants bằng tích (product of sums)
            if (essentialPrimeImplicants.Count == 0)
                return "1"; // Nếu không có implicant nào, hàm là 1

            // Dùng dấu cách thay vì "*" để biểu thị tích, thêm dấu ngoặc cho mỗi implicant
            return string.Join("", essentialPrimeImplicants.Select(implicant => $"({implicant})").ToList());
        }

        private HashSet<string> FindPrimeImplicants(List<string> binaryTerms)
        {
            HashSet<string> primeImplicants = new HashSet<string>();

            while (binaryTerms.Count > 0)
            {
                List<string> newTerms = new List<string>();
                bool[] combined = new bool[binaryTerms.Count];

                for (int i = 0; i < binaryTerms.Count; i++)
                {
                    for (int j = i + 1; j < binaryTerms.Count; j++)
                    {
                        string? combinedTerm = Combine(binaryTerms[i], binaryTerms[j]);
                        if (combinedTerm != null)
                        {
                            newTerms.Add(combinedTerm);
                            combined[i] = true;
                            combined[j] = true;
                        }
                    }
                }

                for (int i = 0; i < binaryTerms.Count; i++)
                {
                    if (!combined[i])
                    {
                        primeImplicants.Add(binaryTerms[i]);
                    }
                }

                binaryTerms = newTerms.Distinct().ToList();
            }

            return primeImplicants;
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

        private HashSet<string> ExtractEssentialPrimeImplicants(HashSet<string> primeImplicants, HashSet<int> terms, Func<string, string> convertToVariables)
        {
            if (!primeImplicants.Any() || !terms.Any())
                return new HashSet<string>();

            var chart = new Dictionary<string, HashSet<int>>();

            foreach (var implicant in primeImplicants)
            {
                chart[implicant] = new HashSet<int>();
                foreach (var term in terms)
                {
                    if (Covers(implicant, Convert.ToString(term, 2).PadLeft(NumVariables, '0')))
                    {
                        chart[implicant].Add(term);
                    }
                }
            }

            var essentialPrimeImplicants = new HashSet<string>();
            var coveredTerms = new HashSet<int>();

            foreach (var term in terms)
            {
                var coveringImplicants = chart.Where(c => c.Value.Contains(term)).Select(c => c.Key).ToList();
                if (coveringImplicants.Count == 1)
                {
                    var essentialImplicant = coveringImplicants.First();
                    if (!essentialPrimeImplicants.Contains(essentialImplicant))
                    {
                        essentialPrimeImplicants.Add(essentialImplicant);
                    }
                    coveredTerms.UnionWith(chart[essentialImplicant]);
                }
            }

            foreach (var implicant in essentialPrimeImplicants)
            {
                chart.Remove(implicant);
            }

            var remainingTerms = terms.Except(coveredTerms).ToHashSet();
            while (remainingTerms.Count > 0 && chart.Count > 0)
            {
                var bestImplicantEntry = chart.OrderByDescending(c => c.Value.Count(v => remainingTerms.Contains(v))).FirstOrDefault();
                if (bestImplicantEntry.Key == null) break;

                var bestImplicant = bestImplicantEntry.Key;
                if (!essentialPrimeImplicants.Contains(bestImplicant))
                {
                    essentialPrimeImplicants.Add(bestImplicant);
                }
                coveredTerms.UnionWith(chart[bestImplicant]);
                remainingTerms = terms.Except(coveredTerms).ToHashSet();
                chart.Remove(bestImplicant);
            }

            return new HashSet<string>(essentialPrimeImplicants.Select(convertToVariables));
        }

        private bool Covers(string implicant, string term)
        {
            for (int i = 0; i < implicant.Length; i++)
            {
                if (implicant[i] != '-' && implicant[i] != term[i])
                    return false;
            }
            return true;
        }

        private string ConvertToVariablesSOP(string term)
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

        private string ConvertToVariablesPOS(string term)
        {
            if (string.IsNullOrEmpty(term))
                return string.Empty;

            HashSet<string> variables = GenerateVariableNames(NumVariables);
            List<string> result = new List<string>();

            for (int i = 0; i < term.Length; i++)
            {
                if (term[i] == '0')
                    result.Add(variables.ElementAt(i));
                else if (term[i] == '1')
                    result.Add(variables.ElementAt(i) + "'");
            }

            return string.Join("+", result); // Trong một implicant POS, các biến được nối bằng "+"
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

    
}