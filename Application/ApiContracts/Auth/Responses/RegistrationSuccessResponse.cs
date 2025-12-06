using System;
using System.Collections.Generic;
using System.Text;

using Application;
using Application.ApiContracts;
using Application.ApiContracts.Auth;

namespace Application.ApiContracts.Auth.Responses
{
    public class RegistrationSuccessResponse
    {
        public string Message { get; set; } = "Registration successful!";
    }
}
