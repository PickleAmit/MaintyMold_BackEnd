using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication1.DTO;
using DATA;
using System.Data.Entity;

namespace WebApplication1.Controllers
{
    public class EmployeeController : ApiController
    {
        NetafimDbContext db = new NetafimDbContext();

        [HttpPost]
        [Route("api/employee/authenticate")]
        public IHttpActionResult AuthenticateLogIn(Credentials credentials)
        {
            try
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
            catch (Exception ex) { return InternalServerError(ex); }

        }


        // not using ? 
        //[HttpGet]
        //[Route("api/locations1")]
        //public IHttpActionResult GetLocations()
        //{
        //    try
        //    {
        //        var locations = db.Locations
        //            .Join(
        //                db.Molds,
        //                location => location.LocationCode,
        //                mold => mold.LocationCode,
        //                (location, mold) => new { Location = location, Mold = mold }
        //            )
        //            .Join(
        //                db.Errors,
        //                locMold => locMold.Mold.MoldID,
        //                error => error.MoldID,
        //                (locMold, error) => new { Location = locMold.Location, Mold = locMold.Mold, Error = error }
        //            )
        //            .GroupBy(lme => lme.Mold.MoldID)
        //            .Select(g => g.OrderByDescending(lme => lme.Error.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().Date).FirstOrDefault())
        //            .Select(lme => new
        //            {
        //                LocationCode = lme.Location.LocationCode,
        //                LocationName = lme.Location.LocationName,
        //                MoldID = lme.Mold.MoldID,
        //                StatusType = lme.Error.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().StatusType
        //            })
        //            .ToList();

        //        return Ok(locations);
        //    }
        //    catch (Exception ex) { return InternalServerError(ex); }

        //}

        [HttpGet]
        [Route("api/alllocations")]
        public IHttpActionResult GetAllLocations()
        {
            try
            {
                var locations = db.Locations
                    .Select(l => new { l.LocationCode, l.LocationName })
                    .ToList();

                return Ok(locations);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [Route("api/locations/new")]
        public IHttpActionResult AddLocation([FromBody] LocationModel locationModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(locationModel.LocationName))
                {
                    return BadRequest("Location name cannot be empty");
                }

                var existingLocation = db.Locations.FirstOrDefault(l => l.LocationName == locationModel.LocationName);
                if (existingLocation != null)
                {
                    return BadRequest("Location already exists");
                }

                var newLocation = new Location
                {
                    LocationName = locationModel.LocationName
                };

                db.Locations.Add(newLocation);
                db.SaveChanges();

                return Created($"api/locations/{newLocation.LocationCode}", newLocation);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // Update the location in the mold table
        [HttpPut]
        [Route("api/molds/{moldId}/location")]
        public IHttpActionResult UpdateMoldLocation(int moldId, LocationModel locationModel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(locationModel.LocationName))
                {
                    return BadRequest("Location name cannot be empty");
                }

                System.Diagnostics.Debug.WriteLine("Finding location...");
                var location = db.Locations.FirstOrDefault(l => l.LocationName == locationModel.LocationName);
                if (location == null)
                {
                    return BadRequest("Location not found");
                }

                System.Diagnostics.Debug.WriteLine("Finding mold...");
                var mold = db.Molds.FirstOrDefault(m => m.MoldID == moldId);
                if (mold == null)
                {
                    return BadRequest("Mold not found");
                }

                System.Diagnostics.Debug.WriteLine("Updating mold location...");
                mold.LocationCode = location.LocationCode;
                db.Entry(mold).State = EntityState.Modified;

                System.Diagnostics.Debug.WriteLine("Saving changes...");
                db.SaveChanges();

                return Ok(mold);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                return InternalServerError(ex);
            }
        }




        public class LocationModel
        {
            public string LocationName { get; set; }
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
