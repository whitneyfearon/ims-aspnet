using IMS_SI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IMS_SI.CollectionViewModels
{
    public class PatientCollection
    {

        public RegisterViewModel ApplicationUser { get; set; }
        public Patient Patient { get; set; }
    }
}