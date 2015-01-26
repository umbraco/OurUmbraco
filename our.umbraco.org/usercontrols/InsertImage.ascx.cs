using System;
using System.Linq;
using System.Web.UI;
using System.IO;

namespace our.usercontrols
{
    public partial class InsertImage : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
                InsertImageMarkdown1.Visible = true;
                InsertImageMarkdown2.Visible = true;
                InsertImageMarkdown3.Visible = true;
                InsertImageRte.Visible = false;
           
         }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            lb_notvalid.Visible = false;

            if (FileUpload1.HasFile)
            {
                Guid g = Guid.NewGuid();
                DirectoryInfo updir = new DirectoryInfo(Server.MapPath("/media/upload/" + g));

                if (!updir.Exists)
                    updir.Create();

                FileUpload1.SaveAs(updir.FullName + "/" + FileUpload1.FileName);

                if (IsValidImage(updir.FullName + "/" + FileUpload1.FileName))
                {

                    tb_url.Text = "/media/upload/" + g + "/" +
                        ResizeImage(updir.FullName + "/", FileUpload1.FileName,
                        500, 1000, true);
                }
                else
                {
                    lb_notvalid.Visible = true;
                }
            }
        }

        private bool IsValidImage(string filename)
        {
            try
            {
                System.Drawing.Image newImage = System.Drawing.Image.FromFile(filename);
            }
            catch (OutOfMemoryException ex)
            {
                // Image.FromFile will throw this if file is invalid.
                // Don't ask me why.
                return false;
            }
            return true;
        }

        private string ResizeImage(string storageDir, string originalFile, int newWidth, int maxHeight, bool onlyResizeIfWider)
        {
            System.Drawing.Image FullsizeImage = System.Drawing.Image.FromFile(storageDir + originalFile);

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (onlyResizeIfWider)
            {
                if (FullsizeImage.Width <= newWidth)
                {
                    newWidth = FullsizeImage.Width;

                    return originalFile;
                }
            }

            int NewHeight = FullsizeImage.Height * newWidth / FullsizeImage.Width;
            if (NewHeight > maxHeight)
            {
                // Resize with height instead
                newWidth = FullsizeImage.Width * maxHeight / FullsizeImage.Height;
                NewHeight = maxHeight;
            }

            System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(newWidth, NewHeight, null, IntPtr.Zero);

            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();

            DirectoryInfo resizedir = new DirectoryInfo(storageDir + "/rs");

            if (!resizedir.Exists)
                resizedir.Create();

            // Save resized picture
            NewImage.Save(storageDir + "/rs/" + originalFile);

            return "rs/" + originalFile;
        }

    }
}