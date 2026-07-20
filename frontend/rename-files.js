const fs = require('fs');
const path = require('path');

const base = 'src/app/pages';
const pages = [
  ['patients', 'patients'],
  ['doctors', 'doctors'],
  ['appointments', 'appointments'],
  ['auth', 'auth'],
  ['dashboard', 'dashboard'],
  ['book-appointment', 'book-appointment'],
  ['my-appointments', 'my-appointments'],
  ['doctors-list', 'doctors-list'],
  ['user-portal', 'user-portal'],
];

const exts = ['ts', 'html', 'scss'];

pages.forEach(([folder, name]) => {
  exts.forEach(ext => {
    const src = path.join(base, folder, `${name}.${ext}`);
    const dst = path.join(base, folder, `${name}.component.${ext}`);
    if (fs.existsSync(src)) {
      fs.copyFileSync(src, dst);
      process.stdout.write(`Copied: ${src} -> ${dst}\n`);
    } else {
      process.stdout.write(`SKIP (not found): ${src}\n`);
    }
  });
});

process.stdout.write('All files copied.\n');
