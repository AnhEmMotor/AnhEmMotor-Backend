$p = "Infrastructure\Seeders\SalesAndInventorySeeder.cs"
$content = Get-Content $p -Raw
# Remove the if (!await context.RepairOrders.AnyAsync ...) block plus the following await SaveChangesAsync line
$pattern = [regex]::Escape("if (!await context.RepairOrders.AnyAsync(cancellationToken).ConfigureAwait(false))") + ".*?await context.SaveChangesAsync\(cancellationToken\).ConfigureAwait\(false\);\s*"
$newContent = [regex]::Replace($content, $pattern, "", [System.Text.RegularExpressions.RegexOptions]::Singleline)
Set-Content $p $newContent -NoNewline
Write-Host "Done. Checking balance..."
$depth = 0; $max = 0; foreach ($ch in $newContent.ToCharArray()) { if ($ch -eq '{') { $depth++; $max = [Math]::Max($max, $depth) } if ($ch -eq '}') { $depth-- } }
Write-Host "Final depth: $depth, Max: $max"
