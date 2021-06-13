using EmployeeMVC01.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeMVC01.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly IConfiguration Configuration;

        [BindProperty]
        public Employee Employee { get; set; }

        public EmployeesController(IConfiguration configuration, ApplicationDBContext db)
        {
            Configuration = configuration;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            Employee = new Employee();
            if (id == null)
            {
                //create
                return View(Employee);
            }
            //update
            Employee = _db.Employees.FirstOrDefault(u => u.Id == id);
            if (Employee == null)
            {
                return NotFound();
            }
            return View(Employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert()
        {
            if (ModelState.IsValid)
            {
                if (Employee.Id == 0)
                {
                    //create
                    _db.Employees.Add(Employee);
                }
                else
                {
                    _db.Employees.Update(Employee);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(Employee);
        }


        public IActionResult EmployeeToCallRESTAPI()
        {
            return View();
        }

        public IActionResult UpsertToCallRESTAPI(int? id)
        {
            Employee employee = new Employee();
            if (id == null)
            {
                return View(employee);
            }
            //update
            employee = _db.Employees.FirstOrDefault(u => u.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            using (var client = new HttpClient())
            {
                var responseTask = client.GetAsync(Configuration["RESTAPI:Url"] + "/" + id.ToString());
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    employee = JsonConvert.DeserializeObject<Employee>(readTask.Result);
                }
            }
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpsertToCallRESTAPI()
        {
            if (ModelState.IsValid)
            {
                if (Employee.Id == 0)
                {
                    using (var client = new HttpClient())
                    {
                        var employee = Newtonsoft.Json.JsonConvert.SerializeObject(Employee);
                        HttpContent content = new StringContent(employee, Encoding.UTF8, "application/json");
                        var postTask = client.PostAsync(Configuration["RESTAPI:Url"], content);
                        postTask.Wait();
                    }
                }
                else
                {
                    using (var client = new HttpClient())
                    {
                        var employee = Newtonsoft.Json.JsonConvert.SerializeObject(Employee);
                        HttpContent content = new StringContent(employee, Encoding.UTF8, "application/json");
                        var postTask = client.PutAsync(Configuration["RESTAPI:Url"] + "/" + Employee.Id.ToString(), content);
                        postTask.Wait();
                    }
                }
                return RedirectToAction("EmployeeToCallRESTAPI");
            }
            return View(Employee);
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _db.Employees.ToListAsync() }); ;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var EmployeeFromDb = await _db.Employees.FirstOrDefaultAsync(u => u.Id == id);
            if (EmployeeFromDb == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            _db.Employees.Remove(EmployeeFromDb);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion

        #region External REST API Calls
        [HttpGet]
        public async Task<IActionResult> GetAllToCallRESTAPI()
        {
            using (var client = new HttpClient())
            {
                var responseTask = client.GetAsync(Configuration["RESTAPI:Url"]);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    List<Employee> employees = JsonConvert.DeserializeObject<List<Employee>>(readTask.Result);

                    var data = Json(new { data = employees });
                    return data;

                }
                else
                {
                    return null;
                }
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteToCallRESTAPI(int id)
        {
            using (var client = new HttpClient())
            {
                var responseTask = client.DeleteAsync(Configuration["RESTAPI:Url"] + "/" + id.ToString());
                responseTask.Wait();
            }
            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion        

    }
}
