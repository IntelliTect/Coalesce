const { exec } = require('child_process');
const fs = require('fs');
const path = require('path');

const rootDir = process.cwd();

function findWebDirectory(dir) {
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            if (file.endsWith('.Web')) {
                return fullPath;
            } else {
                const result = findWebDirectory(fullPath);
                if (result) return result;
            }
        }
    }
    return null;
}

const webDir = findWebDirectory(rootDir);

if (webDir) {
    console.log(`Found .Web directory: ${webDir}`);
    exec('npm install', { cwd: webDir }, (error, stdout, stderr) => {
        if (error) {
            console.error(`Error executing npm install: ${error.message}`);
            console.error(stderr);
            console.log(stdout);
            return;
        }
        console.log(stdout);
        console.error(stderr);

        exec('npm run lint:fix', { cwd: webDir }, (error, stdout, stderr) => {
            if (error) {
                console.error(`Error executing npm run lint:fix: ${error.message}`);
                console.error(stderr);
                console.log(stdout);
                return;
            }
            console.log(stdout);
            console.error(stderr);

            // Delete the script after execution
            fs.unlink(__filename, (err) => {
                if (err) {
                    console.error(`Error deleting script: ${err.message}`);
                } else {
                    console.log('Script deleted successfully.');
                }
            });
        });
    });
} else {
    console.error('No .Web directory found.');
}
