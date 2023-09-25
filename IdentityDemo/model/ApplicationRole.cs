﻿using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.model
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }

        public ApplicationRole(string name, string? description = null)
            : base(name)
        {
            Description = description;
            NormalizedName = name.ToUpperInvariant();
        }
    }
}
