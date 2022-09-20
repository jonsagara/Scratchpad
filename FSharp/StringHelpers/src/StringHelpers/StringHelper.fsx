#load "StringHelper.fs"

open StringHelpers.StringHelper

let dessert = "crème brûlée/!#$@#FASDF_foo.html"
let dessertNoDiacritics = dessert |> removeDiacritics
let dessertSlug = dessert |> toSlug

printfn $"{dessert}"
printfn $"No Diacritics: {dessertNoDiacritics}"
printfn $"Slug: {dessertSlug}"
