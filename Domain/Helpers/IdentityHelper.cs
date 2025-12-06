using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Helpers
{
    public static class IdentityHelper
    {
        public static string GetFieldForIdentityError(string errorCode)
        {
            return errorCode switch
            {
                "DuplicateUserName" or "InvalidUserName" => "Username",
                "DuplicateEmail" or "InvalidEmail" => "Email",
                "PasswordTooShort" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresDigit" or "PasswordRequiresUpper" or "PasswordRequiresLower" => "Password",
                "DuplicateRoleName" or "InvalidRoleName" => "RoleName",
                _ => string.Empty
            };
        }
    }
}
