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

namespace IMS_SI.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db;

        private ApplicationUserManager _userManager;

        //Constructor
        public AdminController()
        {
            db = new ApplicationDbContext();
        }

        //Destructor
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }
        public class OnlyAllowedAttribute : AuthorizeAttribute
        {
            public override void OnAuthorization(AuthorizationContext filterContext)
            {
                base.OnAuthorization(filterContext);

                if (System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    if (filterContext.Result is HttpUnauthorizedResult)
                    {
                        filterContext.Result = new RedirectResult("~/Account/AccessDenied");
                    }
                }
                else
                {
                    filterContext.Result = new RedirectResult("~/Account/Login");
                }
            }
        }
        // GET: Admin
        [Authorize(Roles = "Admin")]
        //[OnlyAllowedAttribute(Roles = "Admin")]
        public ActionResult Index(string message)
        {
            
            var date = DateTime.Now.Date;
            ViewBag.Messege = message;
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

        //Department Section

        //Department List
        [Authorize(Roles = "Admin")]
        public ActionResult DepartmentList()
        {
            var model = db.Department.ToList();
            return View(model);
        }

        //Add Department
        [Authorize(Roles = "Admin")]
        public ActionResult AddDepartment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDepartment(Department model)
        {
            if (db.Department.Any(c => c.Name == model.Name))
            {
                ModelState.AddModelError("Name", "Name already present!");
                return View(model);
            }

            db.Department.Add(model);
            db.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        //Edit Department
        [Authorize(Roles = "Admin")]
        public ActionResult EditDepartment(int id)
        {
            var model = db.Department.SingleOrDefault(c => c.Id == id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDepartment(int id, Department model)
        {
            var department = db.Department.Single(c => c.Id == id);
            department.Name = model.Name;
            department.Description = model.Description;
            department.Status = model.Status;
            db.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteDepartment(int? id)
        {
            var department = db.Department.Single(c => c.Id == id);
            return View(department);
        }

        [HttpPost, ActionName("DeleteDepartment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDepartment(int id)
        {
            var department = db.Department.SingleOrDefault(c => c.Id == id);
            db.Department.Remove(department);
            db.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        //End Department Section

        

        //Start Medicine Section

        //Add Medicine
        [Authorize(Roles = "Admin")]
        public ActionResult AddMedicine()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMedicine(Medicine model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            db.Medicines.Add(model);
            db.SaveChanges();
            return RedirectToAction("ListOfMedicine");
        }

        //List of Medicines
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfMedicine()
        {
            var medicine = db.Medicines.ToList();
            return View(medicine);
        }

        //Edit Medicine
        [Authorize(Roles = "Admin")]
        public ActionResult EditMedicine(int id)
        {
            var medicine = db.Medicines.Single(c => c.Id == id);
            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMedicine(int id, Medicine model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var medicine = db.Medicines.Single(c => c.Id == id);
            medicine.Name = model.Name;
            medicine.Description = model.Description;
            medicine.Price = model.Price;
            medicine.Quantity = model.Quantity;

            db.SaveChanges();
            return RedirectToAction("ListOfMedicine");
        }

        //Delete Medicine
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteMedicine(int? id)
        {
            return View();
        }

        [HttpPost, ActionName("DeleteMedicine")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMedicine(int id)
        {
            var medicine = db.Medicines.Single(c => c.Id == id);
            db.Medicines.Remove(medicine);
            db.SaveChanges();
            return RedirectToAction("ListOfMedicine");
        }

        //End Medicine Section

        //Start Doctor Section

        //Add Doctor 
        [Authorize(Roles = "Admin")]
        public ActionResult AddDoctor()
        {
            var collection = new DoctorCollection
            {
                ApplicationUser = new RegisterViewModel(),
                Doctor = new Doctor(),
                Departments = db.Department.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddDoctor(DoctorCollection model)
        {
            var user = new ApplicationUser
            {
                UserName = model.ApplicationUser.UserName,
                Email = model.ApplicationUser.Email,
                UserRole = "Doctor",
                RegisteredDate = DateTime.Now.Date
            };
            var result = await UserManager.CreateAsync(user, model.ApplicationUser.Password);
            if (result.Succeeded)
            {
                await UserManager.AddToRoleAsync(user.Id, "Doctor");
                var doctor = new Doctor
                {
                    FirstName = model.Doctor.FirstName,
                    LastName = model.Doctor.LastName,
                    FullName = "Dr. " + model.Doctor.FirstName + " " + model.Doctor.LastName,
                    EmailAddress = model.ApplicationUser.Email,
                    //ContactNo = model.Doctor.ContactNo,
                    PhoneNo = model.Doctor.PhoneNo,
                    //Designation = model.Doctor.Designation,
                    Education = model.Doctor.Education,
                    DepartmentId = model.Doctor.DepartmentId,
                    Specialization = model.Doctor.Specialization,
                    Gender = model.Doctor.Gender,
                    BloodGroup = model.Doctor.BloodGroup,
                    ApplicationUserId = user.Id,
                    DateOfBirth = model.Doctor.DateOfBirth,
                    Address = model.Doctor.Address,
                    Status = model.Doctor.Status
                };
                db.Doctors.Add(doctor);
                db.SaveChanges();
                return RedirectToAction("ListOfDoctors");
            }

            return HttpNotFound();

        }

        [Authorize(Roles = "Admin")]
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


        //List Of Doctors
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfDoctors()
        {
            var doctor = db.Doctors.Include(c => c.Department).ToList();
            return View(doctor);
        }

        //Detail of Doctoroles = "Admin")]
        public ActionResult DoctorDetail(int id)
        {
            var doctor = db.Doctors.Include(c => c.Department).SingleOrDefault(c => c.Id == id);
            return View(doctor);
        }

        //Edit Doctors
        [Authorize(Roles = "Admin")]
        public ActionResult EditDoctors(int id)
        {
            var collection = new DoctorCollection
            {
                Departments = db.Department.ToList(),
                Doctor = db.Doctors.Single(c => c.Id == id)
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDoctors(int id, DoctorCollection model)
        {
            var doctor = db.Doctors.Single(c => c.Id == id);
            doctor.FirstName = model.Doctor.FirstName;
            doctor.LastName = model.Doctor.LastName;
            doctor.FullName = "Dr. " + model.Doctor.FirstName + " " + model.Doctor.LastName;
            //doctor.ContactNo = model.Doctor.ContactNo;
            doctor.PhoneNo = model.Doctor.PhoneNo;
            //doctor.Designation = model.Doctor.Designation;
            doctor.Education = model.Doctor.Education;
            doctor.DepartmentId = model.Doctor.DepartmentId;
            doctor.Specialization = model.Doctor.Specialization;
            doctor.Gender = model.Doctor.Gender;
            doctor.BloodGroup = model.Doctor.BloodGroup;
            doctor.DateOfBirth = model.Doctor.DateOfBirth;
            doctor.Address = model.Doctor.Address;
            doctor.Status = model.Doctor.Status;
            db.SaveChanges();

            return RedirectToAction("ListOfDoctors");
        }

        //Delete Doctor
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteDoctor(string id)
        {
            var UserId = db.Doctors.Single(c => c.ApplicationUserId == id);
            return View(UserId);
        }

        [HttpPost, ActionName("DeleteDoctor")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDoctor(string id, Doctor model)
        {
            var doctor = db.Doctors.Single(c => c.ApplicationUserId == id);
            var user = db.Users.Single(c => c.Id == id);
            if (db.Schedules.Where(c => c.DoctorId == doctor.Id).Equals(null))
            {
                var schedule = db.Schedules.Single(c => c.DoctorId == doctor.Id);
                db.Schedules.Remove(schedule);
            }

            db.Users.Remove(user);
            db.Doctors.Remove(doctor);
            db.SaveChanges();
            return RedirectToAction("ListOfDoctors");
        }

        //End Doctor Section

        //Start Nurse Section

        //Add Nurse 
        [Authorize(Roles = "Admin")]
        public ActionResult AddNurse()
        {
            var collection = new NurseCollection
            {
                ApplicationUser = new RegisterViewModel(),
                Nurse = new Nurse()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddNurse(NurseCollection model)
        {
            var user = new ApplicationUser
            {
                UserName = model.ApplicationUser.UserName,
                Email = model.ApplicationUser.Email,
                UserRole = "Nurse",
                RegisteredDate = DateTime.Now.Date
            };
            var result = await UserManager.CreateAsync(user, model.ApplicationUser.Password);
            if (result.Succeeded)
            {
                await UserManager.AddToRoleAsync(user.Id, "Nurse");
                var nurse = new Nurse
                {
                    FirstName = model.Nurse.FirstName,
                    LastName = model.Nurse.LastName,
                    FullName = model.Nurse.FirstName + " " + model.Nurse.LastName,
                    EmailAddress = model.ApplicationUser.Email,
                    //ContactNo = model.Nurse.ContactNo,
                    PhoneNo = model.Nurse.PhoneNo,
                    //Designation = model.Nurse.Designation,
                    Education = model.Nurse.Education,
                    Gender = model.Nurse.Gender,
                    BloodGroup = model.Nurse.BloodGroup,
                    ApplicationUserId = user.Id,
                    DateOfBirth = model.Nurse.DateOfBirth,
                    Address = model.Nurse.Address,
                    Status = model.Nurse.Status
                };
                db.Nurses.Add(nurse);
                db.SaveChanges();
                return RedirectToAction("ListOfNurses");
            }

            return HttpNotFound();

        }

        //List Of Nurses
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfNurses()
        {
            var nurse = db.Nurses.ToList();
            return View(nurse);
        }

        //Detail of Nurseoles = "Admin")]
        public ActionResult NurseDetail(int id)
        {
            var nurse = db.Nurses.SingleOrDefault(c => c.Id == id);
            return View(nurse);
        }

        //Edit Nurses
        [Authorize(Roles = "Admin")]
        public ActionResult EditNurses(int id)
        {
            var collection = new NurseCollection
            {
                Nurse = db.Nurses.Single(c => c.Id == id)
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNurses(int id, NurseCollection model)
        {
            var nurse = db.Nurses.Single(c => c.Id == id);
            nurse.FirstName = model.Nurse.FirstName;
            nurse.LastName = model.Nurse.LastName;
            nurse.FullName = model.Nurse.FirstName + " " + model.Nurse.LastName;
            //nurse.ContactNo = model.Nurse.ContactNo;
            nurse.PhoneNo = model.Nurse.PhoneNo;
            //nurse.Designation = model.Nurse.Designation;
            nurse.Education = model.Nurse.Education;
            nurse.Gender = model.Nurse.Gender;
            nurse.BloodGroup = model.Nurse.BloodGroup;
            nurse.DateOfBirth = model.Nurse.DateOfBirth;
            nurse.Address = model.Nurse.Address;
            nurse.Status = model.Nurse.Status;
            db.SaveChanges();

            return RedirectToAction("ListOfNurses");
        }

        //Delete Nurse
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteNurse(string id)
        {
            var UserId = db.Nurses.Single(c => c.ApplicationUserId == id);
            return View(UserId);
        }

        [HttpPost, ActionName("DeleteNurse")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteNurse(string id, Nurse model)
        {
            var nurse = db.Nurses.Single(c => c.ApplicationUserId == id);
            var user = db.Users.Single(c => c.Id == id);
            if (db.NurseSchedules.Where(c => c.NurseId == nurse.Id).Equals(null))
            {
                var schedule = db.NurseSchedules.Single(c => c.NurseId == nurse.Id);
                db.NurseSchedules.Remove(schedule);
            }

            db.Users.Remove(user);
            db.Nurses.Remove(nurse);
            db.SaveChanges();
            return RedirectToAction("ListOfNurses");
        }

        //End Nurse Section

        //Start NurseSchedule Section
        //Add NurseSchedule
        [Authorize(Roles = "Admin")]
        public ActionResult AddNurseSchedule()
        {
            var collection = new NurseScheduleCollection
            {
                NurseSchedule = new NurseSchedule(),
                Nurses = db.Nurses.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNurseSchedule(NurseScheduleCollection model)
        {
            if (!ModelState.IsValid)
            {
                var collection = new NurseScheduleCollection
                {
                    NurseSchedule = model.NurseSchedule,
                    Nurses = db.Nurses.ToList()
                };
                return View(collection);
            }

            db.NurseSchedules.Add(model.NurseSchedule);
            db.SaveChanges();
            return RedirectToAction("ListOfNurseSchedules");
        }

        //List Of NurseSchedules
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfNurseSchedules()
        {
            var NurseSchedule = db.NurseSchedules.Include(c => c.Nurse).ToList();
            return View(NurseSchedule);
        }

        //Edit NurseSchedule
        [Authorize(Roles = "Admin")]
        public ActionResult EditNurseSchedule(int id)
        {
            var collection = new NurseScheduleCollection
            {
                NurseSchedule = db.NurseSchedules.Single(c => c.Id == id),
                Nurses = db.Nurses.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNurseSchedule(int id, NurseScheduleCollection model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var NurseSchedule = db.NurseSchedules.Single(c => c.Id == id);
            NurseSchedule.NurseId = model.NurseSchedule.NurseId;
            NurseSchedule.AvailableEndDay = model.NurseSchedule.AvailableEndDay;
            NurseSchedule.AvailableEndTime = model.NurseSchedule.AvailableEndTime;
            NurseSchedule.AvailableStartDay = model.NurseSchedule.AvailableStartDay;
            NurseSchedule.AvailableStartTime = model.NurseSchedule.AvailableStartTime;
            NurseSchedule.Status = model.NurseSchedule.Status;
            
            db.SaveChanges();
            return RedirectToAction("ListOfNurseSchedules");
        }

        //Delete NurseSchedule
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteNurseSchedule(int? id)
        {
            return View();
        }

        [HttpPost, ActionName("DeleteNurseSchedule")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteNurseSchedule(int id)
        {
            var NurseSchedule = db.NurseSchedules.Single(c => c.Id == id);
            db.NurseSchedules.Remove(NurseSchedule);
            db.SaveChanges();
            return RedirectToAction("ListOfNurseSchedules");
        }

        //End NurseSchedule Section



        //Start Receptionist Section

        //Add Receptionist 
        [Authorize(Roles = "Admin")]
        public ActionResult AddReceptionist()
        {
            var collection = new ReceptionistCollection
            {
                ApplicationUser = new RegisterViewModel(),
                Receptionist = new Receptionist()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddReceptionist(ReceptionistCollection model)
        {
            var user = new ApplicationUser
            {
                UserName = model.ApplicationUser.UserName,
                Email = model.ApplicationUser.Email,
                UserRole = "Receptionist",
                RegisteredDate = DateTime.Now.Date
            };
            var result = await UserManager.CreateAsync(user, model.ApplicationUser.Password);
            if (result.Succeeded)
            {
                await UserManager.AddToRoleAsync(user.Id, "Receptionist");
                var nurse = new Receptionist
                {
                    FirstName = model.Receptionist.FirstName,
                    LastName = model.Receptionist.LastName,
                    FullName = model.Receptionist.FirstName + " " + model.Receptionist.LastName,
                    EmailAddress = model.ApplicationUser.Email,
                    //ContactNo = model.Receptionist.ContactNo,
                    PhoneNo = model.Receptionist.PhoneNo,
                    Gender = model.Receptionist.Gender,
                    BloodGroup = model.Receptionist.BloodGroup,
                    ApplicationUserId = user.Id,
                    DateOfBirth = model.Receptionist.DateOfBirth,
                    Address = model.Receptionist.Address,
                    Status = model.Receptionist.Status
                };
                db.Receptionists.Add(nurse);
                db.SaveChanges();
                return RedirectToAction("ListOfReceptionists");
            }

            return HttpNotFound();

        }
        //List Of Receptionists
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfReceptionists()
        {
            var receptionist = db.Receptionists.ToList();
            return View(receptionist);
        }

        //Detail of Receptionist Roles = "Admin")]
        public ActionResult ReceptionistDetail(int id)
        {
            var receptionist = db.Receptionists.SingleOrDefault(c => c.Id == id);
            return View(receptionist);
        }

        //Edit Receptionists
        [Authorize(Roles = "Admin")]
        public ActionResult EditReceptionists(int id)
        {
            var collection = new ReceptionistCollection
            {
                Receptionist = db.Receptionists.Single(c => c.Id == id)
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditReceptionists(int id, ReceptionistCollection model)
        {
            var receptionist = db.Receptionists.Single(c => c.Id == id);
            receptionist.FirstName = model.Receptionist.FirstName;
            receptionist.LastName = model.Receptionist.LastName;
            receptionist.FullName = model.Receptionist.FirstName + " " + model.Receptionist.LastName;
            //receptionist.ContactNo = model.Receptionist.ContactNo;
            receptionist.PhoneNo = model.Receptionist.PhoneNo;
            receptionist.Gender = model.Receptionist.Gender;
            receptionist.BloodGroup = model.Receptionist.BloodGroup;
            receptionist.DateOfBirth = model.Receptionist.DateOfBirth;
            receptionist.Address = model.Receptionist.Address;
            receptionist.Status = model.Receptionist.Status;
            db.SaveChanges();

            return RedirectToAction("ListOfReceptionists");
        }

        //Delete Receptionist
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteReceptionist(string id)
        {
            var UserId = db.Receptionists.Single(c => c.ApplicationUserId == id);
            return View(UserId);
        }

        [HttpPost, ActionName("DeleteReceptionist")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReceptionist(string id, Receptionist model)
        {
            var receptionist = db.Receptionists.Single(c => c.ApplicationUserId == id);
            var user = db.Users.Single(c => c.Id == id);
            if (db.ReceptionistSchedules.Where(c => c.ReceptionistId == receptionist.Id).Equals(null))
            {
                var schedule = db.ReceptionistSchedules.Single(c => c.ReceptionistId == receptionist.Id);
                db.ReceptionistSchedules.Remove(schedule);
            }

            db.Users.Remove(user);
            db.Receptionists.Remove(receptionist);
            db.SaveChanges();
            return RedirectToAction("ListOfReceptionists");
        }

        //End Receptionist Section


        //Start ReceptionistSchedule Section
        //Add ReceptionistSchedule
        [Authorize(Roles = "Admin")]
        public ActionResult AddReceptionistSchedule()
        {
            var collection = new ReceptionistScheduleCollection
            {
                ReceptionistSchedule = new ReceptionistSchedule(),
                Receptionists = db.Receptionists.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddReceptionistSchedule(ReceptionistScheduleCollection model)
        {
            if (!ModelState.IsValid)
            {
                var collection = new ReceptionistScheduleCollection
                {
                    ReceptionistSchedule = model.ReceptionistSchedule,
                    Receptionists = db.Receptionists.ToList()
                };
                return View(collection);
            }

            db.ReceptionistSchedules.Add(model.ReceptionistSchedule);
            db.SaveChanges();
            return RedirectToAction("ListOfReceptionistSchedules");
        }

        //List Of ReceptionistSchedules
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfReceptionistSchedules()
        {
            var ReceptionistSchedule = db.ReceptionistSchedules.Include(c => c.Receptionist).ToList();
            return View(ReceptionistSchedule);
        }

        //Edit ReceptionistSchedule
        [Authorize(Roles = "Admin")]
        public ActionResult EditReceptionistSchedule(int id)
        {
            var collection = new ReceptionistScheduleCollection
            {
                ReceptionistSchedule = db.ReceptionistSchedules.Single(c => c.Id == id),
                Receptionists = db.Receptionists.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditReceptionistSchedule(int id, ReceptionistScheduleCollection model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var ReceptionistSchedule = db.ReceptionistSchedules.Single(c => c.Id == id);
            ReceptionistSchedule.ReceptionistId = model.ReceptionistSchedule.ReceptionistId;
            ReceptionistSchedule.AvailableEndDay = model.ReceptionistSchedule.AvailableEndDay;
            ReceptionistSchedule.AvailableEndTime = model.ReceptionistSchedule.AvailableEndTime;
            ReceptionistSchedule.AvailableStartDay = model.ReceptionistSchedule.AvailableStartDay;
            ReceptionistSchedule.AvailableStartTime = model.ReceptionistSchedule.AvailableStartTime;
            ReceptionistSchedule.Status = model.ReceptionistSchedule.Status;
            
            db.SaveChanges();
            return RedirectToAction("ListOfReceptionistSchedules");
        }

        //Delete ReceptionistSchedule
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteReceptionistSchedule(int? id)
        {
            return View();
        }

        [HttpPost, ActionName("DeleteReceptionistSchedule")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReceptionistSchedule(int id)
        {
            var ReceptionistSchedule = db.ReceptionistSchedules.Single(c => c.Id == id);
            db.ReceptionistSchedules.Remove(ReceptionistSchedule);
            db.SaveChanges();
            return RedirectToAction("ListOfReceptionistSchedules");
        }

        //End ReceptionistSchedule Section




        //Start Doctor Schedule Section
        //Add Schedule
        [Authorize(Roles = "Admin")]
        public ActionResult AddSchedule()
        {
            var collection = new ScheduleCollection
            {
                Schedule = new Schedule(),
                Doctors = db.Doctors.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSchedule(ScheduleCollection model)
        {
            if (!ModelState.IsValid)
            {
                var collection = new ScheduleCollection
                {
                    Schedule = model.Schedule,
                    Doctors = db.Doctors.ToList()
                };
                return View(collection);
            }

            db.Schedules.Add(model.Schedule);
            db.SaveChanges();
            return RedirectToAction("ListOfSchedules");
        }

        //List Of Schedules
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfSchedules()
        {
            var schedule = db.Schedules.Include(c => c.Doctor).ToList();
            return View(schedule);
        }

        //Edit Schedule
        [Authorize(Roles = "Admin")]
        public ActionResult EditSchedule(int id)
        {
            var collection = new ScheduleCollection
            {
                Schedule = db.Schedules.Single(c => c.Id == id),
                Doctors = db.Doctors.ToList()
            };
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSchedule(int id, ScheduleCollection model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var schedule = db.Schedules.Single(c => c.Id == id);
            schedule.DoctorId = model.Schedule.DoctorId;
            schedule.AvailableEndDay = model.Schedule.AvailableEndDay;
            schedule.AvailableEndTime = model.Schedule.AvailableEndTime;
            schedule.AvailableStartDay = model.Schedule.AvailableStartDay;
            schedule.AvailableStartTime = model.Schedule.AvailableStartTime;
            schedule.Status = model.Schedule.Status;
            schedule.TimePerPatient = model.Schedule.TimePerPatient;
            db.SaveChanges();
            return RedirectToAction("ListOfSchedules");
        }

        //Delete Schedule
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteSchedule(int? id)
        {
            return View();
        }

        [HttpPost, ActionName("DeleteSchedule")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSchedule(int id)
        {          
            var schedule = db.Schedules.Single(c => c.Id == id);
            db.Schedules.Remove(schedule);
            db.SaveChanges();
            return RedirectToAction("ListOfSchedules");
        }

        //End Schedule Section

        //Start Patient Section

        //List of Patients
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfPatients()
        {
            var patients = db.Patients.ToList();

            return View(patients);
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        //Start Appointment Section

        //Add Appointment
        [Authorize(Roles = "Admin")]
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

        //List of Active Appointment
        [Authorize(Roles = "Admin")]
        public ActionResult ListOfAppointments()
        {
            var date = DateTime.Now.Date;
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient)
                .Where(c => c.Status == true).Where(c => c.AppointmentDate >= date).ToList();
            return View(appointment);
        }

        //List of pending Appointments
        [Authorize(Roles = "Admin")]
        public ActionResult PendingAppointments()
        {
            var date = DateTime.Now.Date;
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient)
                .Where(c => c.Status == false).Where(c => c.AppointmentDate >= date).ToList();
            return View(appointment);
        }

        //Edit Appointment
        [Authorize(Roles = "Admin")]
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

        //Detail of Appointment
        [Authorize(Roles = "Admin")]
        public ActionResult DetailOfAppointment(int id)
        {
            var appointment = db.Appointments.Include(c => c.Doctor).Include(c => c.Patient).Single(c => c.Id == id);
            return View(appointment);
        }

        //Delete Appointment
        [Authorize(Roles = "Admin")]
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

        //End Appointment Section        
    }
}