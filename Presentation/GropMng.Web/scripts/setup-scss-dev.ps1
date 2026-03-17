# Money Manager SCSS Development Helper
# =====================================

function Show-Menu {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║   Money Manager SCSS Development Helper               ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. 👀 Watch SCSS files (auto-compile on change)" -ForegroundColor Green
    Write-Host "2. 🏗️  Build SCSS once" -ForegroundColor Yellow
    Write-Host "3. 📦 Install dependencies" -ForegroundColor Blue
    Write-Host "4. 🚀 Watch SCSS + Start .NET project" -ForegroundColor Magenta
    Write-Host "5. 📊 Show SCSS file count" -ForegroundColor Cyan
    Write-Host "6. ✋ Exit" -ForegroundColor Red
    Write-Host ""
}

function Watch-Scss {
    Clear-Host
    Write-Host "👀 Watching SCSS files for changes..." -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop." -ForegroundColor Yellow
    Write-Host ""
    npm run watch:css
}

function Build-Scss {
    Clear-Host
    Write-Host "🏗️  Building SCSS files..." -ForegroundColor Yellow
    Write-Host ""
    npm run build:css
    Write-Host ""
    Write-Host "✅ Build complete!" -ForegroundColor Green
    Read-Host "Press Enter to continue"
}

function Install-Dependencies {
    Clear-Host
    Write-Host "📦 Installing npm dependencies..." -ForegroundColor Blue
    Write-Host ""
    npm install
    Write-Host ""
    Write-Host "✅ Installation complete!" -ForegroundColor Green
    Read-Host "Press Enter to continue"
}

function Watch-And-Run {
    Clear-Host
    Write-Host "🚀 Starting SCSS watch + .NET development server..." -ForegroundColor Magenta
    Write-Host ""
    Write-Host "Note: SCSS watch is now running in this terminal." -ForegroundColor Yellow
    Write-Host "To run the .NET project, open another terminal and run: dotnet run" -ForegroundColor Yellow
    Write-Host ""
    npm run watch:css
}

function Show-ScssStats {
    Clear-Host
    Write-Host "📊 SCSS File Statistics" -ForegroundColor Cyan
    Write-Host ""
    
    $scssFiles = Get-ChildItem -Path "wwwroot\scss" -Filter "*.scss" -Recurse
    $cssFiles = Get-ChildItem -Path "wwwroot\vendor\css" -Filter "*.css" -Recurse -ErrorAction SilentlyContinue
    
    Write-Host "Total SCSS source files: $($scssFiles.Count)" -ForegroundColor Green
    Write-Host "Total CSS output files: $($cssFiles.Count)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Main SCSS files:" -ForegroundColor Yellow
    Get-ChildItem -Path "wwwroot\scss" -Filter "*.scss" -MaxDepth 1 | Select-Object -ExpandProperty Name | ForEach-Object { Write-Host "  ✓ $_" -ForegroundColor Green }
    
    $scssSize = ($scssFiles | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host ""
    Write-Host "SCSS directory size: $([math]::Round($scssSize, 2)) MB" -ForegroundColor Cyan
    
    Read-Host "Press Enter to continue"
}

# Main loop
do {
    Show-Menu
    $choice = Read-Host "Enter choice (1-6)"
    
    switch ($choice) {
        '1' { Watch-Scss }
        '2' { Build-Scss }
        '3' { Install-Dependencies }
        '4' { Watch-And-Run }
        '5' { Show-ScssStats }
        '6' { Write-Host "👋 Goodbye!" -ForegroundColor Cyan; exit }
        default { Write-Host "❌ Invalid choice. Try again." -ForegroundColor Red; Start-Sleep -Seconds 2 }
    }
} while ($true)
