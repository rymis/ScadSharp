import * as ace from 'ace-builds/src-noconflict/ace.js';

class AceView extends HTMLElement {
    static observedAttributes = [ "value" ];

    constructor() {
        super();
    }

    connectedCallback() {
        this._editor = ace.edit(this, {
            minLines: 30,
            maxLines: 30,
            wrap: true,
            autoScrollEditorIntoView: true
        });
        this._editor.setTheme("ace/theme/one_dark");
        this._editor.session.setMode("ace/mode/scad");
    }

    disconnectedCallback() {
        this._editor = null;
    }

    attributeChangedCallback(name, oldValue, newValue) {
        console.log("VALUE:", name, oldValue, newValue);
        if (name == "value") {
            this._editor.setValue(newValue);
        }
    }

    setValue(text) {
        this._editor.setValue(text);
    }

    getValue() {
        return this._editor.getValue();
    }
};

ace.config.set("basePath", "/js");

customElements.define("ace-view", AceView);
