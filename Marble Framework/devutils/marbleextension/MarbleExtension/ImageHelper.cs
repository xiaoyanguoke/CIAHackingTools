using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace None.MarbleExtension
{
    class ImageHelper : System.Windows.Forms.AxHost
    {
        private ImageHelper()
            : base(null)
        { }

        public static stdole.StdPicture GetIPictureFromImage(System.Drawing.Image image) 
        {
            return GetIPictureFromPicture(image) as stdole.StdPicture;
        }
    }
}
