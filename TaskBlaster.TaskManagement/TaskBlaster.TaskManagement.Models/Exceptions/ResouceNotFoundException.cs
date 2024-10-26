using System;

namespace TaskBlaster.TaskManagement.Models.Exceptions;

public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException() : base("Resource not found") { }
    public ResourceNotFoundException(string errorMessage) : base(errorMessage) { }
}