﻿using cw5.DTOs.Requests;
using cw5.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw5.Services
{
    public interface IStudentsDbService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest student);
        public void PromoteStudent(PromoteStudentsRequest newStudent);
    }
}