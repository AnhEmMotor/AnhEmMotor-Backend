using System;

namespace Application.Common.Extensions
{
    public static class StringExtensions
    {
        public static string ExtractFileName(this string urlOrFileName)
        {
            if(string.IsNullOrWhiteSpace(urlOrFileName))
            {
                return string.Empty;
            }

            var fileName = urlOrFileName.Trim();

            if(fileName.Contains('/'))
            {
                fileName = fileName.Split('/').Last();
            }

            return fileName;
        }
    }
}