const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

let nodejsVersion = process.argv[2];
let version;
let nodeGyp;
const arch = ['ia32', 'x64', 'arm64'];

if (!nodejsVersion) {
    console.error('Node.js version argument is required');
    console.log('Usage: node build.js <major-version>|<nodejs-version>');
    console.log('Example: node build.js 22');
    console.log('Example: node build.js 22.3.0');
    process.exit(1);
}

if(!nodejsVersion.includes('.')){
    console.log();
    console.log(`Resolving latest Node.js version for major version ${nodejsVersion}...`);
    // version = require('./getVersion')(nodejsVersion);
    version = execSync(`node tools/getVersion.js ${nodejsVersion}`);
    console.log(`Resolved Node.js version ${version}`);
}
else{
    console.log();
    console.log(`Using specified Node.js version ${nodejsVersion}`);
    version = nodejsVersion;
    nodejsVersion = version.split('.')[0];
}

if(Number(nodejsVersion) < 16){
    console.error('Node.js version 16 or higher is required');
    process.exit(1);
}

function deleteBuildDir() {

    const buildDir = path.join(__dirname, '..', 'build');

    if (fs.existsSync(buildDir)) {
        console.log();
        console.log(`Removing existing build directory: ${buildDir}`);
        fs.rmSync(buildDir, { recursive: true, force: true });
    }
}

function findNodeGyp() {

    if(nodeGyp){
        return nodeGyp;
    }
        
    try {
        console.log();
        console.log('Locating node-gyp...');
        let result = execSync('npm config get prefix').toString().trim();
        if (fs.existsSync(result)) {
            nodeGyp = path.join(result, 'node_modules', 'node-gyp', 'bin', 'node-gyp.js');
            console.log(`Found node-gyp at ${nodeGyp}`);
            return nodeGyp;
        }
        else{
            console.error();
            console.error('node-gyp not found');
            process.exit(1);
        }
    } catch (err) {
        throw err;
    }   
}

function fixArmBuild() {
    const files = fs.readdirSync(path.join(__dirname, '..', 'build'));
    for (const file of files) {
        if (path.extname(file) === '.vcxproj') {
            const filePath = path.join(__dirname, '..', 'build', file);
            console.log(`Patching ${filePath} for arm64...`);
            let data = fs.readFileSync(filePath, 'utf8');
            data = data.replace(/<FloatingPointModel>Strict<\/FloatingPointModel>/g, '<!-- <FloatingPointModel>Strict</FloatingPointModel> -->');
            fs.writeFileSync(filePath, data, 'utf8');
        }
    }
}

function buildNode(arch) {

    try {
        deleteBuildDir()
        console.log();
        console.log(`Building edge-js ${process.platform}-${arch} for Node.js ${version}...`);
        findNodeGyp();
        console.log();
        execSync(`node "${nodeGyp}" configure --target=${version} --arch=${arch} --runtime=node --release`, { stdio: 'inherit' });
        if(arch === 'arm64' && process.platform === 'win32'){
            console.log();
            fixArmBuild(process.cwd());
            console.log();
        }
        execSync(`node "${nodeGyp}" build`, { stdio: 'inherit' });
        console.log();
        console.log('Build completed successfully');
    } catch (err) {
        console.error();
        console.error('Build failed');
        console.error(err);
        process.exit(1);
    }
}

function copyBuildOutput(arch) {
    const buildDir = path.join(__dirname, '..', 'build', 'Release');
    const outputDir = path.join(__dirname, '..', 'lib', 'native', process.platform, arch, nodejsVersion);

    console.log(`Copying built binaries from ${buildDir} to ${outputDir}...`);
    if (!fs.existsSync(outputDir)) {
        fs.mkdirSync(outputDir, { recursive: true });
    }
    const files = fs.readdirSync(buildDir);
    for (const file of files) {
        if (path.extname(file) === '.node' || path.extname(file) === '.exe') {
            fs.copyFileSync(path.join(buildDir, file), path.join(outputDir, file));
            console.log(`Copied ${file} to ${outputDir}`);
        }
    }
    console.log(`Completed copying built binaries from ${buildDir} to ${outputDir}`);
    console.log();
    console.log(`Creating ${outputDir}/node.version file...`);
    fs.writeFileSync(`${outputDir}/node.version`, version);
    console.log(`Created ${outputDir}/node.version file`);
}

function buildAll() {
    if(process.platform === 'linux'){
        buildNode(process.arch);
    }
    else{
        for (const a of arch) {
            if(a === 'arm64' && process.platform === 'win32' && Number(nodejsVersion) < 20){
                console.log();
                console.log(`Skipping arm64 build for Node.js ${version}`);
                continue;
            }
            if(a === 'ia32' && process.platform === 'win32' && Number(nodejsVersion) > 22){
                console.log();
                console.log(`Skipping x86 build for Node.js ${version}`);
                continue;
            }
            if(a === 'ia32' && process.platform !== 'win32'){
                console.log();
                console.log(`Skipping x86 build on non-windows platform`);
                continue;
            }
            buildNode(a);
            console.log();
            copyBuildOutput(a);
        } 
        deleteBuildDir();
    }
    console.log();
    console.log('All builds completed successfully');
}

buildAll();
