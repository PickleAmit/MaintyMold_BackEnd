using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.DTO;
using DATA;

namespace WebApplication1.Controllers
{
    public class EmployeeController : ApiController
    {
        NetafimDbContext db = new NetafimDbContext();

        [HttpPost]
        [Route("api/employee/authenticate")]
        public IHttpActionResult AuthenticateLogIn(Credentials credentials)
        {
            if (credentials == null || string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
            {
                return BadRequest("Invalid credentials.");
            }

            var employee = db.Employees.FirstOrDefault(e => e.Email == credentials.Email);

            if (employee == null)
            {
                return NotFound();
            }

            if (credentials.Password != employee.Password)
            {
                return BadRequest("Incorrect password.");
            }

            var employeeDto = MapToDto(employee);
            return Ok(employeeDto);
        }

        [HttpGet]
        [Route("api/locations")]
        public IEnumerable<Location> GetLocations()
        {
            return db.Locations.ToList();
        }

        // POST: api/Employee
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Employee/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Employee/5
        public void Delete(int id)
        {
        }

        private EmployeeDto MapToDto(DATA.Employee employee)
        {
            return new EmployeeDto
            {
                EmployeeID = employee.EmployeeNumber,
                Name = employee.FirstName + " " + employee.LastName,
                AreaOfExpertise = employee.AreaOfExpertise,
            };
        }
    }
}
