using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using uProject.usercontrols.Deli.Package;

namespace uProject.Helpers
{
    public static class ProjectCreatorHelper
    {

        public static void MoveToNextStep(UserControl control, int projectId)
        {
            Editor c = (Editor)control.Parent.Parent;
            c.GotoNextStep(c.Step.Value, projectId);

        }

        public static void MoveToPreviousStep(UserControl control, int projectId)
        {
            Editor c = (Editor)control.Parent.Parent;
            c.GoToPreviousStep(c.Step.Value, projectId);

        }


    }
}
