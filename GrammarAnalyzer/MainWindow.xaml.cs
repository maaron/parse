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

            var doc = inputTextBox.Document;
            var range = new TextRange(doc.ContentStart, doc.ContentEnd)
                .Text = Properties.Settings.Default.InputText;
#if false
            var editor = new TextEditor();
            editor.DataContext = new { 
                ViewportHeight = 50, ViewportWidth = 100,
                VerticalOffset = 10, HorizontalOffset = 50
            };
#else
            var editor = new TextEditor2();
            editor.DataContext = new
            {
                Lines = new[] { "asdf", "qwer", "zxcv" }.Select(l => new { Text = l })
            };
#endif
            var window = new Window()
            {
                Width = 300,
                Height = 300
            };
            window.Content = editor;
            window.Show();
        }

        private void analyzeButton_Click(object sender, RoutedEventArgs args)
        {
            var result = EBNF.Ebnf.syntax(new Parse.ParseInput<char>(ebnfTextBox.Text));
            if (result.IsSuccess)
            {
                try
                {
                    var range = new TextRange(
                        inputTextBox.Document.ContentStart,
                        inputTextBox.Document.ContentEnd);

                    var analysis = EBNF.Ebnf.ParseRule(ruleTextBox.Text, result.Success.Value,
                        new Parse.ParseInput<char>(range.Text));

                    treeView.ItemsSource = new[] { analysis };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void inputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var doc = inputTextBox.Document;
            Properties.Settings.Default.InputText = new TextRange(
                doc.ContentStart, doc.ContentEnd).Text;
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

            var all = new TextRange(doc.ContentStart, doc.ContentEnd);
            all.ClearAllProperties();

            if (analysis.IsMatch && !analysis.End.Equals(analysis.Start))
            {
                var start = (ParseInput<char>)analysis.Start;
                var end = (ParseInput<char>)analysis.End;
#if false
                var chars = from p in inputTextBox.Document.Blocks.OfType<Paragraph>()
                            from run in p.Inlines.OfType<Run>()
                            from ptr in For(run.ContentStart, t => t.)
                            select run.ContentStart.GetPositionAtOffset(i);
#endif
                var chars = Generate(
                    doc.ContentStart.GetInsertionPosition(LogicalDirection.Forward),
                    ptr => ptr.GetNextInsertionPosition(LogicalDirection.Forward))
                    .TakeWhile(ptr => ptr != null);

                var range = chars.Where((ptr, index) => index == start.Position || index == end.Position).ToArray();
                new TextRange(range[0], range[1]).ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.LightBlue));
            }
        }
    }
}
