using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using IMS_SI.CollectionViewModels;
using IMS_SI.Models;
using Microsoft.AspNetCore.Identity;

namespace IMS_SI.Controllers
{
    public class ReceptionistController : Controller
    {
        private ApplicationDbContext db;

        private ApplicationUserManager _userManager;

        //Constructor
        public ReceptionistController()
        {
            db = new ApplicationDbContext();
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        //Destructor
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
        }

        [Authorize(Roles = "Receptionist")]
        public ActionResult Index(string message)
        {
            var date = DateTime.Now.Date;
            ViewBag.Messege = message;
            var user = User.Identity.GetUserId();
            //var doctor = db.Doctors.Single(c => c.ApplicationUserId == user);
            var model = new CollectionOfAll
            {

                Departments = db.Department.ToList(),
                Doctors = db.Doctors.ToList(),
                Patients = db.Patients.ToList(),
                Medicines = db.Medicines.ToList(),
                ActiveAppointments =
                    db.Appointments.Where(c => c.Status).Where(c => c.AppointmentDate >= date).ToList(),
                PendingAppointments = db.Appointments.Where(c => c.Status == false)
                    .Where(c => c.AppointmentDate >= date).ToList(),
            };
            return View(model);
        }


        // Patient Section

        [Authorize(Roles = "Receptionist")]
        public ActionResult AddPatient()
        {
            var collection = new PatientCollection
            {
                ApplicationUser = new RegisterViewModel(),
                Patient = new Patient()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPatient(PatientCollection model)
        {
            var user = new ApplicationUser
            {
                UserName = model.ApplicationUser.UserName,
                Email = model.ApplicationUser.Email,
                UserRole = "Patient",
                RegisteredDate = DateTime.Now.Date
            };
            var result = await UserManager.CreateAsync(user, model.ApplicationUser.Password);
            if (result.Succeeded)
            {
                await UserManager.AddToRoleAsync(user.Id, "Patient");
                var patient = new Patient
                {
                    FirstName = model.Patient.FirstName,
                    LastName = model.Patient.LastName,
                    FullName = model.Patient.FirstName + " " + model.Patient.LastName,
                    EmailAddress = model.ApplicationUser.Email,
                    //ContactNo = model.Doctor.ContactNo,
                    PhoneNo = model.Patient.PhoneNo,
                    Gender = model.Patient.Gender,
                    BloodGroup = model.Patient.BloodGroup,
                    ApplicationUserId = user.Id,
                    DateOfBirth = model.Patient.DateOfBirth,
                    Address = model.Patient.Address
                };
                db.Patients.Add(patient);
                db.SaveChanges();
                return RedirectToAction("ListOfPatients");
            }
            return HttpNotFound();
        }

        [Authorize(Roles="Receptionist")]
        public ActionResult EditPatient(int id)
        {
            var patient = db.Patients.Single(c => c.Id == id);
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPatient(int id, Patient model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var patient = db.Patients.Single(c => c.Id == id);
            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.FullName = model.FirstName + " " + model.LastName;
            patient.Address = model.Address;
            patient.BloodGroup = model.BloodGroup;
            patient.Contact = model.Contact;
            patient.DateOfBirth = model.DateOfBirth;
            patient.EmailAddress = model.EmailAddress;
            patient.Gender = model.Gender;
            patient.PhoneNo = model.PhoneNo;
            db.SaveChanges();
            return RedirectToAction("ListOfPatients");
        }

        //Delete Patient
        [Authorize(Roles="Receptionist")]
        public ActionResult DeletePatient()
        {
            return View();
        }

        [HttpPost, ActionName("DeletePatient")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePatient(string id)
        {
            var patient = db.Patients.Single(c => c.ApplicationUserId == id);
            var user = db.Users.Single(c => c.Id == id);
            db.Users.Remove(user);
            db.Patients.Remove(patient);
            db.SaveChanges();
            return RedirectToAction("ListOfPatients");
        }

        //List of Patients
        [Authorize(Roles = "Receptionist")]
        public ActionResult ListOfPatients()
        {
            var patients = db.Patients.ToList();
            return View(patients);
        }
        //End Patient Section

        [Authorize(Roles = "Receptionist")]
        public ActionResult AllDoctorSchedule()
        {
            var schedule = db.Schedules
                             .Include(c => c.Doctor).ToList();
            if (schedule == null)
            {
                RedirectToAction("AvailableDoctors");
            }

            return View(schedule);
        }
        [Authorize(Roles = "Receptionist")]
        public ActionResult AllNurseSchedule()
        {
            var schedule = db.NurseSchedules
                             .Include(c => c.Nurse).ToList();
            if (schedule == null)
            {
                RedirectToAction("AvailableNurses");
            }

            return View(schedule);
        }





        //Start Appointment Section

        //Add Appointment
        [Authorize(Roles = "Receptionist")]
        public ActionResult AddAppointment()
        {
            var collection = new AppointmentCollection
            {
                Appointment = new Appointment(),
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAppointment(AppointmentCollection model)
        {
            string user = User.Identity.GetUserId();
            var collection = new AppointmentCollection
            {
                Appointment = model.Appointment,
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
            };
            if (model.Appointment.AppointmentDate >= DateTime.Now.Date)
            {
                var appointment = new Appointment();
                appointment.PatientId = model.Appointment.PatientId;
                appointment.DoctorId = model.Appointment.DoctorId;
                appointment.AppointmentDate = model.Appointment.AppointmentDate;
                appointment.Problem = model.Appointment.Problem;
                appointment.Status = model.Appointment.Status;

                db.Appointments.Add(appointment);
                db.SaveChanges();

                if (model.Appointment.Status == true)
                {
                    return RedirectToAction("ActiveAppointments");
                }
                else
                {
                    return RedirectToAction("PendingAppointments");
                }
            }
            ViewBag.Messege = "Please Enter the Date greater than today or equal!!";

            return View(collection);
        }

        //List of Active Appointments
        [Authorize(Roles = "Receptionist")]
        public ActionResult ActiveAppointments()
        {
            var date = DateTime.Now.Date;
            var appointment = db.Appointments
                                .Include(c => c.Doctor)
                                .Include(c => c.Patient)
                                .Where(c => c.Status == true)
                                .Where(c => c.AppointmentDate >= date).ToList();
            return View(appointment);
        }

        //List of Appointments
        [Authorize(Roles = "Receptionist")]
        public ActionResult ListOfAppointments()
        {
            var appointment = db.Appointments
                                .Include(c => c.Doctor)
                                .Include(c => c.Patient)
                                .ToList();
            return View(appointment);
        }

        //Edit Appointment
        [Authorize(Roles = "Receptionist")]
        public ActionResult EditAppointment(int id)
        {
            var collection = new AppointmentCollection
            {
                Appointment = db.Appointments.SingleOrDefault(c => c.Id == id),
                Doctors = db.Doctors.ToList(),
                Patients = db.Patients.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAppointment(int id, AppointmentCollection model)
        {
            var collection = new AppointmentCollection
            {
                Appointment = model.Appointment,
                Doctors = db.Doctors.ToList(),
                Patients = db.Patients.ToList()
            };
            if (model.Appointment.AppointmentDate >= DateTime.Now.Date)
            {
                var appointment = db.Appointments.SingleOrDefault(c => c.Id == id);
                appointment.DoctorId = model.Appointment.DoctorId;
                appointment.PatientId = model.Appointment.PatientId;
                appointment.AppointmentDate = model.Appointment.AppointmentDate;
                appointment.Problem = model.Appointment.Problem;
                db.SaveChanges();
                return RedirectToAction("ListOfAppointments");
            }
            ViewBag.Messege = "Please Enter the Date greater than today or equal!!";

            return View(collection);
        }

        //Detail of appointment
        [Authorize(Roles = "Receptionist")]
        public ActionResult DetailOfAppointment(int id)
        {
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient).SingleOrDefault(c => c.Id == id);
            return View(appointment);
        }


        //Delete Appointment
        [Authorize(Roles = "Receptionist")]
        public ActionResult DeleteAppointment(int? id)
        {
            var appointment = db.Appointments.SingleOrDefault(c => c.Id == id);
            return View(appointment);
        }

        [HttpPost, ActionName("DeleteAppointment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAppointment(int id)
        {
            var appointment = db.Appointments.SingleOrDefault(c => c.Id == id);
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            return RedirectToAction("ListOfAppointments");
        }

        //List of Pending Appointments
        public ActionResult PendingAppointments()
        {
            var date = DateTime.Now.Date;
            var appointment = db.Appointments
                                .Include(c => c.Doctor)
                                .Include(c => c.Patient)
                                .Where(c => c.Status == false)
                                .Where(c => c.AppointmentDate >= date).ToList();
            return View(appointment);
        }
        //End Appointment Section



        //Start Doctor Section

        //List of Available Doctors
        [Authorize(Roles = "Receptionist")]
        public ActionResult AvailableDoctors()
        {
            var doctor = db.Doctors.Where(c => c.Status == "Active").ToList();
            return View(doctor);
        }

        //Show Doctor Schedule
        [Authorize(Roles = "Receptionist")]
        public ActionResult DoctorSchedule(int id)
        {
            var schedule = db.Schedules.Include(c => c.Doctor).SingleOrDefault(c => c.DoctorId == id);
            return View(schedule);
        }

        //Doctor Detail
        [Authorize(Roles = "Receptionist")]
        public ActionResult DoctorDetail(int id)
        {
            var doctor = db.Doctors.SingleOrDefault(c => c.Id == id);
            return View(doctor);
        }

        //List Of Doctors
        [Authorize(Roles = "Receptionist")]
        public ActionResult ListOfDoctors()
        {
            var doctor = db.Doctors.Include(c => c.Department).ToList();
            return View(doctor);
        }

        //End Doctor Section


        //Start Nurse Section

        //List of Available Doctors
        [Authorize(Roles = "Receptionist")]
        public ActionResult AvailableNurses()
        {
            var doctor = db.Nurses.Where(c => c.Status == "Active").ToList();
            return View(doctor);
        }

        //Show Doctor Schedule
        [Authorize(Roles = "Receptionist")]
        public ActionResult NurseSchedule(int id)
        {
            var schedule = db.NurseSchedules.Include(c => c.Nurse).SingleOrDefault(c => c.NurseId == id);
            return View(schedule);
        }

        //Doctor Detail
        [Authorize(Roles = "Receptionist")]
        public ActionResult NurseDetail(int id)
        {
            var nurse = db.Nurses.SingleOrDefault(c => c.Id == id);
            return View(nurse);
        }

        //List Of Doctors
        [Authorize(Roles = "Receptionist")]
        public ActionResult ListOfNurses()
        {
            var nurse = db.Nurses.ToList();
            return View(nurse);
        }

        //End Doctor Section



        //Start Receptionist Section

        //List of Available Receptionists
        [Authorize(Roles = "Receptionist")]
        public ActionResult AvailableReceptionists()
        {
            var Receptionist = db.Receptionists.Where(c => c.Status == "Active").ToList();
            return View(Receptionist);
        }

        //Show Receptionist Schedule
        [Authorize(Roles = "Receptionist")]
        public ActionResult ReceptionistSchedule(int id)
        {
            var schedule = db.ReceptionistSchedules.Include(c => c.Receptionist).SingleOrDefault(c => c.ReceptionistId == id);
            return View(schedule);
        }

        //Doctor Detail
        [Authorize(Roles = "Receptionist")]
        public ActionResult ReceptionistDetail(int id)
        {
            var Receptionist = db.Receptionists.SingleOrDefault(c => c.Id == id);
            return View(Receptionist);
        }

        //End Receptionist Section


        //Start Prescription Section

        //List of Prescription
        [Authorize(Roles = "Receptionist")]
        public ActionResult ListOfPrescription()
        {
            var prescription = db.Prescription.ToList();
            return View(prescription);
        }

        //Prescription View
        public ActionResult PrescriptionView(int id)
        {
            var prescription = db.Prescription.SingleOrDefault(c => c.Id == id);
            return View(prescription);
        }

        //End Prescription Section

        //Start Schedule Section

        //Check his Schedule 
        [Authorize(Roles = "Receptionist")]
        public ActionResult ScheduleDetail()
        {
            string user = User.Identity.GetUserId();
            var receptionist = db.Receptionists.Single(c => c.ApplicationUserId == user);

            if (receptionist != null)
            {
 var schedule = db.ReceptionistSchedules.SingleOrDefault(c => c.ReceptionistId == receptionist.Id);
            if (schedule == null)
            {
                return Content("<script language='javascript' type='text/javascript'>alert('No Schedule Available!');window.location.href = '/Receptionist/Index';</script>");
                //Content("<script language='javascript' type='text/javascript'>alert('No Schedule Available!');</script>");
                // return RedirectToAction("Index", "Doctor");

            }
            else
            {
                return View(schedule);
            }
            }
            else
            {
                return Content("<script language='javascript' type='text/javascript'>alert('No Schedule Available!');window.location.href = '/Receptionist/Index';</script>");
            }
           



        }

        //Edit Schedule
        [Authorize(Roles = "Receptionist")]
        public ActionResult EditSchedule(int id)
        {
            var schedule = db.ReceptionistSchedules.Single(c => c.Id == id);
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSchedule(int id, Schedule model)
        {
            var schedule = db.ReceptionistSchedules.Single(c => c.Id == id);
            schedule.AvailableEndDay = model.AvailableEndDay;
            schedule.AvailableEndTime = model.AvailableEndTime;
            schedule.AvailableStartDay = model.AvailableStartDay;
            schedule.AvailableStartTime = model.AvailableStartTime;
            schedule.Status = model.Status;
            db.SaveChanges();
            return RedirectToAction("ScheduleDetail");
        }

        //End schedule Section
    }
}