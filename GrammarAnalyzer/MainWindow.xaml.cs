using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Functional;
using Parse;
using Parse.CharCombinators;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace GrammarAnalyzer
{
    public class CompletionData : ICompletionData
    {
        private string data;
        public CompletionData(string data)
        {
            this.data = data;
        }

        public object Content
        {
            get
            {
                return data;
            }
        }

        public object Description
        {
            get
            {
                return data;
            }
        }

        public ImageSource Image
        {
            get
            {
                return null;
            }
        }

        public double Priority
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Text
        {
            get
            {
                return data;
            }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, data);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum GrammarFormats {Abnf, Ebnf};

        private static Dictionary<GrammarFormats, string> grammarFormats = new Dictionary<GrammarFormats, string>()
        {
            {GrammarFormats.Abnf, "Augmented BNF"},
            {GrammarFormats.Ebnf, "Extended BNF"}
        };
        public Dictionary<GrammarFormats, string> Formats { get { return grammarFormats; } }

        private GrammarFormats grammarFormat = GrammarFormats.Abnf;
        public GrammarFormats GrammarFormat { get { return grammarFormat; } set { grammarFormat = value; } }

        private FList<Abnf.Rule> abnfRules = null;

        public MainWindow()
        {
            InitializeComponent();

            grammarEditor.TextArea.PreviewKeyDown += grammarEditor_PreviewKeyDown;

            inputTextBox.Text = Properties.Settings.Default.InputText;
            grammarEditor.Text = Properties.Settings.Default.GrammarText;
            var snapshot = grammarEditor.Document.CreateSnapshot();
            DataContext = this;
        }

        private void analyzeButton_Click(object sender, RoutedEventArgs args)
        {
            var input = new Parse.ParseInput<char>(grammarEditor.Text);

            try
            {
                if (grammarFormat == GrammarFormats.Abnf)
                {
                    var result = Abnf.syntax(input);

                    if (result.IsSuccess)
                    {
                        var analysis = Abnf.ParseRule(ruleTextBox.Text, result.Success.Value,
                            new Parse.ParseInput<char>(inputTextBox.Text));

                        treeView.ItemsSource = new[] { analysis };
                    }
                    else treeView.ItemsSource = null;
                }
                else if (grammarFormat == GrammarFormats.Ebnf)
                {
                    var result = EBNF.Ebnf.syntax(input);

                    if (result.IsSuccess)
                    {
                        var analysis = EBNF.Ebnf.ParseRule(ruleTextBox.Text, result.Success.Value,
                            new Parse.ParseInput<char>(inputTextBox.Text));

                        treeView.ItemsSource = new[] { analysis };
                    }
                    else treeView.ItemsSource = null;
                }
                else throw new Exception("Unsupported grammar format " + grammarFormat);
            }
            catch (Exception e)
            {
                treeView.ItemsSource = null;
                Console.WriteLine(e);
            }
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            var editor = (ICSharpCode.AvalonEdit.TextEditor)sender;
            Properties.Settings.Default.InputText = editor.Text;
        }

        private static IEnumerable<T> Generate<T>(T start, Func<T, T> increment)
        {
            while (true)
            {
                yield return start;
                start = increment(start);
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var analysis = (Analysis<char>)e.NewValue;
            var doc = inputTextBox.Document;

            inputTextBox.Select(0, 0);

            if (analysis != null && analysis.IsMatch && !analysis.End.Equals(analysis.Start))
            {
                var start = (ParseInput<char>)analysis.Start;
                var end = (ParseInput<char>)analysis.End;

                inputTextBox.Select(start.Position, end.Position - start.Position );
            }
        }

        private void grammarEditor_TextChanged(object sender, EventArgs e)
        {
            var editor = (ICSharpCode.AvalonEdit.TextEditor)sender;
            var text = editor.Text;
            Properties.Settings.Default.GrammarText = editor.Text;

            if (grammarFormat == GrammarFormats.Abnf)
            {
                var result = Abnf.syntax(new StringInput(text));

                if (result.IsSuccess)
                {
                    abnfRules = result.Success.Value;
                    UpdateCompletionWindow();
                }
            }
        }

        private void UpdateCompletionWindow()
        {
            if (completionWindow != null)
            {
                completionWindow.CompletionList.CompletionData.Clear();
                foreach (var rule in abnfRules)
                {
                    completionWindow.CompletionList.CompletionData.Add(
                        new CompletionData(rule.Name.Value));
                }
            }
        }

        CompletionWindow completionWindow = null;
        private void grammarEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space 
                && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                completionWindow = new CompletionWindow(grammarEditor.TextArea);
                completionWindow.Show();
                UpdateCompletionWindow();
                completionWindow.Closed += (s, a) =>
                {
                    completionWindow = null;
                };
                e.Handled = true;
            }
            else if (completionWindow != null
                && e.Key == Key.Tab)
            {
                e.Handled = true;
                completionWindow.CompletionList.RequestInsertion(e);
            }
        }
    }
}
