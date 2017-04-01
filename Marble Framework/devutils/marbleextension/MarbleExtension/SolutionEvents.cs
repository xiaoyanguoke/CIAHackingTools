﻿using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
namespace None.MarbleExtension
{
    class SolutionEvents : IVsSolutionEvents
    {
        private EnvDTE.DTE applicationObject;
        private MarbleExtensionPackage _package;

        public SolutionEvents(MarbleExtensionPackage package) 
        {
            _package = package;            
        }

        public void RegisterSolutionEvents()
        {
            uint cookie;
            var vsSolution = (IVsSolution)ServiceProvider.GlobalProvider.GetService(typeof(IVsSolution));
            vsSolution.AdviseSolutionEvents(this, out cookie);
            applicationObject = (EnvDTE.DTE)((_package as IServiceProvider).GetService(typeof(EnvDTE.DTE)));            
        }

        private void WaitForSolutionExistsThread()
        {
            //Update Marble Via Git
            string sMarbleUtils = _package.UserDataPath + "\\MarbleExtension";
            bool bReplaceMarbleH = false;
            //Create Utils Directory if it doesn't exist
            if (!Directory.Exists(sMarbleUtils))
                Directory.CreateDirectory(sMarbleUtils); 

            //Check for Git - If doesn't exist, init and clone
            GitInterop gitMeStuff = new GitInterop(sMarbleUtils);
            string sMarbleUtilsGit = sMarbleUtils + "\\.git";
            if (!Directory.Exists(sMarbleUtilsGit))
            {
                //Clone MarbuleExtensionBuilds here
                gitMeStuff.Git_Clone("ssh://git@stash.devlan.net:7999/devutils/marbleextensionbuilds.git", sMarbleUtils);
                bReplaceMarbleH = true;
            }

            //Get Marble Last Write Time
            string sMarbleOrig = sMarbleUtils + "\\Marble\\Marble.horig";
            if (File.Exists(sMarbleOrig))
            {
                DateTime dtOrig = File.GetLastWriteTime(sMarbleOrig);

                //See if were out of date
                if (gitMeStuff.Git_Pull())
                {
                    //If Marble Changed
                    DateTime dtNew = File.GetLastWriteTime(sMarbleOrig);
                    if (!dtOrig.Equals(dtNew))
                    {
                        //Alert User And Update
                        DialogResult result = MessageBox.Show("Your Marble Extension Utilities are out-of-date. Update Now? \r\n\r\n**Note: This will cause the Marble.h to be reset.", "Warning!", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {                          
                            bReplaceMarbleH = true;
                        }
                    }
                }
            }

            //Wait Till Solution Has Path Set
            while (applicationObject.Solution.FullName.Equals(""))
                System.Threading.Thread.Sleep(100);
            
            if(bReplaceMarbleH)
            {
                //Get Shared header
                string sDirPath = applicationObject.Solution.FullName;
                string sCurrentSolutionDir = System.IO.Path.GetDirectoryName(sDirPath);
                string sShareLocation = sCurrentSolutionDir + "\\Shared";
                string sMarble = sShareLocation + "\\Marble.h";          
  
                //Create Shared
                if (!Directory.Exists(sShareLocation))
                    Directory.CreateDirectory(sShareLocation);                              

                //Delete Old and Copy Over New
                File.Delete(sMarble);
                File.Copy(sMarbleOrig, sMarble);
            }     
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            
            System.Threading.Thread oThread = new System.Threading.Thread(new ThreadStart(WaitForSolutionExistsThread));

            oThread.Start();

            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            //Add Shared Folder To Additional Include Directories For Each Project
            //IVsUIShell uiShell = (IVsUIShell)(_package as IServiceProvider).GetService(typeof(SVsUIShell));
            //Guid clsid = Guid.Empty;
            //int result;
            //string sInclude = "$(SolutionDir)Shared;";
            
            //Projects pProjects = applicationObject.Solution.Projects;
            //foreach (Project project in pProjects) 
            //{
            //    string name = "Project File Path";
            //    string value = project.FullName.ToString();

            //    if (project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageMC || project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageVC)
            //    {
            //        //foreach (Configuration config in project.ConfigurationManager)
            //        //{


            //        // string name = config.Properties.Item("AdditionalIncludeDirectories").Name.ToString();
            //        // string value = config.Properties.Item("AdditionalIncludeDirectories").Value.ToString();
            //        //foreach (Property prop in config.Properties) 
            //        //foreach (Property prop in project.Properties) 
            //        //{

            //        //    string name = prop.Name.ToString();
            //        //    string value = prop.Value.ToString();
            //        uiShell.ShowMessageBox(0, ref clsid, name, value, string.Empty, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO, 0, out result);
            //        //}                    
            //        //}
            //    }                
            //}
            
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }
    }
}
