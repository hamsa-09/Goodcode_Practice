$files = Get-ChildItem -Path "c:\Users\Hp\source\repos\sportManagement" -Recurse -Filter "*.cs"
foreach ($file in $files) {
    Write-Host "Checking $($file.FullName)"
    $content = Get-Content -Path $file.FullName -Raw
    if ($content -match "SportManagement.Api") {
        Write-Host "Updating $($file.FullName)"
        $content = $content.Replace("SportManagement.Api", "Assignment_Example_HU")
        Set-Content -Path $file.FullName -Value $content -NoNewline
    }
}
$startFiles = Get-ChildItem -Path "c:\Users\Hp\source\repos\sportManagement" -Filter "*.cs"
foreach ($file in $startFiles) {
    Write-Host "Checking $($file.FullName)"
    $content = Get-Content -Path $file.FullName -Raw
    if ($content -match "SportManagement.Api") {
        Write-Host "Updating $($file.FullName)"
        $content = $content.Replace("SportManagement.Api", "Assignment_Example_HU")
        Set-Content -Path $file.FullName -Value $content -NoNewline
    }
}
