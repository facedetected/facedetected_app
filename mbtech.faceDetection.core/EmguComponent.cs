using System;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace mbtech.faceDetection.core
{
    public class EmguComponent : IComponent
    {

      
        public EmguComponent()
        {
           
        }
        public Response ProcessImage(string file)
        {
            var response = new Response();
            List<Rectangle> facesDetected = new List<Rectangle>() ;
            try
            {
                Image<Bgr, byte> graImage = new Image<Bgr, byte>(file);
                using (CascadeClassifier face = new CascadeClassifier("resources/haarcascade_frontalface_default.xml"))
                {
                    using (UMat ugray = new UMat())
                    {
                        CvInvoke.CvtColor(graImage, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                        CvInvoke.EqualizeHist(ugray, ugray);

                         facesDetected = face.DetectMultiScale(
                           ugray, 1.1, 10, new Size(20, 20)).ToList();

                    
                    }
                    response.items = facesDetected.ToArray();

                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return response;
        }
    }
}
