using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DATA;

namespace WebApplication1.Controllers
{
    public class ManagerController : ApiController
    {
        NetafimDbContext db = new NetafimDbContext();

        // Counts the errors by type שבר/יזום
        [HttpGet]
        [Route("api/errors/count-by-type")]
        public IHttpActionResult GetErrorCountByType(DateTime? endDate = null, string timePeriod = "all")
        {
            try
            {
                Tuple<DateTime, DateTime> dates = GetStartAndEndDate(endDate, timePeriod);
                DateTime actualStartDate = dates.Item1;
                DateTime actualEndDate = dates.Item2;

                var errorsByType = db.Errors
                    .Where(e => e.OpeningDate >= actualStartDate && e.OpeningDate <= actualEndDate)
                    .GroupBy(e => e.ErrorType)
                    .Select(group => new
                    {
                        ErrorType = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                return Ok(errorsByType);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // Count errors assigned to each technician
        [HttpGet]
        [Route("api/errors/count-by-technician")]
        public IHttpActionResult GetErrorCountByTechnician(DateTime? endDate = null, string timePeriod = "all")
        {
            try
            {
                Tuple<DateTime, DateTime> dates = GetStartAndEndDate(endDate, timePeriod);
                DateTime actualStartDate = dates.Item1;
                DateTime actualEndDate = dates.Item2;

                var errorsByTechnician = db.Errors
                    .Where(e => e.OpeningDate >= actualStartDate && e.OpeningDate <= actualEndDate)
                    .GroupBy(e => e.Technician.Employee.FirstName + " " + e.Technician.Employee.LastName)
                    .Select(group => new
                    {
                        TechnicianName = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                return Ok(errorsByTechnician);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("api/errors/average-resolution-time")]
        public IHttpActionResult GetAverageResolutionTime(DateTime? endDate = null, string timePeriod = "all")
        {
            try
            {
                Tuple<DateTime, DateTime> dates = GetStartAndEndDate(endDate, timePeriod);
                DateTime actualStartDate = dates.Item1;
                DateTime actualEndDate = dates.Item2;

                var averageResolutionTime = db.Errors
                    .Where(e => e.OpeningDate >= actualStartDate && e.OpeningDate <= actualEndDate && e.ClosingDate != null)
                    .Average(e => DbFunctions.DiffHours(e.OpeningDate, e.ClosingDate));

                return Ok(new { AverageResolutionTime = averageResolutionTime });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("api/errors/count-by-priority")]
        public IHttpActionResult GetErrorCountByPriority(DateTime? endDate = null, string timePeriod = "all")
        {
            try
            {
                Tuple<DateTime, DateTime> dates = GetStartAndEndDate(endDate, timePeriod);
                DateTime actualStartDate = dates.Item1;
                DateTime actualEndDate = dates.Item2;

                var errorsByPriority = db.Errors
                    .Where(e => e.OpeningDate >= actualStartDate && e.OpeningDate <= actualEndDate)
                    .GroupBy(e => e.Priority.Description)
                    .Select(group => new
                    {
                        Priority = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                return Ok(errorsByPriority);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("api/errors/most-common")]
        public IHttpActionResult GetMostCommonErrors(DateTime? endDate = null, string timePeriod = "all")
        {
            try
            {
                Tuple<DateTime, DateTime> dates = GetStartAndEndDate(endDate, timePeriod);
                DateTime actualStartDate = dates.Item1;
                DateTime actualEndDate = dates.Item2;

                var mostCommonErrors = db.Errors
                    .Where(e => e.OpeningDate >= actualStartDate && e.OpeningDate <= actualEndDate)
                    .GroupBy(e => e.Description)
                    .Select(group => new
                    {
                        Description = group.Key,
                        Count = group.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5) // adjust as needed
                    .ToList();

                return Ok(mostCommonErrors);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("api/errors/average-resolution-time-by-errortype")]
        public IHttpActionResult GetAverageResolutionTimeByErrorType(DateTime? endDate = null, string timePeriod = "all")
        {
            try
            {
                Tuple<DateTime, DateTime> dates = GetStartAndEndDate(endDate, timePeriod);
                DateTime actualStartDate = dates.Item1;
                DateTime actualEndDate = dates.Item2;

                var avgResolutionTimeByErrorType = db.Errors
                    .Where(e => e.ClosingDate != null
                                && e.OpeningDate != null
                                && e.OpeningDate >= actualStartDate && e.OpeningDate <= actualEndDate)
                    .GroupBy(e => e.ErrorType)
                    .Select(group => new
                    {
                        ErrorType = group.Key,
                        AverageResolutionTime = group.Average(e => DbFunctions.DiffHours(e.OpeningDate, e.ClosingDate))
                    })
                    .ToList();

                return Ok(avgResolutionTimeByErrorType);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/errors/average-resolution-time-by-technician")]
        public IHttpActionResult GetAverageResolutionTimeByTechnician(DateTime? endDate = null, string timePeriod = "all")
        {
            try
            {
                Tuple<DateTime, DateTime> dates = GetStartAndEndDate(endDate, timePeriod);
                DateTime actualStartDate = dates.Item1;
                DateTime actualEndDate = dates.Item2;

                var avgResolutionTimeByTechnician = db.Errors
                    .Include(e => e.Technician)
                    .Include(e => e.Technician.Employee)
                    .Where(e => e.ClosingDate != null
                                && e.OpeningDate != null
                                && e.OpeningDate >= actualStartDate && e.OpeningDate <= actualEndDate)
                    .GroupBy(e => new { e.TechnicianID, TechnicianName = e.Technician.Employee.FirstName + " " + e.Technician.Employee.LastName })
                    .Select(group => new
                    {
                        Technician = group.Key.TechnicianName,
                        AverageResolutionTime = group.Average(e => DbFunctions.DiffHours(e.OpeningDate, e.ClosingDate))
                    })
                    .ToList();

                return Ok(avgResolutionTimeByTechnician);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private Tuple<DateTime, DateTime> GetStartAndEndDate(DateTime? endDate, string timePeriod)
        {
            DateTime actualEndDate = endDate ?? DateTime.Now;
            DateTime actualStartDate;

            switch (timePeriod.ToLower())
            {
                case "day":
                    actualStartDate = actualEndDate.AddDays(-1);
                    break;
                case "week":
                    actualStartDate = actualEndDate.AddDays(-7);
                    break;
                case "month":
                    actualStartDate = actualEndDate.AddMonths(-1);
                    break;
                default:
                    actualStartDate = (DateTime)db.Errors.Min(e => e.OpeningDate);
                    break;
            }

            return new Tuple<DateTime, DateTime>(actualStartDate, actualEndDate);
        }

    }

}

