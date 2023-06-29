using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.ML;
using MLmodel;

namespace WebApplication1.Controllers
{
    public class MLModelController : ApiController
    {
        private TechnicianPredictionModel _mlModel;
        private ITransformer _model;

        public MLModelController()
        {
            _mlModel = new TechnicianPredictionModel();
            _model = _mlModel.LoadModel();
        }

        // POST api/predict
        [HttpPost]
        [Route("api/predict")]
        public IHttpActionResult PredictTechnician([FromBody] RequestBody requestBody)
        {
            float encodedProblemType;

            try
            {
                // Encode the problem type string into a float
                encodedProblemType = _mlModel.EncodeProblemType(requestBody.ProblemType);
            }
            catch (Exception ex)
            {
                return BadRequest("Error encoding problem type: " + ex.Message);
            }

            ModelInput input;

            try
            {
                // Create a new ModelInput with the encoded problem type
                input = new ModelInput
                {
                    ProblemType = encodedProblemType, // Here is the encoded problem type
                    Priority = requestBody.Priority,
                    TechnicianID = 0, // These fields are not needed for the prediction
                    TimeToResolve = 0  // These fields are not needed for the prediction
                };
            }
            catch (Exception ex)
            {
                return BadRequest("Error creating ModelInput: " + ex.Message);
            }

            try
            {
                var prediction = _mlModel.PredictTechnician(_model, input);
                return Ok(new { technician = prediction });
            }
            catch (Exception ex)
            {
                // Ideally you should handle specific exceptions so you can return more meaningful error messages
                return Content(HttpStatusCode.InternalServerError,"Error making prediction: " + ex.Message);
            }
        }


        public class RequestBody
        {
            public string ProblemType { get; set; }
            public float Priority { get; set; }
        }

    }
}
