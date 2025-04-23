const { exec, spawn, spawnSync } = require('child_process');

function exists(cmd){
    exec(cmd, function (err, stdout, stderr) {
        return stdout.toLowerCase().includes('not found') ? false : true;
    });
}

let mono = exists('which mono');
let pkgconfig = exists('which pkg-config');
return mono && pkgconfig;

