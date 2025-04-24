const { execSync } = require('child_process');

function exists(cmd){
    try{
        let result = execSync(cmd).toString();
        return result.toLowerCase().includes('not found') || result.length === 0 ? false : true;
    }
    catch{
        return false;
    }
}

module.exports = function() {
    let mono = exists('which mono');
    let pkgconfig = exists('which pkg-config');
    return mono && pkgconfig
}