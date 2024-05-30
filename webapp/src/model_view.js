import * as THREE from "three";
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

/* This function decodes model from ScadSharp format to THREE.js
 *
 * JSONModel:
 * {
 *     "meshes": [
 *         {
 *             "name": "obj1",
 *             "geometry": {
 *                 "position": {
 *                     "itemSize": 3,
 *                     "type": "Float32Array",
 *                     "array": [ 0, 1, 2, 3... ]
 *                 },
 *                 "normal": {
 *                     "itemSize": 3,
 *                     "type": "Float32Array",
 *                     "array": [ 0, 1, 2, 3... ]
 *                 },
 *                 "uv": {
 *                     "itemSize": 2,
 *                     "type": "Float32Array",
 *                     "array": [ 0, 1, 2, 3... ]
 *                 },
 *                 "boundingSphere": {
 *                     "center": [ 0, 0, 0],
 *                     "radius": 10
 *                 }
 *             },
 *             "material": {
 *                 "color": 16777215,
 *                 ...
 *             }
 *         }
 *     ],
 *
 *     "images": []
 * }
 */
function decodeModel(json) {
    // TODO: images, ...
    let res = [];

    // We need to add all meshes:
    for (let m of json.meshes) {
        // Load geometry:
        let g = new THREE.BufferGeometry();
        g.setAttribute("position", new THREE.BufferAttribute(new Float32Array(m.geometry.position.array), 3));
        g.setAttribute("normal", new THREE.BufferAttribute(new Float32Array(m.geometry.normal.array), 3));
        g.setAttribute("uv", new THREE.BufferAttribute(new Float32Array(m.geometry.uv.array), 2));
        let material = new THREE.MeshBasicMaterial();
        material.color.setRGB(m.material.color.r, m.material.color.g, m.material.color.b);

        res.push(new THREE.Mesh(g, material));
    }

    return res;
}

const zUpMatrix = new THREE.Matrix4(
    1, 0, 0, 0,
    0, 0, 1, 0,
    0, 1, 0, 0,
    0, 0, 0, 1);

class ModelView extends HTMLElement {
    static observedAttributes = [ "name" ];

    constructor() {
        super();

        this._scene = new THREE.Scene();
        this._camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
        this.addEventListener("resize", (ev) => {
            console.log(ev);
        });
    }

    _render() {
        this._renderer = new THREE.WebGLRenderer({alpha: true});
        this._renderer.setSize(Math.max(this.offsetWidth, 640), Math.max(this.offsetHeight, 480));
        //this._renderer.setSize(640, 480);
        this.appendChild(this._renderer.domElement);

        this._addAux();

        this._camera.position.z = 10;
        const controls = new OrbitControls(this._camera, this._renderer.domElement);

        let animate;
        animate = () => {
            requestAnimationFrame(animate);
            controls.update();
            this._renderer.render(this._scene, this._camera);
        };
        animate();
    }

    _addAux()
    {
        // Grid:
        const size = 10;
        const divisions = 10;
        const gridHelper = new THREE.GridHelper(size, divisions);
        gridHelper.applyMatrix4(zUpMatrix);
        this._scene.add(gridHelper);

        // Axis:
        const axesHelper = new THREE.AxesHelper(15);
        axesHelper.applyMatrix4(zUpMatrix);
        this._scene.add(axesHelper);

        // Light:
        const light = new THREE.AmbientLight();
        this._scene.add(light);
    }

    connectedCallback() {
        this._render();
    }

    disconnectedCallback() {
        this._renderer = null;
    }

    attributeChangedCallback(name, oldValue, newValue) {
        console.log("NAME:", name, oldValue, newValue);
        if (name == "name") {
            this._name.innerText = newValue.value;
        }
    }

    // there can be other element methods and properties
    updateModel(model) {
        let models = decodeModel(model);
        // Clear the scene:
        this._scene.clear();
        this._addAux();

        for (let obj of models) {
            this._scene.add(obj);
        }
        return;
    }
}

customElements.define("model-view", ModelView);
