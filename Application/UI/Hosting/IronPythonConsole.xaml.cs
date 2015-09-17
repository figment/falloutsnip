using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;

namespace FalloutSnip.UI.Hosting
{
    /// <summary>
    ///     Interaction logic for IronPythonConsole.xaml
    /// </summary>
    public partial class IronPythonConsole : UserControl
    {
        private ConsoleOptions consoleOptionsProvider;
        // this is the name of the file currently being edited in the pad
        private string currentFileName;

        public IronPythonConsole()
        {
            Initialized += MainWindow_Initialized;

            IHighlightingDefinition pythonHighlighting;

            using (var s = new MemoryStream(Properties.Resources.Python))
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    pythonHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Python Highlighting", new[] {".cool"}, pythonHighlighting);

            InitializeComponent();
            textEditor.SyntaxHighlighting = pythonHighlighting;
            textEditor.PreviewKeyDown += textEditor_PreviewKeyDown;
            consoleOptionsProvider = new ConsoleOptions(consoleControl.Pad);
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            //propertyGridComboBox.SelectedIndex = 1;
        }

        private void TextEditor_Initialized(object sender, EventArgs e)
        {
        }

        private void IronPythonConsoleControl_Initialized(object sender, EventArgs e)
        {
        }

        private void textEditor_TextInput(object sender, TextCompositionEventArgs e)
        {
        }

        private void openFileClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            if (dlg.ShowDialog() ?? false)
            {
                currentFileName = dlg.FileName;
                textEditor.Load(currentFileName);
                textEditor.SyntaxHighlighting =
                    HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFileName));
            }
        }

        private void saveFileClick(object sender, RoutedEventArgs e)
        {
            if (currentFileName == null)
            {
                var dlg = new SaveFileDialog();
                dlg.DefaultExt = ".txt";
                if (dlg.ShowDialog() ?? false)
                {
                    currentFileName = dlg.FileName;
                }
                else
                {
                    return;
                }
            }
            textEditor.Save(currentFileName);
        }

        private void runClick(object sender, RoutedEventArgs e)
        {
            RunStatements();
        }

        private void textEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) RunStatements();
        }

        private void RunStatements()
        {
            var statementsToRun = "";
            statementsToRun = textEditor.TextArea.Selection.Length > 0 
                ? textEditor.TextArea.Selection.GetText() 
                : textEditor.TextArea.Document.Text;
            consoleControl.Pad.Console.RunStatements(statementsToRun);
        }
    }
}