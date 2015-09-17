using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using Microsoft.Scripting;
using System.Threading;
using PythonConsoleControl;

namespace FalloutSnip.UI.Hosting
{
    /// <summary>
    /// Interaction logic for IronPythonConsole.xaml
    /// </summary>
    public partial class IronPythonConsole : UserControl
    {
        private FalloutSnip.UI.Hosting.ConsoleOptions consoleOptionsProvider;
        // this is the name of the file currently being edited in the pad
        private string currentFileName;

        public IronPythonConsole()
        {

            Initialized += new EventHandler(MainWindow_Initialized);

            IHighlightingDefinition pythonHighlighting;

            using (var s = new System.IO.MemoryStream(Properties.Resources.Python))
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting("Python Highlighting", new string[] { ".cool" }, pythonHighlighting);

            InitializeComponent();
            textEditor.SyntaxHighlighting = pythonHighlighting;
            textEditor.PreviewKeyDown += new KeyEventHandler(textEditor_PreviewKeyDown);
            consoleOptionsProvider = new FalloutSnip.UI.Hosting.ConsoleOptions(consoleControl.Pad);   
        }

        void MainWindow_Initialized(object sender, EventArgs e)
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
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(currentFileName));
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

        void textEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) RunStatements();
        }

        void RunStatements()
        {
            string statementsToRun = "";
            if (textEditor.TextArea.Selection.Length > 0)
                statementsToRun = textEditor.TextArea.Selection.GetText(textEditor.TextArea.Document);
            else
                statementsToRun = textEditor.TextArea.Document.Text;
            consoleControl.Pad.Console.RunStatements(statementsToRun);
        }         
    }
}
