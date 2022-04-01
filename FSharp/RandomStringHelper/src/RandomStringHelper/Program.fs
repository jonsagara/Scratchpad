open RandomStringHelper

let writeSample (functionName) (generatedString) =
    printfn $"{functionName}:"
    printfn $"  {generatedString}"

writeSample (nameof RandomStringHelper.generateAlphanumericString) (RandomStringHelper.generateAlphanumericString 64)
writeSample (nameof RandomStringHelper.generateAlphanumericStringWithDashUnderscore) (RandomStringHelper.generateAlphanumericStringWithDashUnderscore 64)
writeSample (nameof RandomStringHelper.generateRandomString) (RandomStringHelper.generateRandomString 64)
