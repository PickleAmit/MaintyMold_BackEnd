using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DATA;

namespace WebApplication1.Controllers
{
    public class MockDataController : ApiController
    {
        NetafimDbContext db = new NetafimDbContext();

        [HttpPost]
        [Route("api/mockdata")]
        public IHttpActionResult GenerateMockData()
        {
            try
            {
                for (int i = 0; i < 100; i++) 
                {
                    AddSpecificError();
                }
                return Ok("Success!");
            }
            catch (Exception ex) { return InternalServerError(ex); }
        }

        private void AddNewError()
        {
            try {
            string[] descriptions = { "תקלת סגר דריפנט", "תקלה בשסתום הוויפיי", "נזילה בתבנית", "תקלת חדירת שמן" };
            string[] errorTypes = { "יזום", "שבר", "שבר", "שבר" };
            int[] technicianIDs = { 2, 4, 6, 1002 };
            int[] priorityIDs = { 1, 2, 3 };
            int[] moldIDs = { 1, 2, 3, 4, 5 };

            Random random = new Random();

            DateTime openingDateTime = DateTime.Now.AddMonths(random.Next(-12, 0));

            Error error = new Error
            {
                Description = descriptions[random.Next(descriptions.Length)],
                ErrorType = errorTypes[random.Next(errorTypes.Length)],
                TechnicianID = technicianIDs[random.Next(technicianIDs.Length)],
                OpeningDate = openingDateTime,
                ClosingDate = null,
                TreatmentHours = null,
                PriorityID = priorityIDs[random.Next(priorityIDs.Length)],
                MoldID = moldIDs[random.Next(moldIDs.Length)],
                LeadingTechnicianID = technicianIDs[random.Next(technicianIDs.Length)],
            };
                Console.WriteLine(error);

                db.Errors.Add(error);
                db.SaveChanges();

                StatusError statusError = new StatusError
            {
                Name = "תקלה חדשה",
                Time = openingDateTime.AddHours(random.Next(9, 18)),
                Date = openingDateTime,
                Description = descriptions[random.Next(descriptions.Length)],
                MoldRoomTechnicianNumber = technicianIDs[random.Next(technicianIDs.Length)],
                ErrorNumber = error.ErrorNumber,
                StatusType = "Waiting for treatment"
            };

                db.StatusErrors.Add(statusError);
                db.SaveChanges();


                DateTime closingDateTime = openingDateTime.AddDays(random.Next(1, 7));

            StatusError closingStatusError = new StatusError
            {
                Name = "סגירת תקלה",
                Time = closingDateTime.AddHours(random.Next(9, 18)),
                Date = closingDateTime,
                Description = "סיום טיפול",
                MoldRoomTechnicianNumber = error.LeadingTechnicianID,
                ErrorNumber = error.ErrorNumber,
                StatusType = "Finished"
            };
                Error errorToUpdate = db.Errors.Find(error.ErrorNumber);
                if (errorToUpdate != null)
                {
                    errorToUpdate.ClosingDate = closingDateTime;
                    errorToUpdate.TreatmentHours = (decimal?)(closingDateTime - openingDateTime).TotalHours;
                    db.Entry(errorToUpdate).State = System.Data.Entity.EntityState.Modified;
                }

                //db.Errors.Add(error);
                db.StatusErrors.Add(closingStatusError);
            db.SaveChanges();
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
                //string location = Url.Route("DefaultApi", new { controller = "Error", id = error.ErrorNumber });
        }

        private void AddSpecificError()
        {
            try
            {
                // technician 4 best for תקלת חדירת שמן
                // technician 2 best for תקלת סגר דריפנט
                // descriptions - "תקלת סגר דריפנט", "תקלה בשסתום הוויפיי", "נזילה בתבנית", "תקלת חדירת שמן"
                // technicians - 2, 4, 6, 1002
                string description = "נזילה בתבנית";
                string errorType = "שבר";
                int technicianID = 6; // example technician ID
                int priorityID = 3; // priority 3
                int moldID = 3; // example mold ID

                Random random = new Random();
                DateTime openingDateTime = DateTime.Now;

                Error error = new Error
                {
                    Description = description,
                    ErrorType = errorType,
                    TechnicianID = technicianID,
                    OpeningDate = openingDateTime,
                    ClosingDate = null,
                    TreatmentHours = null,
                    PriorityID = priorityID,
                    MoldID = moldID,
                    LeadingTechnicianID = technicianID
                };

                Console.WriteLine(error);

                db.Errors.Add(error);
                db.SaveChanges();

                StatusError statusError = new StatusError
                {
                    Name = "תקלה חדשה",
                    Time = openingDateTime.AddHours(random.Next(1, 3)),
                    Date = openingDateTime,
                    Description = description,
                    MoldRoomTechnicianNumber = technicianID,
                    ErrorNumber = error.ErrorNumber,
                    StatusType = "Waiting for treatment"
                };

                db.StatusErrors.Add(statusError);
                db.SaveChanges();

                DateTime closingDateTime = openingDateTime.AddDays(random.Next(1,2)); // max 2 days after the 'openingDateTime'

                StatusError closingStatusError = new StatusError
                {
                    Name = "סגירת תקלה",
                    Time = closingDateTime.AddHours(random.Next(9,18)), // for example 1 hour after closing
                    Date = closingDateTime,
                    Description = "סיום טיפול",
                    MoldRoomTechnicianNumber = technicianID,
                    ErrorNumber = error.ErrorNumber,
                    StatusType = "Finished"
                };

                Error errorToUpdate = db.Errors.Find(error.ErrorNumber);
                if (errorToUpdate != null)
                {
                    errorToUpdate.ClosingDate = closingDateTime;
                    errorToUpdate.TreatmentHours = (decimal?)(closingDateTime - openingDateTime).TotalHours;
                    db.Entry(errorToUpdate).State = System.Data.Entity.EntityState.Modified;
                }

                db.StatusErrors.Add(closingStatusError);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
