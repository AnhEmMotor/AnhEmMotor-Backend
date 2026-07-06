const fs = require('fs');
const p = 'WebAPI/Extensions/MigrationExtensions.cs';
let lines = fs.readFileSync(p, 'utf8').split(/\r?\n/);
for (let i = lines.length - 1; i >= 0; i--) {
  if (lines[i].includes('WorkshopAndServiceSeeder')) {
    lines.splice(i, 1);
    console.log('Removed line', i+1, ':', lines[i]);
  }
}
fs.writeFileSync(p, lines.join('\n') + '\n', 'utf8');
console.log('Done. Total lines:', lines.length);
