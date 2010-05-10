namespace EventArgsTemplate
{
    using System;
    using System.Collections.Generic;
    using EnvDTE;
    using Microsoft.VisualStudio.TemplateWizard;

    class Wizard : IWizard
    {
        public void RunStarted(object automationObject, Dictionary<string,string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            
        }

        public void ProjectFinishedGenerating(Project project)
        {
            
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
           
        }

        public void RunFinished()
        {
            
        }
    }
}
