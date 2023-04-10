using DATA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class ErrorController : ApiController
    {
        NetafimDbContext db = new NetafimDbContext();


        //Gets all of the errors
        [HttpGet]
        [Route("api/error")]
        public IHttpActionResult Get()
        {
            var errors = db.Errors.Include(e => e.Mold.Location).Include(e => e.Priority).ToList(); // Retrieve errors from the database first with necessary includes

            var errorsWithDetails = errors
                .Select((error, index) => new
                {
                    RowNumber = index + 1,
                    ErrorNumber = error.ErrorNumber,
                    Description = error.Description,
                    ErrorType = error.ErrorType,
                    TreatmentHours = error.TreatmentHours,
                    TechnicianID = error.TechnicianID,
                    MoldID = error.MoldID,
                    LocationName = error.Mold?.Location?.LocationName,
                    PriorityDescription = error.Priority?.Description,
                    // ... other fields you want to return
                })
                .ToList();

            return Ok(errorsWithDetails);
        }

        //Gets specific error
        [HttpGet]
        [Route("api/error/{id}")]
        public IHttpActionResult Get(int id)
        {
            var error = db.Errors.Find(id);
            if (error == null)
            {
                return NotFound();
            }

            //Add StatusError information
            var statusErrors = db.StatusErrors
                .Where(se => se.ErrorNumber == id)
                .Select(se => new
                {
                    Name = se.Name,
                    Time = se.Time,
                    Date = se.Date,
                    Description = se.Description,
                    MoldRoomTechnicianNumber = se.MoldRoomTechnicianNumber,
                    ErrorNumber = se.ErrorNumber,
                    StatusType = se.StatusErrorStageEnum.StatusType
                }).ToArray();

            var result = new
            {
                Error = new
                {
                    ErrorNumber = error.ErrorNumber,
                    OpeningDate = error.OpeningDate,
                    ClosingDate = error?.ClosingDate,
                    TreatmentHours = error?.TreatmentHours,
                    Description = error.Description,
                    ErrorType = error.ErrorType,
                    TechnicianID = error.TechnicianID,
                    PriorityID = error.PriorityID,
                    MoldID = error.MoldID
                },
                StatusErrors = statusErrors
            };
            return Ok(result);
        }

        // Creates new error and Updates the StatusType to be 'waiting for treatment'
        [HttpPost]
        [Route("api/error/new")]
        public IHttpActionResult AddError(AddErrorModel model)
        {
            string description = model.Description;
            string type = model.Type;
            int technicianId = model.TechnicianID;
            int moldID = model.MoldID;

            if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(type))
            {
                return BadRequest("Description and Type cannot be empty.");
            }

            // Create a new Error object and set the properties
            Error error = new Error
            {
                Description = description,
                ErrorType = type,
                TechnicianID = technicianId,
                OpeningDate = DateTime.Now,
                ClosingDate = null,
                TreatmentHours = null,
                PriorityID = null,
                MoldID = moldID,
            };

            // Add the new Error object to the database and save the changes
            db.Errors.Add(error);
            db.SaveChanges();

            StatusError statusError = new StatusError
            {
                Name = "תקלה חדשה",
                Time = DateTime.Now,
                Date = DateTime.Now,
                Description = null,
                MoldRoomTechnicianNumber = null,
                ErrorNumber = error.ErrorNumber,
                StatusType = "Waiting for treatment"
            };

            db.StatusErrors.Add(statusError);
            db.SaveChanges();

            string location = Url.Route("DefaultApi", new { controller = "Error", id = error.ErrorNumber });
            return Created(new Uri(Request.RequestUri, location), error);
        }

        // Adds priority to error
        [HttpPut]
        [Route("api/error/{errorNumber}/priority")]
        public IHttpActionResult UpdatePriority(int errorNumber, [FromBody] UpdatePriorityModel model)
        {
            var error = db.Errors.Find(errorNumber);
            if (error == null)
            {
                return NotFound();
            }

            var priority = new Priority
            {
                Description = model.Description,
                ManagerID = model.ManagerID
            };

            db.Priorities.Add(priority);
            db.SaveChanges();

            error.PriorityID = priority.PriorityID;
            db.Entry(error).State = EntityState.Modified;
            db.SaveChanges();

            return Ok();
        }

        public class UpdatePriorityModel
        {
            public string Description { get; set; }
            public int ManagerID { get; set; }
        }


        // Adds new statusError to an Error and sets the StatusType to - 'In treatment'
        [HttpPost]
        [Route("api/error/{errorNumber}/status")]
        public IHttpActionResult AddStatusError(int errorNumber, AddStatusErrorModel model)
        {
            //Check if the specified error exists
            var error = db.Errors.Find(errorNumber);
            if (error == null)
            {
                return NotFound();
            }

            //Create a new StatusError obj and set the properties
            StatusError statusError = new StatusError
            {
                Name = model.Name,
                Time = DateTime.Now,
                Date = DateTime.Now,
                Description = model.Description,
                MoldRoomTechnicianNumber = model.MoldRoomTechnicianNumber,
                ErrorNumber = errorNumber,
                StatusType = "In treatment"
            };

            //Add the new StatusError object to the database 
            db.StatusErrors.Add(statusError);
            db.SaveChanges();


            string location = Url.Route("DefaultApi", new { controller = "StatusError", id = statusError.ErrorStatusID });
            return Created(new Uri(Request.RequestUri, location), statusError);
        }

        // Closes a specific error and sets the StatusType to - 'Finished'
        [HttpPut]
        [Route("api/error/{errorNumber}/close")]
        public IHttpActionResult CloseError(int errorNumber, AddStatusErrorModel model)
        {
            // Check if the specified error exists
            var error = db.Errors.Find(errorNumber);
            if (error == null)
            {
                return NotFound();
            }

            // Set the ClosingDate and calculate the TreatmentHours
            error.ClosingDate = DateTime.Now;
            error.TreatmentHours = (decimal)(error.ClosingDate - error.OpeningDate).GetValueOrDefault().TotalHours;

            // Create a new StatusError object and set the properties
            StatusError statusError = new StatusError
            {
                Name = model.Name,
                Time = DateTime.Now,
                Date = DateTime.Now,
                Description = model.Description,
                MoldRoomTechnicianNumber = model.MoldRoomTechnicianNumber,
                ErrorNumber = errorNumber,
                StatusType = "Finished"
            };

            // Add the new StatusError object to the database
            db.StatusErrors.Add(statusError);

            // Save the changes
            db.SaveChanges();

            return Ok(error);
        }


        public class AddStatusErrorModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int MoldRoomTechnicianNumber { get; set; }
        }

    }
}
