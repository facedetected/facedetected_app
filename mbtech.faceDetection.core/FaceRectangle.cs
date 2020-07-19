using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mbtech.faceDetection.core
{
    public class Response
    {
        public string code { get; set; }
        public string message { get; set; }
        public Error error { get; set; }
        public Rectangle[] items { get;set; }
    }
    public class FaceObject
    {
        public string faceId { get; set; }
        public FaceRectangle faceRectangle { get; set; }

    }
    public class FaceRectangle
    {
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }

    }

    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
    }
}
