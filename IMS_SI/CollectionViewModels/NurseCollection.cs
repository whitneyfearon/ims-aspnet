using IMS_SI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IMS_SI.CollectionViewModels
{
    public class NurseCollection
    {
        public RegisterViewModel ApplicationUser { get; set; }
        public Nurse Nurse { get; set; }
    }
}