﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetGo.Domain.Payload.Request.User
{
    public class SignInRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public SignInRequest(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
