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
        public IHttpActionResult GetLocations()
        {
            var locations = db.Locations
                .Join(
                    db.Molds,
                    location => location.LocationCode,
                    mold => mold.LocationCode,
                    (location, mold) => new { Location = location, Mold = mold }
                )
                .Join(
                    db.Errors,
                    locMold => locMold.Mold.MoldID,
                    error => error.MoldID,
                    (locMold, error) => new { Location = locMold.Location, Mold = locMold.Mold, Error = error }
                )
                .GroupBy(lme => lme.Mold.MoldID)
                .Select(g => g.OrderByDescending(lme => lme.Error.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().Date).FirstOrDefault())
                .Select(lme => new
                {
                    LocationCode = lme.Location.LocationCode,
                    LocationName = lme.Location.LocationName,
                    MoldID = lme.Mold.MoldID,
                    StatusType = lme.Error.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().StatusType
                })
                .ToList();

            return Ok(locations);
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
