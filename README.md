SharpScad - OpenSCAD clone written in C#
========================================

This implementation of CSG (constructive solid geometry) and OpenSCAD language allows to apply textures to models and to generate Wavefront OBJ textured outputs. So it can be used for model generation and for architecture and technical previews. Moreover you can simply take your OpenSCAD project and to add textures.

SharpScad has cross-platform GUI that should work in UNIX clones (GNU/Linux, FreeBSD, ...), MacOS X, Windows (10+).


**WARNING:** this project is in alpha stage now so don't expect to see some production ready application.

## Motivation
Originally I wanted to have a simple program to generate primitive 3D models and I've started writing OpenSCAD implementation in C++. After some time I received an offer from Microsoft so I needed to learn C# and so I've rewritten the project in C#.

## Project structure
Project contains several parts. The main part is *Scad* library that implements CSG operations. As a part of this library in Openscad subdirectory I implemented OpenSCAD parser and interpreter. *Parser* library implements PEG parsing library that allows to define grammars easily.

There are several applications here. ScadToStl allows to render model to STL/OBJ file. ScadView is the main GUI for the project.

I use several external libraries in this project.

 * *CommandLineParser* for parsing command line arguments
 * *SharpWebview* for making nice user interface
 * *ObjParser* by Stefan Gordon was imported and patched for Wavefront.Obj files loading

For interface I'm using:

 * *mithril.js* as a lightweight Web framework
 * *construct-ui* for widgets
 * *ace editor* for editor widget
 * *three.js* to show 3D model
 * *esbuild* for building/minifying.

## TODO

### Version 0.01
 * ☑ Implement CSG
 * ☑ Implement texturing
 * ☑ Implement basic GUI

### Version 0.1:
 * ☐ render list of basic models
 * ☐ useful GUI with correct axes
 * ☐ tested on Linux and Windows
 * ☐ texturing

### Version 0.2:
 * ☐ BOSL is useful
 * ☐ more debugging options
 * ☐ start generating distributives
