using CommunityToolkit.Mvvm.ComponentModel;
using MVP_Voltage.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVP_Voltage
{
    public sealed class Settings:ObservableObject
    {
        public ROIModel MasterROI { get; set; } = new ROIModel();
        public ROIModel OCRROI { get; set; } = new ROIModel();

    }
}
