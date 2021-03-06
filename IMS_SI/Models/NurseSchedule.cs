﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IMS_SI.Models
{
    public class NurseSchedule
    {
        public int Id { get; set; }

        public Nurse Nurse { get; set; }
        [Display(Name = "Nurse Name")]
        public int NurseId { get; set; }

        [Required]
        [Display(Name = "Start Day")]
        public string AvailableStartDay { get; set; }

        [Required]
        [Display(Name = "End Day")]
        public string AvailableEndDay { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime AvailableStartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime AvailableEndTime { get; set; }

        [Required]
        public string Status { get; set; }
    }
}