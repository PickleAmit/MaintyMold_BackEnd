using DATA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
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
            try
            {
                var errorsWithLatestStatus = db.Errors.Include(e => e.Mold.Location)
                                                      .Include(e => e.Priority)
                                                      .Join(db.StatusErrors,
                                                            error => error.ErrorNumber,
                                                            status => status.ErrorNumber,
                                                            (error, status) => new { Error = error, Status = status })
                                                      .GroupBy(x => x.Error.ErrorNumber)
                                                      .Select(group => new
                                                      {
                                                          Error = group.FirstOrDefault().Error,
                                                          LatestStatus = group.OrderByDescending(x => x.Status.Date).FirstOrDefault().Status.StatusType
                                                      })
                                                      .ToList();

                var errorsWithDetails = errorsWithLatestStatus
                    .Select((x, index) => new
                    {
                        RowNumber = index + 1,
                        ErrorNumber = x.Error.ErrorNumber,
                        Description = x.Error.Description,
                        ErrorType = x.Error.ErrorType,
                        OpeningDate = x.Error.OpeningDate,
                        ClosingDate = x.Error?.ClosingDate,
                        TreatmentHours = x.Error.TreatmentHours,
                        TechnicianID = x.Error.TechnicianID,
                        TechnicianName = x.Error.Technician.Employee.FirstName + " " + x.Error.Technician.Employee.LastName,
                        MoldID = x.Error.MoldID,
                        MoldDesc = x.Error.Mold.MoldDescription,
                        LocationName = x.Error.Mold?.Location?.LocationName,
                        PriorityDescription = x.Error.Priority?.Description,
                        ErrorStatus = x.LatestStatus,
                        // ... other fields you want to return
                    })
                    .ToList();

                return Ok(errorsWithDetails);
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }


        // Gets all the errors with StatusType 'Waiting for treatment'
        [HttpGet]
        [Route("api/error/waitingfortreatment")]
        public IHttpActionResult GetErrorsWaitingForTreatment()
        {
            try
            {
                var errors = db.Errors
                    .Where(e => e.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().StatusType == "Waiting for treatment")
                    .ToList();

                var errorsWithDetails = errors
                .Select((error, index) => new
                {
                    RowNumber = index + 1,
                    ErrorNumber = error.ErrorNumber,
                    Description = error.Description,
                    ErrorType = error.ErrorType,
                    TreatmentHours = error.TreatmentHours,
                    TechnicianID = error.TechnicianID,
                    TechnicianName = error.Technician.Employee.FirstName + " " + error.Technician.Employee.LastName,
                    MoldID = error.MoldID,
                    LocationName = error.Mold?.Location?.LocationName,
                    PriorityDescription = error.Priority?.Description,
                    StatusType = error.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().StatusType
                    // ... other fields you want to return
                })
                .ToList();
                return Ok(errorsWithDetails);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }

        // Gets all the errors with StatusType 'In Treatment'
        [HttpGet]
        [Route("api/error/intreatment")]
        public IHttpActionResult GetErrorsInTreatment()
        {
            try
            {
                var errors = db.Errors
                    .Where(e => e.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().StatusType == "In treatment")
                    .ToList();

                var errorsWithDetails = errors
                    .Select((error, index) => new
                    {
                        RowNumber = index + 1,
                        ErrorNumber = error.ErrorNumber,
                        Description = error.Description,
                        ErrorType = error.ErrorType,
                        TreatmentHours = error.TreatmentHours,
                        TechnicianID = error.TechnicianID,
                        TechnicianName = error.Technician.Employee.FirstName + " " + error.Technician.Employee.LastName,
                        MoldID = error.MoldID,
                        LocationName = error.Mold?.Location?.LocationName,
                        PriorityDescription = error.Priority?.Description,
                        StatusType = error.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().StatusType,
                    })
                    .ToList();

                return Ok(errorsWithDetails);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }

        // Gets all the errors with StatusType 'Finished'
        [HttpGet]
        [Route("api/error/finished")]
        public IHttpActionResult GetErrorsFinished()
        {
            try
            {
                var errors = db.Errors
                    .Where(e => e.StatusErrors.OrderByDescending(s => s.Date).FirstOrDefault().StatusType == "finished")
                    .ToList();

                var errorsWithDetails = errors
                    .Select((error, index) => new
                    {
                        RowNumber = index + 1,
                        ErrorNumber = error.ErrorNumber,
                        Description = error.Description,
                        ErrorType = error.ErrorType,
                        OpeningDate = error.OpeningDate,
                        ClosingDate = error.ClosingDate,
                        TreatmentHours = error.TreatmentHours,
                        TechnicianID = error.TechnicianID,
                        TechnicianName = error.Technician.Employee.FirstName + " " + error.Technician.Employee.LastName,
                        MoldID = error.MoldID,
                        LocationName = error.Mold?.Location?.LocationName,
                        PriorityDescription = error.Priority?.Description,
                        StatusType = error.StatusErrors.OrderByDescending(s => s.Date).LastOrDefault().StatusType,
                    })
                    .ToList();

                return Ok(errorsWithDetails);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }

        //Gets specific error
        [HttpGet]
        [Route("api/error/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
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
                        StatusDescription = se.Description,
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
                        ErrorDescription = error.Description,
                        ErrorType = error.ErrorType,
                        TechnicianID = error.TechnicianID,
                        TechnicianName = error.Technician.Employee.FirstName + " " + error.Technician.Employee.LastName,
                        PriorityID = error.PriorityID,
                        PriorityName = error.Priority.Description,
                        MoldID = error.MoldID,
                        MoldDescription = error.Mold.MoldDescription
                    },
                    StatusErrors = statusErrors
                };
                return Ok(result);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }

        // Creates new error and Updates the StatusType to be 'waiting for treatment'
        [HttpPost]
        [Route("api/error/new")]
        public IHttpActionResult AddError(AddErrorModel model)
        {
            try
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
                    LeadingTechnicianID = null,
                };

                if (model.ErrorPicture != null && model.ErrorPicture.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(model.ErrorPicture.InputStream))
                    {
                        error.ErrorPicture = binaryReader.ReadBytes(model.ErrorPicture.ContentLength);
                    }
                }

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
            catch (Exception ex) { return InternalServerError(ex); }

        }

        // Adds priority to error
        [HttpPut]
        [Route("api/error/{errorNumber}/priority")]
        public IHttpActionResult UpdatePriority(int errorNumber, [FromBody] UpdatePriorityModel model)
        {
            try
            {
                var error = db.Errors.Find(errorNumber);
                if (error == null)
                {
                    return NotFound();
                }

                // Find the priority based on the description
                var priority = db.Priorities.FirstOrDefault(p => p.Description == model.Description);
                var leadingTechnician = db.MoldRoomTechnicians.FirstOrDefault(m => m.EmployeeNumber == model.LeadingTechnicianId);

                if (priority == null || leadingTechnician == null)
                {
                    return BadRequest("Invalid priority description or invalid leadingTEchnicianID");
                }


                // Update the error with the found priorityID

                error.PriorityID = priority.PriorityID;
                error.LeadingTechnicianID = leadingTechnician.EmployeeNumber;
                db.Entry(error).State = EntityState.Modified;
                db.SaveChanges();

                return Ok(error.Technician.Employee.FirstName + " " + error.Technician.Employee.LastName);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }

        public class UpdatePriorityModel
        {
            public string Description { get; set; }
            public int LeadingTechnicianId { get; set; }
        }


        // Adds new statusError to an Error and sets the StatusType to - 'In treatment'
        [HttpPost]
        [Route("api/error/{errorNumber}/status")]
        public IHttpActionResult AddStatusError(int errorNumber, AddStatusErrorModel model)
        {
            try
            {
                //Check if the specified error exists
                var error = db.Errors.Find(errorNumber);
                if (error == null)
                {
                    return NotFound();
                }

                // Update the Mold table based on the MoldDescription
                var mold = db.Molds.FirstOrDefault(m => m.MoldID == error.MoldID);
                if (mold != null)
                {
                    mold.LastTreatmentDate = DateTime.Now;
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
            catch (Exception ex) { return InternalServerError(ex); }

        }

        // Closes a specific error and sets the StatusType to - 'Finished'
        [HttpPut]
        [Route("api/error/{errorNumber}/close")]
        public IHttpActionResult CloseError(int errorNumber, AddStatusErrorModel model)
        {
            try
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


                // Update the Mold table based on the MoldDescription
                var mold = db.Molds.FirstOrDefault(m => m.MoldID == error.MoldID);
                if (mold != null)
                {
                    mold.LastTreatmentDate = error.ClosingDate;
                    mold.HourOfLastTreatment = (int)error.TreatmentHours;
                }


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
            catch (Exception ex) { return InternalServerError(ex); }

        }


        public class AddStatusErrorModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int MoldRoomTechnicianNumber { get; set; }
        }

    }
}
