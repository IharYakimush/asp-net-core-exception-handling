using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Commmunity.AspNetCore.ExceptionHandling.Integration.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {

            if (id > 25)
            {
                throw new DuplicateWaitObjectException();
            }

            if (id > 20)
            {
                throw new DuplicateNameException();
            }

            if (id > 15)
            {
                throw new InvalidConstraintException();
            }

            if (id > 10)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (id > 5)
            {
                throw new InvalidCastException();
            }

            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
