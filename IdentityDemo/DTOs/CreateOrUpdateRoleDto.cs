﻿namespace IdentityDemo.DTOs
{
    public class CreateOrUpdateRoleDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
    }
}
