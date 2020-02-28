﻿using System.IO;

namespace IntelliTect.Coalesce.Models
{
    /// <summary>
    /// Representation of a file for use in Coalesce method parameters.
    /// </summary>
    public interface IFile
    {
        Stream Content { get; }
        string ContentType { get; }
        string Name { get; }
        long Length { get; }
    }
}