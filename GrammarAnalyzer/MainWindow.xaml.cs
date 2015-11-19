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

namespace GrammarAnalyzer
{
    public class BackgroundConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value ? new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xB2, 0xB2))
                : new SolidColorBrush(Color.FromArgb(0xFF, 0xC6, 0xFB, 0xB2));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void analyzeButton_Click(object sender, RoutedEventArgs args)
        {
            var result = EBNF.Ebnf.syntax(new Parse.ParseInput<char>(ebnfTextBox.Text));
            if (result.IsSuccess)
            {
                try
                {
                    var analysis = EBNF.Ebnf.ParseRule(ruleTextBox.Text, result.Success.Value,
                        new Parse.ParseInput<char>(inputTextBox.Text));

                    treeView.ItemsSource = new[] { analysis };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
