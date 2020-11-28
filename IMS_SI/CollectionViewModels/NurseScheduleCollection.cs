using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IMS_SI.Models;

namespace IMS_SI.CollectionViewModels
{
    public class NurseScheduleCollection
    {
        public NurseSchedule NurseSchedule { get; set; }
        public IEnumerable<Nurse> Nurses { get; set; }
    }
}