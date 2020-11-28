using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using IMS_SI.CollectionViewModels;
using IMS_SI.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNet.Identity.Owin;

namespace IMS_SI.Controllers
{
    public class NurseController : Controller

    {
        private ApplicationDbContext db;

        private ApplicationUserManager _userManager;

        //Constructor
        public NurseController()
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

        // GET: Nurse
        [Authorize(Roles = "Nurse")]
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

        //List Of Doctors
        [Authorize(Roles = "Nurse")]
        public ActionResult ListOfDoctors()
        {
            var doctor = db.Doctors.Include(c => c.Department).ToList();
            return View(doctor);
        }


        //List of Available Doctors
        [Authorize(Roles = "Nurse")]
        public ActionResult AvailableDoctors()
        {
            var doctor = db.Doctors.Where(c => c.Status == "Active").ToList();
            return View(doctor);
        }


        //Patient Section
        [Authorize(Roles = "Nurse")]
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

        //List of Patients
        [Authorize(Roles = "Nurse")]
        public ActionResult ListOfPatients()
        {
            var patients = db.Patients.ToList();
            return View(patients);
        }



        [Authorize(Roles = "Nurse")]
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
        [Authorize(Roles = "Nurse")]
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

        //End Patient Section

        //List of Prescription
        [Authorize(Roles = "Nurse")]
        public ActionResult ListOfPrescription()
        {
            //var user = User.Identity.GetUserId();
            //var doctor = db.Doctors.SingleOrDefault(c => c.ApplicationUserId == user);
            var prescription = db.Prescription.ToList();
            return View(prescription);
        }

        //View Of Prescription
        [Authorize(Roles = "Nurse")]
        public ActionResult ViewPrescription(int id)
        {
            var prescription = db.Prescription.SingleOrDefault(c => c.Id == id);
            return View(prescription);
        }

      

        //Start Schedule Section

        //Check his Schedule 
        [Authorize(Roles = "Nurse")]
        public ActionResult ScheduleDetail()
        {
            string user = User.Identity.GetUserId();
            var nurse = db.Nurses.SingleOrDefault(c => c.ApplicationUserId == user);
            if (nurse != null)
            {
    var schedule = db.NurseSchedules.SingleOrDefault(c => c.NurseId == nurse.Id);
                if (schedule == null)
                {
                    return Content("<script language='javascript' type='text/javascript'>alert('No Schedule Available!');window.location.href = '/Nurse/Index';</script>");
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
                return Content("<script language='javascript' type='text/javascript'>alert('No Schedule Available!');window.location.href = '/Nurse/Index';</script>");
            }
            


        }


        //Edit Schedule
        [Authorize(Roles = "Nurse")]
        public ActionResult EditSchedule(int id)
        {
            var schedule = db.NurseSchedules.SingleOrDefault(c => c.Id == id);
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSchedule(int id, Schedule model)
        {
            var schedule = db.NurseSchedules.SingleOrDefault(c => c.Id == id);
            schedule.AvailableEndDay = model.AvailableEndDay;
            schedule.AvailableEndTime = model.AvailableEndTime;
            schedule.AvailableStartDay = model.AvailableStartDay;
            schedule.AvailableStartTime = model.AvailableStartTime;
            schedule.Status = model.Status;            
            db.SaveChanges();
            return RedirectToAction("ScheduleDetail");
        }

        //End schedule Section

        //Show Doctor Schedule
        [Authorize(Roles = "Nurse")]
        public ActionResult DoctorSchedule(int id)
        {
            var schedule = db.Schedules
                             .Include(c => c.Doctor)
                             .FirstOrDefault(c => c.DoctorId == id);
            if (schedule == null)
            {
                RedirectToAction("AvailableDoctors");
            }

            return View(schedule);
        }

        [Authorize(Roles = "Nurse")]
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

        //Start Appointment Section
        [Authorize(Roles = "Nurse")]
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
        [Authorize(Roles = "Nurse")]
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


        //List of Active Appointment
        [Authorize(Roles = "Nurse")]
        public ActionResult ListOfAppointments()
        {
            var date = DateTime.Now.Date;
            var appointment = db.Appointments
                                .Include(c => c.Doctor)
                                .Include(c => c.Patient)
                                .Where(c => c.Status == true)
                                .Where(c => c.AppointmentDate >= date).ToList();
            return View(appointment);
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

        //Edit Appointment
        [Authorize(Roles = "Nurse")]
        public ActionResult EditAppointment(int id)
        {
            var collection = new AppointmentCollection
            {
                Appointment = db.Appointments.Single(c => c.Id == id),
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
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
                Patients = db.Patients.ToList(),
                Doctors = db.Doctors.ToList()
            };
            if (model.Appointment.AppointmentDate >= DateTime.Now.Date)
            {
                var appointment = db.Appointments.Single(c => c.Id == id);
                appointment.PatientId = model.Appointment.PatientId;
                appointment.DoctorId = model.Appointment.DoctorId;
                appointment.AppointmentDate = model.Appointment.AppointmentDate;
                appointment.Problem = model.Appointment.Problem;
                appointment.Status = model.Appointment.Status;
                db.SaveChanges();
                if (model.Appointment.Status == true)
                {
                    return RedirectToAction("ListOfAppointments");
                }
                else
                {
                    return RedirectToAction("PendingAppointments");
                }
            }
            ViewBag.Messege = "Please Enter the Date greater than today or equal!!";

            return View(collection);
        }


        //Detail of appointment
        [Authorize(Roles = "Nurse")]
        public ActionResult DetailOfAppointment(int id)
        {
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient).SingleOrDefault(c => c.Id == id);
            return View(appointment);
        }

        //Delete Appointment
        [Authorize(Roles = "Nurse")]
        public ActionResult DeleteAppointment(int? id)
        {
            var appointment = db.Appointments.Single(c => c.Id == id);
            return View(appointment);
        }

        [HttpPost, ActionName("DeleteAppointment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAppointment(int id)
        {
            var appointment = db.Appointments.Single(c => c.Id == id);
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            if (appointment.Status)
            {
                return RedirectToAction("ListOfAppointments");
            }
            else
            {
                return RedirectToAction("PendingAppointments");
            }
        }

        // End of appointment section


    }
}