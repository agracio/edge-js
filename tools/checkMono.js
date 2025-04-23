const { exec } = require('child_process');

function exists(cmd, cb){
    exec(cmd, function (err, stdout, stderr) {
        let result
        if(err){
            result = false;
        }
        else{
            result = stdout.toLowerCase().includes('not found') || stdout.length === 0 ? false : true
        }
        cb(result);
    });
}

exists('which mono', (mono)=>{
    exists('which pkg-config', (pkgconfig)=>{
        console.log(mono && pkgconfig)
    });
});

