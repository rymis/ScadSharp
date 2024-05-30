const { exec } = require("child_process");
const fs = require("fs");
const path = require("path");

"use strict";

const BUILD = "npx esbuild index.js --bundle --minify --jsx-factory=m --jsx-fragment=m.Fragment --sourcemap --outfile=js/index.js";
const ACE_SRC = "./node_modules/ace-builds/src-min-noconflict/";
const FILES = {
  'keybinding-emacs.js': true,
  'keybinding-sublime.js': true,
  'keybinding-vim.js': true,
  'keybinding-vscode.js': true,
  'mode-scad.js': true,
};

function build_js() {
    exec(BUILD, (error, stdout, stderr) => {
        if (error) {
            console.log(`error: ${error.message}`);
            return;
        }
        if (stderr) {
            console.log(`stderr: ${stderr}`);
            return;
        }
        console.log(`stdout: ${stdout}`);
    });
}

function copy_ace() {
    let files = fs.readdirSync(ACE_SRC);

    for (let fnm of files) {
        if (FILES[fnm] || fnm.startsWith("theme-")) {
            console.log(`cp ${fnm}`);
            fs.copyFileSync(path.join(ACE_SRC, fnm), path.join("./js", fnm));
        }
    }
}

build_js();
copy_ace();
