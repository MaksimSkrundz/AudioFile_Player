using System.IO;
using System.Windows.Forms;

namespace CourseProject
{
    public class MetadataController
    {
        private readonly Label lblTitle;
        private readonly Label lblArtist;

        public MetadataController(Label lblTitle, Label lblArtist)
        {
            this.lblTitle = lblTitle;
            this.lblArtist = lblArtist;
        }

        public void Update(string path)
        {
            try
            {
                var md = MetadataReader.Read(path);
                lblTitle.Text = "Название: " + (md.Title ?? Path.GetFileName(path));
                lblArtist.Text = "Исполнитель: " + (md.Artist ?? "Неизвестно");
            }
            catch
            {
                lblTitle.Text = "Название: " + Path.GetFileName(path);
                lblArtist.Text = "Исполнитель: Неизвестно";
            }
        }

        public void Clear()
        {
            lblTitle.Text = "Название: -";
            lblArtist.Text = "Исполнитель: -";
        }
    }
}