using System;

namespace TaskBlaster.TaskManagement.Models.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException() : base("Bad request") { }
    public BadRequestException(string errorMessage) : base(errorMessage) { }
}