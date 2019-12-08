using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DataContext _context;

        public ValuesController(DataContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var value = _context.Value.ToList();
            return Ok(value);
        }
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            var value = _context.Value.FirstOrDefault(x=> x.Id == id);
            return Ok(value);
        }

    }
}