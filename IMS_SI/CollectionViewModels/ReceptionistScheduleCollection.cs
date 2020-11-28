using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IMS_SI.Models;

namespace IMS_SI.CollectionViewModels
{
    public class ReceptionistScheduleCollection
    {
        public ReceptionistSchedule ReceptionistSchedule{ get; set; }
        public IEnumerable<Receptionist> Receptionists { get; set; }
    }
}