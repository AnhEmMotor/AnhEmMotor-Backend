using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace IntegrationTests.SetupClass
{
    public class IntegrationTestFileHelper
    {
        private static readonly Lazy<byte[]> ValidJpgBytes = new(() =>
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestData", "valid-image.jpg");
            return File.ReadAllBytes(path);
        });

        public static byte[] GetValidJpgBytes() => ValidJpgBytes.Value;

        public static MultipartFormDataContent CreateSingleImageForm(
            string fieldName = "file",
            string fileName = "image.jpg",
            string contentType = "image/jpeg",
            byte[]? bytes = null)
        {
            bytes ??= GetValidJpgBytes();

            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, fieldName, fileName);
            return content;
        }

        public static MultipartFormDataContent CreateManyImagesForm(params (string Field, string FileName, string ContentType, byte[] Bytes)[] files)
        {
            var content = new MultipartFormDataContent();
            foreach (var f in files)
            {
                var fileContent = new ByteArrayContent(f.Bytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);
                content.Add(fileContent, f.Field, f.FileName);
            }
            return content;
        }
    }
}
