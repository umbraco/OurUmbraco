using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using uProject.Helpers;

namespace uProject.usercontrols.Deli.Package.Steps
{
    public partial class Licenses : UserControl
    {
        private Guid _projectGuid;


        private bool _editMode
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["projectId"]))
                {
                    return true;
                }
                return false;
            }
        }

        private int? _projectId;
        public int? ProjectId
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["id"]))
                {
                    _projectId = Int32.Parse(Request["id"]);
                }
                return _projectId;
            }
            set
            {
                _projectId = value;
            }
        }


        private void ReBindLicenses()
        {
        }

        protected void EditLicense(object sender, CommandEventArgs e)
        {
        }

        protected void UpdateLicense_Click(object sender, EventArgs e)
        {
        }


        protected void DeleteLicense(object sender, CommandEventArgs e)
        {
        }

        protected void DisableEnableLicense(object sender, CommandEventArgs e)
        {
        }

        protected void OnLicenseBound(object sender, RepeaterItemEventArgs e)
        {
        }



        protected void SaveLicense_Click(object sender, EventArgs e)
        {
        }


        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void GenKey_Click(object sender, EventArgs e)
        {
        }

        protected void DownloadKey_Click(object sender, EventArgs e)
        {
        }

        protected void SaveStep(object sender, EventArgs e)
        {
            //move to the license step
            ProjectCreatorHelper.MoveToNextStep(this, (int)ProjectId);
        }

        protected void MoveLast(object sender, EventArgs e)
        {
            //move to the screenshots step
            ProjectCreatorHelper.MoveToPreviousStep(this, (int)ProjectId);
        }
    }
}