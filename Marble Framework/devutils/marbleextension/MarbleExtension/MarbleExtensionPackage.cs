using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.CommandBars;
using EnvDTE;
using EnvDTE80;

//Icons from www.easyicon.net
//Marble Icon - http://www.easyicon.net/language.en/510940-4Balls_black_icon.html
//Rebuild Icon - http://www.easyicon.net/language.en/1154855-undo_icon.html
//Clean Icon - http://www.easyicon.net/language.en/559418-Eraser_icon.html
//Red Light Icon - http://www.easyicon.net/language.en/1076023-Red_ball_icon.html
//Green Light Icon - http://www.easyicon.net/language.en/1075969-Green_ball_icon.html

namespace None.MarbleExtension
{
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideAutoLoad(UIContextGuids.SolutionExists)] //Auto load when a solution exists
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidMarbleExtensionPkgString)]
    public sealed class MarbleExtensionPackage : Package
    {
        private EnvDTE.DTE applicationObject;
        private CommandBar cbMarbleBar;
        private CommandBarButton cbbAlgorithm;
        private CommandBarButton cbbState;
        private stdole.StdPicture picGreenLight = null; //image green light
        private stdole.StdPicture picRedLight = null; //image red light
        private string sCleanText = "Code State: Clean";
        private string sDirtyText = "Code State: Dirty";
        private string sMarbleLogName = "\\Marble Build Log.log";
        private string sBuildDependenciesLog = "\\Module List.csv";
        private OutputWindowWriter outWindow;
        private SolutionEvents seEvents;
        private bool bErr = false;
        private string sErr = "";

        //Constructor 
        public MarbleExtensionPackage()
        {          
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        protected override void Initialize()
        {         
            base.Initialize();
            seEvents = new SolutionEvents(this);
            outWindow = new OutputWindowWriter(this);

            //Install commandbar and menu items
            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item. Build Command
                CommandID menuBuild = new CommandID(GuidList.guidMarbleExtensionCmdSet, (int)PkgCmdIDList.cmdidMarbleBuild);
                MenuCommand buildItem = new MenuCommand(MarbleBuildCallback, menuBuild);
                mcs.AddCommand(buildItem);

                //Rebuild Command
                CommandID menuRebuild = new CommandID(GuidList.guidMarbleExtensionCmdSet, (int)PkgCmdIDList.cmdidMarbleRebuild);
                MenuCommand rebuildItem = new MenuCommand(MarbleRebuildCallback, menuRebuild);
                mcs.AddCommand(rebuildItem);

                //Clean Command
                CommandID menuClean = new CommandID(GuidList.guidMarbleExtensionCmdSet, (int)PkgCmdIDList.cmdidMarbleClean);
                MenuCommand cleanItem = new MenuCommand(MarbleCleanCallback, menuClean);
                mcs.AddCommand(cleanItem);
            }

            bool bCmdsInstalled = false; 
            applicationObject = (DTE)GetService(typeof(DTE));
            CommandBars commandBars = (CommandBars)applicationObject.CommandBars;
            try
            {
                cbMarbleBar = commandBars["Marble"];
            }
            catch (ArgumentException e)
            { }

            if (cbMarbleBar == null)
            {                
                //Add Marble as an available command bar
                cbMarbleBar = commandBars.Add("Marble", MsoBarPosition.msoBarTop);
            }
            else
                bCmdsInstalled = true;
            

            //Get Red/Green Light icons for status
            picGreenLight = ImageHelper.GetIPictureFromImage(Resources.GreenLight.ToBitmap());
            picRedLight = ImageHelper.GetIPictureFromImage(Resources.RedLight.ToBitmap());

            //Add Marble Build Button            
            CommandBarButton cbButtonBuildMarble = (CommandBarButton)cbMarbleBar.Controls.Add(Type.Missing, (int)PkgCmdIDList.cmdidMarbleBuild);
            cbButtonBuildMarble.Caption = "Marble Build";
            cbButtonBuildMarble.TooltipText = "Builds using the Marble Framework";
            cbButtonBuildMarble.Style = MsoButtonStyle.msoButtonIconAndCaption;
            cbButtonBuildMarble.Picture = ImageHelper.GetIPictureFromImage(Resources.Build.ToBitmap());
            cbButtonBuildMarble.Click += new _CommandBarButtonEvents_ClickEventHandler(marbleBuildClick);

            //Add Marble Rebuild Button
            CommandBarButton cbButtonRebuildMarble = (CommandBarButton)cbMarbleBar.Controls.Add(Type.Missing, (int)PkgCmdIDList.cmdidMarbleRebuild);
            cbButtonRebuildMarble.Caption = "Marble Rebuild";
            cbButtonRebuildMarble.TooltipText = "Rebuilds the solution using the Marble Framework";
            cbButtonRebuildMarble.Style = MsoButtonStyle.msoButtonIconAndCaption;
            cbButtonRebuildMarble.Picture = ImageHelper.GetIPictureFromImage(Resources.Rebuild.ToBitmap());
            cbButtonRebuildMarble.Click += new _CommandBarButtonEvents_ClickEventHandler(marbleRebuildClick);

            //Add Marble Clean Button
            CommandBarButton cbButtonCleanMarble = (CommandBarButton)cbMarbleBar.Controls.Add(Type.Missing, (int)PkgCmdIDList.cmdidMarbleClean);
            cbButtonCleanMarble.Caption = "Marble Clean";
            cbButtonCleanMarble.TooltipText = "Cleans changes made by the Marble Framework";
            cbButtonCleanMarble.Style = MsoButtonStyle.msoButtonIconAndCaption;
            cbButtonCleanMarble.Picture = ImageHelper.GetIPictureFromImage(Resources.Clean.ToBitmap());
            cbButtonCleanMarble.Click += new _CommandBarButtonEvents_ClickEventHandler(marbleCleanClick);

            //Add Algorithm Name
            cbbAlgorithm = (CommandBarButton)cbMarbleBar.Controls.Add();
            cbbAlgorithm.Caption = "Algorithm Used";
            cbbAlgorithm.Style = MsoButtonStyle.msoButtonCaption;
            cbbAlgorithm.Enabled = false;

            //Add Current State
            cbbState = (CommandBarButton)cbMarbleBar.Controls.Add();
            cbbState.Caption = sCleanText;
            cbbState.Style = MsoButtonStyle.msoButtonIconAndCaption;
            cbbState.State = MsoButtonState.msoButtonUp;
            cbbState.Picture = picGreenLight; //Start with green light
            cbbState.Enabled = true;

            //make command bar enabled and visible
            cbMarbleBar.Enabled = true;
            if (!bCmdsInstalled)
                cbMarbleBar.Visible = true;
           
            //Register for new/open solution events
            seEvents.RegisterSolutionEvents();         
        }                      
        #endregion

        //Marble Build Called
        private void MarbleBuildCallback(object sender, EventArgs e)
        {
            //Initialize writer            
            SafeOutputClear();          
            SafeOutputWrite("Starting Marble Builder");
            bErr = true;
            sErr = "";
            string sCurrentSolutionDir = System.IO.Path.GetDirectoryName(applicationObject.Solution.FullName);
            string sLogFile = sCurrentSolutionDir + sMarbleLogName;
            string sBuildDepends = sCurrentSolutionDir + sBuildDependenciesLog;
            string sMarble = sCurrentSolutionDir + "\\Shared\\Marble.h";

            //Validate Marble Exists
            if (!File.Exists(sMarble)) 
            {
                SafeOutputWrite("Failed to find Shared\\Marble.h");
                SafeOutputWrite("Please reload the solution");
                SafeOutputWrite("Exiting...");
                return;
            }

            //Check to see if algorithm is chosen - if none, call rebuild
            

            //Marble anything new using the same algorithm


            //Add new strings to receipt file
            
            
            //Modify Error Codes That Haven't Been Modified


            //Add New Error Code Changes To Log File


            //Start Dirty - On Success Change To Clean
            cbbState.Picture = picRedLight;
            cbbState.Caption = sDirtyText;

            //Build the solution
            applicationObject.Solution.SolutionBuild.Build(true);
            int iNumFailed = applicationObject.Solution.SolutionBuild.LastBuildInfo;
           
            //If successful build change back to green light
            if (iNumFailed == 0)
            {
                bErr = false;

                //Clean Up Special (Maintain Timestamps - Trick into Changes)


                //Parse Linker Log For Each Project To Determine Included Modules (Classes Linked Against)


                //Validate All Built Binaries
                ValidateBinaries();
            }

            if(bErr) //unsuccessful build
            {
                //Don't Clean Up
                SafeOutputWrite("Marble Build Failed");
                SafeOutputWrite(sErr);
                return;
            }

            cbbState.Picture = picGreenLight;
            cbbState.Caption = sCleanText;

            //Write Build Log to File            
            SafeOutputWrite("Marble Build Complete");
            SafeOutputFlush(sLogFile);
        }

        //Marble Rebuild Called
        private void MarbleRebuildCallback(object sender, EventArgs e)
        {
            //Conduct a Marble Clean
            MarbleCleanCallback(sender, e);

            //Initialize writer
            SafeOutputClear();
            SafeOutputWrite("Starting Marble Builder");
            bErr = true;
            sErr = "";
            string sCurrentSolutionDir = System.IO.Path.GetDirectoryName(applicationObject.Solution.FullName);
            string sLogFile = sCurrentSolutionDir + sMarbleLogName;
            string sBuildDepends = sCurrentSolutionDir + sBuildDependenciesLog;
            string sShared = sCurrentSolutionDir + "\\Shared";
            string sMarble = sShared + "\\Marble.h";

            //Validate Marble Exists
            if (!File.Exists(sMarble))
            {                
                SafeOutputWrite("Failed to find Shared\\Marble.h");
                SafeOutputWrite("Please reload the solution");
                SafeOutputWrite("Exiting...");
                return;
            }

            //Choose new algorithm - Check Filters


            //Add Selected Algorithm To Solution Shared


            //Set Algorithm in text box

            
            //Run Marbler


            //Add strings to receipt file


            //Modify Error Codes


            //Add Error Code Changes To Log File


            //Start Dirty - On Success Change To Clean
            cbbState.Picture = picRedLight;
            cbbState.Caption = sDirtyText;

            //Rebuild the solution
            applicationObject.Solution.SolutionBuild.Clean(true);
            applicationObject.Solution.SolutionBuild.Build(true);
            int iNumFailed = applicationObject.Solution.SolutionBuild.LastBuildInfo;

            //If successful build change back to green light            
            if (iNumFailed == 0)
            {
                bErr = false;
                

                //Clean up changes made


                //Parse Linker Log For Each Project To Determine Included Modules (Classes Linked Against)


                //Validate All Built Binaries                
                ValidateBinaries();                
            }

            //unsuccessful build
            if(bErr)
            {
                //Don't Clean Up
                SafeOutputWrite("Marble Build Failed");
                SafeOutputWrite(sErr);
                return;
            }

            cbbState.Picture = picGreenLight;
            cbbState.Caption = sCleanText;

            //Write Build Log to File            
            SafeOutputWrite("Marble Build Complete");
            SafeOutputFlush(sLogFile);
        }

        //Marble Clean Called
        private void MarbleCleanCallback(object sender, EventArgs e)
        {
            //Cleanup Marble Modifications - Don't Clear Log File Only Window
            SafeOutputClear();
            SafeOutputWrite("Clearing Marble Changes");
            bool bErr = false;
            string sErrString = ""; 

            //Execute Mender Here

            //Execute Error Cleanup

            if (!bErr)
                SafeOutputWrite("Successfully Cleared Marble Changes");
            else
                SafeOutputWrite(sErrString);
        }

        //Validates All Build Binaries Do Not Contain Strings
        private void ValidateBinaries() 
        {
            //Get Path To Marble Receipt
            SafeOutputWrite("Verifying Built Binaries");
            string sCurrentSolutionDir = System.IO.Path.GetDirectoryName(applicationObject.Solution.FullName);
            string sReceipt = sCurrentSolutionDir + "\\Marble Receipt.xml";

            //Make Sure Receipt Exists
            //if (!File.Exists(sReceipt))
            //{
            //    bErr = true;
            //    sErr += "Failed to find the Marble Receipt file!";
            //    return;
            //}
            
            //Run Validator to verify 
            foreach (Project proj in applicationObject.Solution.Projects) 
            {
                string sVerify = "Verifying Project: " + proj.Name;
                SafeOutputWrite(sVerify);

                if (!bErr)
                {
                    SafeOutputWrite("Verified");
                }
                else 
                {
                    sErr += "Failed to verify: ";
                    return;
                }
            }            
        }

        //Safely write string to output window
        private void SafeOutputWrite(string sString)         
        {
            if (outWindow != null)
            {
                outWindow.Write(sString);
            }
        }

        //Safely Clear
        private void SafeOutputClear()
        {
            if (outWindow != null)
            {
                outWindow.Clear();
            }
        }

        //Safely Flush
        private void SafeOutputFlush(string sOutputLogFile)
        {
            if (outWindow != null)
            {
                outWindow.FlushToLogFile(sOutputLogFile);
            }
        }

        //Forward to Marble Build Callback
        private void marbleBuildClick(CommandBarButton ctrl, ref bool cancel)
        {
            MarbleBuildCallback(null, null);
        }

        //Forward to Marble Rebuild Callback
        private void marbleRebuildClick(CommandBarButton ctrl, ref bool cancel)
        {
            MarbleRebuildCallback(null, null);
        }

        //Forward to Marble Clean Callback
        private void marbleCleanClick(CommandBarButton ctrl, ref bool cancel)
        {
            MarbleCleanCallback(null, null);
        }

    }
}
 