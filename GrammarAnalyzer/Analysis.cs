using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using Parse;

namespace GrammarAnalyzer
{
    public static class Extensions
    {
        public static Parser<T> Analyzed<T>(
            this Parser<T> parser,
            string name, 
            AnalysisBuilder<T> analysis)
        {
            return input =>
            {
                analysis.PushFrame(name, input);
                var result = parser(input);
                analysis.PopFrame(result);
                return result;
            };
        }
    }

    public class Analysis<T> : INotifyPropertyChanged
    {
        public string RuleName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool isMatch;
        public bool IsMatch
        {
            get { return isMatch; }
            set { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsMatch")); isMatch = value; }
        }
        public IParseInput<T> Start;
        public IParseInput<T> End;
        public List<Analysis<T>> SubMatches { get; private set; }

        public Analysis(string ruleName, IParseInput<T> input)
        {
            SubMatches = new List<Analysis<T>>();
            Start = input;
            RuleName = ruleName;
        }
    }

    public class AnalysisBuilder<T>
    {
        private Stack<Analysis<T>> stack = new Stack<Analysis<T>>();
        private Analysis<T> root;

        public void PushFrame(string ruleName, IParseInput<T> input)
        {
            var a = new Analysis<T>(ruleName, input);
            if (stack.Count > 0) stack.Peek().SubMatches.Add(a);
            else root = a;
            stack.Push(a);
        }

        public void PopFrame(Result<T> result)
        {
            var frame = stack.Pop();
            frame.IsMatch = result.IsSuccess;
            frame.End = result.Remaining;
        }

        public Analysis<T> ToAnalysis()
        {
            return root;
        }
    }
}
