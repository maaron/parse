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

using Parse;

namespace GrammarAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            inputTextBox.Text = Properties.Settings.Default.InputText;
            grammarEditor.Text = Properties.Settings.Default.GrammarText;
            var snapshot = grammarEditor.Document.CreateSnapshot();
        }

        private void analyzeButton_Click(object sender, RoutedEventArgs args)
        {
            var input = new Parse.ParseInput<char>(grammarEditor.Text);

            try
            {
                if (((string)(formatComboBox.SelectedValue as System.Windows.Controls.ComboBoxItem).Content) == "Augmented BNF")
                {
                    var result = Abnf.syntax(input);

                    if (result.IsSuccess)
                    {
                        var analysis = Abnf.ParseRule(ruleTextBox.Text, result.Success.Value,
                            new Parse.ParseInput<char>(inputTextBox.Text));

                        treeView.ItemsSource = new[] { analysis };
                    }
                }
                else
                {
                    var result = EBNF.Ebnf.syntax(input);

                    if (result.IsSuccess)
                    {
                        var analysis = EBNF.Ebnf.ParseRule(ruleTextBox.Text, result.Success.Value,
                            new Parse.ParseInput<char>(inputTextBox.Text));

                        treeView.ItemsSource = new[] { analysis };
                    }
                }
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
            Properties.Settings.Default.GrammarText = editor.Text;
        }
    }
}
