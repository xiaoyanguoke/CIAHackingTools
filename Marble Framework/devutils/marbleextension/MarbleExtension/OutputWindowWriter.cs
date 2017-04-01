using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using EnvDTE80;
using EnvDTE;

namespace None.MarbleExtension
{
    class OutputWindowWriter : TextWriter
    {
        #region Constants
     
        /// Name of the custom output pane.        
        private const string PaneName = "Marble Log";
        
        /// Guid for the custom output pane.        
        private static readonly Guid PaneGuid = new Guid("CB2D8728-97B6-4658-B258-A826EDD96D37");

        #endregion
        #region Members
        
        /// Output window.        
        private IVsOutputWindow _outputWindow;
        
        /// Output window pane.
        private IVsOutputWindowPane _outputPane;
        
        /// Parent package.        
        private MarbleExtensionPackage _package;

        //Output pane log - to be dumped to a file
        private string sOutputPaneStrings;
              
        #endregion

        #region Properties

        /// Initializes new instance of the writer.
        public OutputWindowWriter(MarbleExtensionPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            _package = package;
        }

        /// Gets output window object.
        private IVsOutputWindow OutputWindow
        {
            get
            {
                if (_outputWindow == null)
                {
                    DTE dte = (DTE)(( _package as IServiceProvider ).GetService(typeof(DTE)));
                    IServiceProvider serviceProvider = new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                    _outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                }

                return _outputWindow;
            }
        }

        /// Returns output pane.
        private IVsOutputWindowPane OutputPane
        {
            get
            {
                if (_outputPane == null)
                {
                    Guid generalPaneGuid = PaneGuid;
                    IVsOutputWindowPane pane;

                    OutputWindow.GetPane(ref generalPaneGuid, out pane);

                    if (pane == null)
                    {
                        OutputWindow.CreatePane(ref generalPaneGuid, PaneName, 1, 1);
                        OutputWindow.GetPane(ref generalPaneGuid, out pane);
                    }

                    _outputPane = pane;                    
                }

                return _outputPane;
            }
        }
        #endregion

        #region Methods

        /// Writes a message into our output pane.
        public override void Write(string message)
        {
            sOutputPaneStrings += message;
            sOutputPaneStrings += "\r\n";            
            OutputPane.OutputString(message);
            OutputPane.OutputString("\r\n");
        }
      
        /// Writes a character into our output pane.
        public override void Write(char ch)
        {
            sOutputPaneStrings += ch.ToString();
            sOutputPaneStrings += "\r\n";
            OutputPane.OutputString(ch.ToString());
            OutputPane.OutputString("\r\n");
        }

        //Writes the output pane text to the specified log file
        public void FlushToLogFile(string FilePath) 
        {
            FileStream f = File.Open(FilePath, FileMode.Create);            
            StreamWriter sw = new StreamWriter(f);                    
            sw.Write(sOutputPaneStrings);
            sw.Flush();
            sw.Close();
            f.Close();
        }
        
        /// Clears output pane.
        public void Clear()
        {
            OutputPane.Clear();
            sOutputPaneStrings = "";
        }
        
        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }
        #endregion

    }
}
