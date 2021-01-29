﻿using cw5.DTOs.Requests;
using cw5.DTOs.Responses;
using cw5.Models;
using cw5.Services;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace cw5.Controllers
{

    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {


        //private const string ConString = "Data Source=DESKTOP-RSTT48M\\SQLEXPRESS;Initial Catalog=apbd;Integrated Security=True";
        private IStudentsDbService _service;
        public EnrollmentsController(IStudentsDbService service)
        {
            _service = service;
        }
            
        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest newStudent)
        {
            try
            {
                EnrollStudentResponse resp = _service.EnrollStudent(newStudent);
                return CreatedAtRoute(new RouteValues(), resp);
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }

        }

        [Route("promotions")]
        [HttpPost]
        
        public IActionResult PromoteStudent(PromoteStudentsRequest promotion)
        {
            
                _service.PromoteStudent(promotion);

                PromoteStudentsResponse resp = new PromoteStudentsResponse();
                return CreatedAtRoute(new RouteValues(), resp);
         }
            
            
        
    }
}
