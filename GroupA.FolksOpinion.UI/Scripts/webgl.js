/*
	File Name: webgl.js
	Author(s): Julian Reid
	Created: 22/01/2015
	Modified: 11/02/2015
	Version: 0.7
	Description:
		Renders the map in 3d using webgl.
*/

// TODO: Implement alternative 2D Quad view for map.

// TODO: Tidy up all this mess. Too many globals. :(
var gl, glCanvas,
	mapShader,
	activeShader,

	mvMatrix = mat4.create(),
	pMatrix = mat4.create(),

	sphere = null,
	lastTime = Date.now(),

	mapTexture = null,
	planetReady = false,
    cameraZ = -128;

var keyboard = {
    keys: [],
    up: 38,
    down: 40,
    left: 37,
    right: 39,
    control: 17
}

var worldPosition = {
    xAngle: 0,
    yAngle: 1,
    zoom: 1,
    targetXAngle: 2,
    targetYAngle: 0,
    targetZoom: 1.75
}

function loadTexture(url) {
    var texture = gl.createTexture();
    image = new Image();

	image.onload = function()
	{
		textureFromImage(image, texture);
	}
	
	image.src = url;

    return texture;
}

function textureFromImage(image, texture) {
    if (texture === undefined)
        texture = gl.createTexture();

    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

    gl.bindTexture(gl.TEXTURE_2D, null);

    return texture;
}

var boundTexture = null;

function bindTexture(texture) {
    if (boundTexture != texture) {
        boundTexture = texture;
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.uniform1i(activeShader.samplerUniform, 0);
    }
}

function getGLContext(canvas) {
    var gl = canvas.getContext("webgl");

    if (!gl)
        alert("Your browser does not support 3D WebGl rendering. Please" +
			  "upgrade to a newer version. We recommend the newest version" +
			  "of Firefox or Google Chrome");

    return gl;
}

function onGLCanvasResized() {
    glCanvas.width = window.innerWidth;
    glCanvas.height = window.innerHeight;
}

function compileShader(source, type) {
    shader = gl.createShader(type);

    gl.shaderSource(shader, source);
    gl.compileShader(shader);

    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
        console.log("Error compiling shader: " + gl.getShaderInfoLog(shader));
        return null;
    }

    return shader;
}

function loadShader(fragmentShaderSource, vertexShaderSource) {
    var fragmentShader = compileShader(fragmentShaderSource, gl.FRAGMENT_SHADER);
    var vertexShader = compileShader(vertexShaderSource, gl.VERTEX_SHADER);

    var shader = gl.createProgram();
    gl.attachShader(shader, vertexShader);
    gl.attachShader(shader, fragmentShader);
    gl.linkProgram(shader);

    if (!gl.getProgramParameter(shader, gl.LINK_STATUS))
        console.log("Could not initialise shaders");

    return shader;
}

function useShader(shader) {
    gl.useProgram(shader);

    shader.vertexPositionAttribute = gl.getAttribLocation(shader, "aVertexPosition");
    gl.enableVertexAttribArray(shader.vertexPositionAttribute);

    shader.texturePositionAttribute = gl.getAttribLocation(shader, "aTexturePosition");
    gl.enableVertexAttribArray(shader.texturePositionAttribute);

    shader.pMatrixUniform = gl.getUniformLocation(shader, "uPMatrix");
    shader.mvMatrixUniform = gl.getUniformLocation(shader, "uMVMatrix");

    activeShader = shader;
}

function arrayPush(dest, source) {
    for (var i = 0; i < source.length; ++i)
        dest.push(source[i]);
}

function buildSphere() {
    vertices = [];
    indices = [];
    textureCoords = [];

    vSegs = 16;
    hSegs = 16;

    for (var i = 0; i <= vSegs; ++i)
        for (var t = 0; t <= hSegs; ++t) {
            arrayPush(vertices, [Math.cos((t / (hSegs)) * Math.PI * 2) * Math.sin((i / vSegs / 2) * 2 * Math.PI),
								 Math.cos((i / vSegs / 2) * Math.PI * 2),
								 Math.sin((t / hSegs) * Math.PI * 2) * Math.sin((i / vSegs / 2) * 2 * Math.PI)]);
        }

    for (var i = 0; i <= vSegs - 1; ++i)
        for (var t = 0; t <= hSegs - 1; ++t) {
            arrayPush(indices, [(i * (hSegs + 1)) + t, (i * (hSegs + 1)) + t + 1, ((i + 1) * (hSegs + 1)) + t]);
            arrayPush(indices, [(i * (hSegs + 1)) + t + 1, ((i + 1) * (hSegs + 1)) + t, ((i + 1) * (hSegs + 1)) + (t + 1)]);
        }


    tx = 0.1;
    ty = 0;
    tw = 0.85;
    th = 1.2;
    for (var i = 0; i <= vSegs; ++i)
        for (var t = 0; t <= hSegs; ++t) {
            var x = (tw * (1 - (t / hSegs))) + tx,
				y = (th * (i / vSegs)) + ty;
            arrayPush(textureCoords,
					  [clamp(x, 0, 1),
					   clamp(y, 0, 1)]);
        }

    return new Model(vertices, indices, textureCoords);
}

function clamp(number, min, max) {
    return number < min ? min : (number > max ? max : number);
}

