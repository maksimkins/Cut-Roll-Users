using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cut_Roll_Users.Core.Users.Dtos;

public class UserSimlified
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public string? AvatarPath { get; set; }
}
