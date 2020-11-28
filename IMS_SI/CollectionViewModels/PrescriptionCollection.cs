using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IMS_SI.Models;

namespace IMS_SI.CollectionViewModels
{
    public class PrescriptionCollection
    {
        public Prescription Prescription { get; set; }
        public List<Patient> Patients { get; set; }
    }
}