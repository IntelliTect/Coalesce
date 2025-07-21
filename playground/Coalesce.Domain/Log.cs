﻿using IntelliTect.Coalesce.DataAnnotations;

namespace Coalesce.Domain;

[Edit(PermissionLevel = SecurityPermissionLevels.DenyAll)]
[Create(PermissionLevel = SecurityPermissionLevels.DenyAll)]
[Delete(PermissionLevel = SecurityPermissionLevels.DenyAll)]
public class Log
{
#nullable disable
    public int LogId { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
}
