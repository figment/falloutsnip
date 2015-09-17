using System.Collections.Generic;
using System.Windows.Forms;
using PythonConsoleControl;
using FalloutSnip.UI.Hosting;

namespace FalloutSnip.UI.Docking
{
    using System;

    using FalloutSnip.Domain.Model;

    public partial class InterpreterConsole : BaseDockContent
    {
        private FalloutSnip.UI.Hosting.IronPythonConsole pythonConsole;
        public InterpreterConsole()
        {
            pythonConsole = new IronPythonConsole();
            this.InitializeComponent();
            this.elementHost1.Child = pythonConsole;
        }

        public IronPythonConsole InnerView
        {
            get { return pythonConsole; }
        }

        public PythonConsole Console
        {
            get { return pythonConsole.consoleControl.Console; }
        }


    }
}
