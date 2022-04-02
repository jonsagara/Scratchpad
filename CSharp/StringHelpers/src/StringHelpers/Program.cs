
using StringHelpers;

WriteSample(nameof(RandomStringHelper.GenerateAlphanumericString), RandomStringHelper.GenerateAlphanumericString(64));
WriteSample(nameof(RandomStringHelper.GenerateAlphanumericString) + " (includeDashAndUnderscore=false)", RandomStringHelper.GenerateAlphanumericString(64, includeDashAndUnderscore: false));
WriteSample(nameof(RandomStringHelper.GenerateUppercaseAlphanumericString), RandomStringHelper.GenerateUppercaseAlphanumericString(64));
WriteSample(nameof(RandomStringHelper.GenerateRandomString), RandomStringHelper.GenerateRandomString(64));
WriteSample(nameof(StringEncodingHelper.ToUrlEncodedString), Guid.NewGuid().ToUrlEncodedString());

static void WriteSample(string functionName, string generatedString)
{
    Console.WriteLine($"{functionName}:");
    Console.WriteLine($"  {generatedString}");
}