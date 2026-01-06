using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVP_Voltage.Util
{
    public sealed record RoiDragArgs
    (
        RoiHandle Handle,
            double Dx,
            double Dy
    );
}
