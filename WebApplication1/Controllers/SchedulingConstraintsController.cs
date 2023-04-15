using DATA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class SchedulingConstraintsController : ApiController
    {
        NetafimDbContext db = new NetafimDbContext();

        //Post new SchedulingConstraint
        [HttpPost]
        [Route("api/schedulingconstraints/new")]
        public IHttpActionResult AddSchedulingConstraint(AddSchedulingConstraintModel model)
        {
            try
            {
                int technicianId = model.TechnicianID;
                string description = model.Description;
                TimeSpan constraintStartHour = model.ConstraintStartHour;
                TimeSpan constraintEndHour = model.ConstraintEndHour;
                DateTime constraintDate = model.ConstraintDate;

                if (string.IsNullOrEmpty(description))
                {
                    return BadRequest("Description cannot be empty.");
                }

                SchedulingConstraint constraint = new SchedulingConstraint
                {
                    TechnicianID = technicianId,
                    ConstraintStartHour = constraintStartHour,
                    ConstraintEndHour = constraintEndHour,
                    ConstraintDate = constraintDate,
                    Description = description,
                };

                db.SchedulingConstraints.Add(constraint);
                db.SaveChanges();

                return Ok(constraint);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }

        [HttpGet]
        [Route("api/schedulingconstraints")]
        public IHttpActionResult GetAllSchedulingConstraints()
        {
            try
            {
                var constraints = db.SchedulingConstraints.Select(c => new
                {
                    c.TechnicianID,
                    c.ConstraintDate,
                    c.Description,
                    c.ConstraintStartHour,
                    c.ConstraintEndHour
                }).ToList();
                return Ok(constraints);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }

        // Gets all the constraints of a specific technician
        [HttpGet]
        [Route("api/schedulingconstraints/technician/{technicianId}")]
        public IHttpActionResult GetSchedulingConstraintsByTechnicianId(int technicianId)
        {
            try
            {
                var constraints = db.SchedulingConstraints.Where(c => c.TechnicianID == technicianId).Select(c => new
                {
                    c.TechnicianID,
                    c.ConstraintDate,
                    c.Description,
                    c.ConstraintStartHour,
                    c.ConstraintEndHour
                }).ToList();
                return Ok(constraints);
            }
            catch (Exception ex) { return InternalServerError(ex); }

        }


    }

    public class AddSchedulingConstraintModel
    {
        public TimeSpan ConstraintStartHour { get; set; }
        public TimeSpan ConstraintEndHour { get; set; }
        public string Description { get; set; }
        public int TechnicianID { get; set; }
        public DateTime ConstraintDate { get; set; }

    }
}