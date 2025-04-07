using System;
using System.Collections.Generic;
using System.Linq;

namespace QuineMcCluskey
{
    public interface IBooleanFunctionMinimizer
    {
        string Minimize(); // Chỉ trả về một biểu thức tùy theo loại đầu vào
    }

    public interface IInputStrategy
    {
        HashSet<int> GetTerms(int numVariables, HashSet<int> dontCares);
    }

    public abstract class BooleanFunctionMinimizer : IBooleanFunctionMinimizer
    {
        protected int NumVariables { get; }
        protected HashSet<int> Terms { get; } // Có thể là minterms hoặc maxterms
        protected HashSet<int> DontCares { get; }

        protected BooleanFunctionMinimizer(int numVariables, HashSet<int>? terms, HashSet<int>? dontCares)
        {
            if (numVariables < 1 || numVariables > 31)
                throw new ArgumentException("Số lượng biến phải từ 1 đến 31.");
            NumVariables = numVariables;
            Terms = terms ?? new HashSet<int>();
            DontCares = dontCares ?? new HashSet<int>();
        }

        public abstract string Minimize();

        protected HashSet<string> GenerateVariableNames(int numVariables)
        {
            HashSet<string> variables = new HashSet<string>();
            for (int i = 0; i < numVariables; i++)
            {
                variables.Add(((char)('A' + i)).ToString());
            }
            return variables;
        }
    }

    public class QuineMcCluskeyMinimizer : BooleanFunctionMinimizer
    {
        private readonly bool isMintermInput; // Xác định loại đầu vào: true = minterms, false = maxterms

        public QuineMcCluskeyMinimizer(int numVariables, HashSet<int>? terms, HashSet<int>? dontCares, bool isMintermInput)
            : base(numVariables, terms, dontCares)
        {
            this.isMintermInput = isMintermInput;
        }

        public override string Minimize()
        {
            return isMintermInput ? MinimizeSOP() : MinimizePOS();
        }

        private string MinimizeSOP()
        {
            int maxValue = (1 << NumVariables) - 1;
            if (Terms.Count == maxValue + 1) // Nếu tất cả minterms được chọn
                return "1";

            if (!Terms.Any())
                return "0";

            HashSet<int> allTerms = new HashSet<int>(Terms); // Minterms
            allTerms.UnionWith(DontCares);
            List<string> binaryTerms = allTerms.Select(m => Convert.ToString(m, 2).PadLeft(NumVariables, '0')).ToList();
            HashSet<string> primeImplicants = FindPrimeImplicants(binaryTerms);
            HashSet<string> essentialPrimeImplicants = ExtractEssentialPrimeImplicants(primeImplicants, Terms, ConvertToVariablesSOP);

            return essentialPrimeImplicants.Any() ? string.Join(" + ", essentialPrimeImplicants) : "0";
        }

        private string MinimizePOS()
        {
            int maxValue = (1 << NumVariables) - 1;
            if (Terms.Count == maxValue + 1) // Nếu tất cả maxterms được chọn
                return "0";

            if (!Terms.Any())
                return "1";

            HashSet<int> allTerms = new HashSet<int>(Terms); // Maxterms
            allTerms.UnionWith(DontCares);
            List<string> binaryTerms = allTerms.Select(m => Convert.ToString(m, 2).PadLeft(NumVariables, '0')).ToList();
            HashSet<string> primeImplicants = FindPrimeImplicants(binaryTerms);
            HashSet<string> essentialPrimeImplicants = ExtractEssentialPrimeImplicants(primeImplicants, Terms, ConvertToVariablesPOS);

            if (!essentialPrimeImplicants.Any())
                return "1";

            return string.Join("", essentialPrimeImplicants.Select(implicant => $"({implicant})"));
        }

