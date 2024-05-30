//import m from "mithril";
import { Grid, Col } from "construct-ui";
import { ajaxGet, ajaxPost } from "./ajax.js";

function loadSource() {
    // 1. Send request to load source
    ajaxGet("/api/load", (data) => {
        var d = JSON.parse(data);
        var editor = document.getElementById("source_editor");
        editor.setValue(d.text);
    });
}

function updateModel() {
    var editor = document.getElementById("source_editor");
    var src = editor.getValue();
    ajaxPost("/api/save", function (resp) {
        var data = JSON.parse(resp);
        var view = document.getElementById("model");

        if (data.model) {
            view.updateModel(data.model);
        }

        var logs = data.log || [];
        var log = document.getElementById("log");
        log.innerHTML = "";
        for (var line of logs) {
            var d = document.createElement("p");
            d.appendChild(document.createTextNode(line));
            log.appendChild(d);
        }
    }, JSON.stringify({ text: src}), { "content-type": "application/json" } );
}

export var IndexPage = {
    view: function () {
        setTimeout(loadSource, 0);
        return <Grid>
            <Col class="cui-example-grid-col" span={6}>
                <div id="src">
                    <ace-view id="source_editor"></ace-view>
                    <button onclick={updateModel}>Save</button>
                </div>
                <div id="log"></div>
            </Col>
            <Col class="cui-example-grid-col" span={6}>
                <model-view id="model" style="width:100%; height:100%; border: 1px solid black;"></model-view>
            </Col>
        </Grid>;
    }
};