var worldTransform = mat4.create();

function drawWorld(delta) {

    gl.viewport(0, 0, glCanvas.width, glCanvas.height);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    mat4.ortho(pMatrix,
               -2 * (glCanvas.width / glCanvas.height),
               2 * (glCanvas.width / glCanvas.height),
			   -2, 2, 0.001, 1000.0);

    mat4.identity(mvMatrix);
    mat4.translate(mvMatrix, mvMatrix, [0.0, 0.0, cameraZ]);

    if (planetReady) {
        mat4.multiply(mvMatrix, mvMatrix, worldTransform);

        gl.uniformMatrix4fv(mapShader.pMatrixUniform, false, pMatrix);
        gl.uniformMatrix4fv(mapShader.mvMatrixUniform, false, mvMatrix);

        gl.bindBuffer(gl.ARRAY_BUFFER, sphere.positionBuffer);
        gl.vertexAttribPointer(mapShader.vertexPositionAttribute,
							   sphere.positionBuffer.itemSize,
							   gl.FLOAT, false, 0, 0);

        gl.bindBuffer(gl.ARRAY_BUFFER, sphere.texturePosBuffer);
        gl.vertexAttribPointer(mapShader.texturePositionAttribute,
							   sphere.texturePosBuffer.itemSize,
							   gl.FLOAT, false, 0, 0);

        bindTexture(mapTexture);

        gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, sphere.indexBuffer);
        gl.drawElements(gl.TRIANGLES, sphere.indexBuffer.numItems, gl.UNSIGNED_SHORT, 0);
    }
}

function webGLStart() {
    glCanvas = document.getElementById("map");

    window.onresize = function () {
        onGLCanvasResized();
    }

    window.addEventListener("keydown", function (e) {
        keyboard.keys[e.keyCode] = true;
    });

    window.addEventListener("keyup", function (e) {
        keyboard.keys[e.keyCode] = false;
    });

    gl = getGLContext(glCanvas);

    mapShader = loadShader(document.getElementById("mapFrag").innerHTML,
					       document.getElementById("mapVert").innerHTML);
    useShader(mapShader);

    gl.clearColor(0, 138 / 255, 168/255, 168);
    gl.enable(gl.DEPTH_TEST);

    onGLCanvasResized();
    drawLoop();
}

function buildPlanet() {
    mapTexture = gl.createTexture();
    textureFromImage(mapCanvas, mapTexture);
    sphere = buildSphere();
    planetReady = true;
}

function drawLoop() {
    window.requestAnimationFrame(drawLoop);

    var currentTime = Date.now();
    var delta = (currentTime - lastTime) / 1000;
    lastTime = currentTime;

    worldTransform = mat4.create();

    var rotateSpeed = 4;

    if (keyboard.keys[keyboard.left])
        worldPosition.targetXAngle -= delta * rotateSpeed / worldPosition.zoom;

    if (keyboard.keys[keyboard.right])
        worldPosition.targetXAngle += delta * rotateSpeed / worldPosition.zoom;

    if (keyboard.keys[keyboard.up]) {
        if (keyboard.keys[keyboard.control])
            worldPosition.targetZoom += delta * worldPosition.targetZoom;
        else
            worldPosition.targetYAngle -= delta * rotateSpeed / worldPosition.zoom;
    }

    if (keyboard.keys[keyboard.down]) {
        if (keyboard.keys[keyboard.control])
            worldPosition.targetZoom -= delta * worldPosition.targetZoom;
        else
            worldPosition.targetYAngle += delta * rotateSpeed / worldPosition.zoom;
    }

    worldPosition.zoom += (worldPosition.targetZoom - worldPosition.zoom) * delta * 4;
    worldPosition.xAngle += (worldPosition.targetXAngle - worldPosition.xAngle) * delta * 4;
    worldPosition.yAngle += (worldPosition.targetYAngle - worldPosition.yAngle) * delta * 4;

    mat4.scale(worldTransform, worldTransform,
               [worldPosition.zoom,
                worldPosition.zoom,
                worldPosition.zoom]);

    mat4.rotate(worldTransform, worldTransform, worldPosition.xAngle, [0, 1, 0]);
    mat4.rotate(worldTransform, worldTransform, worldPosition.yAngle,
                [Math.cos(worldPosition.xAngle), 0, Math.sin(worldPosition.xAngle)]);

    drawWorld(delta);
}



// Model ----------------------------------------------------------------------

function Model(vertices, indices, textureCoords, normals) {
    this.positionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, this.positionBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
    this.positionBuffer.itemSize = 3;
    this.positionBuffer.numItems = vertices.length / this.positionBuffer.itemSize;

    this.indexBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.indexBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(indices), gl.STATIC_DRAW);
    this.indexBuffer.itemSize = 1;
    this.indexBuffer.numItems = indices.length / this.indexBuffer.itemSize;

    this.texturePosBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, this.texturePosBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(textureCoords), gl.STATIC_DRAW);
    this.texturePosBuffer.itemSize = 2;
    this.texturePosBuffer.numItems = textureCoords.length / this.texturePosBuffer.itemSize;
}

Model.prototype =
{
    positionBuffer: null,
    indexBuffer: null,
    texturePosBuffer: null,
    normalBuffer: null,
}