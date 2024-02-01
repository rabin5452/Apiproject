﻿using Microsoft.AspNetCore.Identity;

namespace Practiseproject.Model
{
    public class User:IdentityUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
