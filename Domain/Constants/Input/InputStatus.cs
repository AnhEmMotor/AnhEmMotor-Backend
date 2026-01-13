using System;
using Domain.Constants;
using Domain;

namespace Domain.Constants.Input
{
    public static class InputStatus
    {
        public const string Working = "working";
        public const string Finish = "finished";
        public const string Cancel = "cancelled";

        public static readonly string[] AllowedValues = [ Working, Finish, Cancel ];
        public static readonly string[] FinishInputValues = [ Finish ];
        public static readonly string[] WorkingInputValues = [ Working ];
        public static readonly string[] NotEditInputValues = [ Cancel, Finish ];

        public static bool IsValid(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;

            return AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsFinished(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;

            return FinishInputValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCanEdit(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;
            return WorkingInputValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCannotEdit(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return true;
            return NotEditInputValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsCannotDelete(string? value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return true;
            return NotEditInputValues.Contains(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
