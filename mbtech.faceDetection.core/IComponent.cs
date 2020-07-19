using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mbtech.faceDetection.core
{
    public interface IComponent
    {
        Response ProcessImage(string file);
    }
}
