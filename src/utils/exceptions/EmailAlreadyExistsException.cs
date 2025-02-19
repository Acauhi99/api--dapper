using System;

namespace api__dapper.utils.exceptions
{
  public class EmailAlreadyExistsException : Exception
  {
    public EmailAlreadyExistsException(string message) : base(message) { }
  }
}
