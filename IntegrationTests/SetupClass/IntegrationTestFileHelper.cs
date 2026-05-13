using System;
using System.Net.Http.Headers;

namespace IntegrationTests.SetupClass
{
    public class IntegrationTestFileHelper
    {
        private static readonly Lazy<byte[]> ValidJpgBytes = new(
            () => Convert.FromBase64String(
                "/9j/4AAQSkZJRgABAQAAAQABAAD/4gHYSUNDX1BST0ZJTEUAAQEAAAHIAAAAAAQwAABtbnRyUkdCIFhZWiAAAAAAAAAAAAAAAABhY3NwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAA9tYAAQAAAADTLQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAlkZXNjAAAA8AAAACRyWFlaAAABFAAAABRnWFlaAAABKAAAABRiWFlaAAABPAAAABR3dHB0AAABUAAAABRyVFJDAAABZAAAAChnVFJDAAABZAAAAChiVFJDAAABZAAAAChjcHJ0AAABjAAAADxtbHVjAAAAAAAAAAEAAAAMZW5VUwAAAAgAAAAcAHMAUgBHAEJYWVogAAAAAAAAb6IAADj1AAADkFhZWiAAAAAAAABimQAAt4UAABjaWFlaIAAAAAAAACSgAAAPhAAAts9YWVogAAAAAAAA9tYAAQAAAADTLXBhcmEAAAAAAAQAAAACZmYAAPKnAAANWQAAE9AAAApbAAAAAAAAAABtbHVjAAAAAAAAAAEAAAAMZW5VUwAAACAAAAAcAEcAbwBvAGcAbABlACAASQBuAGMALgAgADIAMAAxADb/2wBDACAWGBwYFCAcGhwkIiAmMFA0MCwsMGJGSjpQdGZ6eHJmcG6AkLicgIiuim5woNqirr7EztDOfJri8uDI8LjKzsb/2wBDASIkJDAqMF40NF7GhHCExsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsbGxsb/wAARCAAwADADASIAAhEBAxEB/8QAGQAAAgMBAAAAAAAAAAAAAAAAAwQAAgUB/8QAJRAAAQQBBAEEAwAAAAAAAAAAAQACAxEEEiExUUEiMmGBE3Gh/8QAGAEBAQEBAQAAAAAAAAAAAAAAAwIAAQT/xAAaEQACAwEBAAAAAAAAAAAAAAAAAQIRITFB/9oADAMBAAIRAxEAPwBsk67HhcLz0oef2qy+hv0kboyF8jI0E70fhZ8shcbP9KrISXkkk7+VQjteZ7rEusQSKd0bwbJWjDkiSh5WWCBymMdx/M0NG1qk6N02mg1eyWzQ4Q2O000bBBzIy+A7cG+U0uBx6YoFvRQ1vS7pANqaXHjdCKXGOx1Gk1DG1vtbVJaJzy0gAEhP4odoJd56VRRyXBqxSHMfT99Lr3tjbbyAPkpGbNY8hkVkk1ZSN0GkAyBpmcPlDDtNqSG39c7/AGo73kGyiYqfgSAEWa5WjGQ1oCzyA3Ge4eAhw55bTZBY78qo4RJn/9k="));

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

        public static MultipartFormDataContent CreateManyImagesForm(
            params (string Field, string FileName, string ContentType, byte[] Bytes)[] files)
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
