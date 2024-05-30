// Entry point for everything
import m from "mithril";
import { IndexPage } from "./src/index_page.jsx";
import {} from "./src/model_view.js";
import {} from "./src/ace_view.js";

import "./node_modules/construct-ui/lib/index.css";

// Set fragment function and mithril global instance to make JSX easier
m.Fragment = { view: vnode => vnode.children };
window.m = m;

window.onload = function () {
    let div = document.getElementById("main-app");

    m.route(div, "/index", {
        "/index": IndexPage,
    });
};