        private HashSet<string> FindPrimeImplicants(List<string> binaryTerms)
        {
            HashSet<string> primeImplicants = new HashSet<string>();
            Dictionary<int, List<string>> termsByOnes = new Dictionary<int, List<string>>();

            foreach (var term in binaryTerms)
            {
                int onesCount = term.Count(c => c == '1');
                if (!termsByOnes.ContainsKey(onesCount))
                    termsByOnes[onesCount] = new List<string>();
                termsByOnes[onesCount].Add(term);
            }

            HashSet<string> currentTerms = new HashSet<string>(binaryTerms);
            for (int i = 0; i < NumVariables; i++)
            {
                HashSet<string> newTerms = new HashSet<string>();
                bool[] combined = new bool[currentTerms.Count];
                int index = 0;

                foreach (var term in currentTerms)
                {
                    int onesCount = term.Count(c => c == '1');
                    if (termsByOnes.ContainsKey(onesCount + 1))
                    {
                        foreach (var otherTerm in termsByOnes[onesCount + 1])
                        {
                            string? combinedTerm = Combine(term, otherTerm);
                            if (combinedTerm != null)
                            {
                                newTerms.Add(combinedTerm);
                                combined[index] = true;
                            }
                        }
                    }
                    index++;
                }

                index = 0;
                foreach (var term in currentTerms)
                {
                    if (!combined[index])
                        primeImplicants.Add(term);
                    index++;
                }

                if (newTerms.Count == 0) break;
                currentTerms = newTerms;

                termsByOnes.Clear();
                foreach (var term in currentTerms)
                {
                    int onesCount = term.Count(c => c == '1');
                    if (!termsByOnes.ContainsKey(onesCount))
                        termsByOnes[onesCount] = new List<string>();
                    termsByOnes[onesCount].Add(term);
                }
            }

            primeImplicants.UnionWith(currentTerms);
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
                        chart[implicant].Add(term);
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
                    essentialPrimeImplicants.Add(essentialImplicant);
                    coveredTerms.UnionWith(chart[essentialImplicant]);
                }
            }

            var remainingTerms = terms.Except(coveredTerms).ToHashSet();
            while (remainingTerms.Count > 0)
            {
                var bestImplicant = chart
                    .Where(c => !essentialPrimeImplicants.Contains(c.Key))
                    .OrderByDescending(c => c.Value.Intersect(remainingTerms).Count())
                    .FirstOrDefault();

                if (bestImplicant.Key == null) break;

                essentialPrimeImplicants.Add(bestImplicant.Key);
                coveredTerms.UnionWith(bestImplicant.Value);
                remainingTerms = terms.Except(coveredTerms).ToHashSet();
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

            return string.Join("+", result);
        }
    }

    public class MintermInputStrategy : IInputStrategy
    {
        public HashSet<int> GetTerms(int numVariables, HashSet<int> dontCares)
        {
            Console.WriteLine("Nhập các Minterms (dấu cách):");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return new HashSet<int>();

            string[] termsInput = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            HashSet<int> terms = new HashSet<int>(termsInput.Select(int.Parse));
            ValidateInput(numVariables, terms, dontCares);
            return terms;
        }

        private void ValidateInput(int numVariables, HashSet<int> terms, HashSet<int> dontCares)
        {
            int maxValue = (1 << numVariables) - 1;
            foreach (var term in terms)
            {
                if (term < 0 || term > maxValue)
                    throw new ArgumentException($"Minterm {term} không hợp lệ với {numVariables} biến.");
            }
        }
    }

    public class MaxtermInputStrategy : IInputStrategy
    {
        public HashSet<int> GetTerms(int numVariables, HashSet<int> dontCares)
        {
            Console.WriteLine("Nhập các Maxterms (dấu cách):");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return new HashSet<int>();

            string[] termsInput = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            HashSet<int> terms = new HashSet<int>(termsInput.Select(int.Parse));
            ValidateInput(numVariables, terms, dontCares);
            return terms;
        }

        private void ValidateInput(int numVariables, HashSet<int> terms, HashSet<int> dontCares)
        {
            int maxValue = (1 << numVariables) - 1;
            foreach (var term in terms)
            {
                if (term < 0 || term > maxValue)
                    throw new ArgumentException($"Maxterm {term} không hợp lệ với {numVariables} biến.");
            }
        }
    }

    public static class MinimizerFactory
    {
        public static IBooleanFunctionMinimizer CreateMinimizer(int numVariables, IInputStrategy inputStrategy, HashSet<int> dontCares, bool isMintermInput)
        {
            HashSet<int> terms = inputStrategy.GetTerms(numVariables, dontCares);
            return new QuineMcCluskeyMinimizer(numVariables, terms, dontCares, isMintermInput);
        }
    }

    internal class Program1
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

                Console.WriteLine("Chọn loại đầu vào (1: Minterms -> SOP, 2: Maxterms -> POS):");
                string? choiceInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(choiceInput))
                    throw new ArgumentException("Lựa chọn không được để trống.");
                int choice = int.Parse(choiceInput);

                IInputStrategy inputStrategy;
                bool isMintermInput;
                switch (choice)
                {
                    case 1:
                        inputStrategy = new MintermInputStrategy();
                        isMintermInput = true;
                        break;
                    case 2:
                        inputStrategy = new MaxtermInputStrategy();
                        isMintermInput = false;
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

                IBooleanFunctionMinimizer minimizer = MinimizerFactory.CreateMinimizer(numVariables, inputStrategy, dontCares, isMintermInput);

                string result = minimizer.Minimize();
                Console.WriteLine(isMintermInput ? "Hàm SOP tối giản:" : "Hàm POS tối giản:");
                Console.WriteLine("Y = " + result);
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
