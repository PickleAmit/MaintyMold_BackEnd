using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLmodel
{
    public class ModelInput
    {
        public float ProblemType { get; set; }
        public float Priority { get; set; }
        public uint TechnicianID { get; set; }
        public float TimeToResolve { get; set; }
    }

}
