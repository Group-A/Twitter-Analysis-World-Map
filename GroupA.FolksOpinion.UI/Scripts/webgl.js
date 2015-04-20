/*
	File Name: webgl.js
	Author(s): Julian Reid
	Created: 22/01/2015
	Modified: 11/02/2015
	Version: 0.7
	Description:
		Renders the map in 3d using webgl.
*/

// WebGL globals
var gl, glCanvas,
	mapShader,
	activeShader,
    MAX_DELTA = 1 / 10,

	mvMatrix = mat4.create(),
	pMatrix = mat4.create(),

    // vertex buffers
	sphere = null,
	plane = null,

	lastTime = Date.now(),

	mapTexture = null,
	planetReady = false,
    cameraZ = -128,
	
	RENDER_2D = 0,
	RENDER_3D = 1,
	renderType = RENDER_3D;

// Keyboard data structure
var keyboard = {
    keys : [],
	keysLast : [],
    up  : 38,
    down : 40,
    left : 37,
    right : 39,
    control : 17,
	space : 32
}

// Mouse data structure
var mouse = {
	buttons : [false, false, false],
	x : -1,
	y : -1,
	LEFT : 0,
	RIGHT : 2,
	MIDDLE : 1
}

// The position of the 3D world
var worldPosition = {
    xAngle: 0,
    yAngle: 1,
	x: 0,
	y: 0,
    zoom: 1,
    targetXAngle: 2,
    targetYAngle: 0,
	targetX: 0,
	targetY: 0,
    targetZoom: 1.75
}

// Load an image and make a WebGL texture out
// of it
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

// Create a WebGL texture from an image
function textureFromImage(image, texture) {
    if (texture === undefined)
        texture = gl.createTexture();

    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
    // Smooth filtering
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

    gl.bindTexture(gl.TEXTURE_2D, null);

    return texture;
}

var boundTexture = null;

// Bind a texture to the GPU for drawing on a mesh
function bindTexture(texture) {
    if (boundTexture != texture) {
        boundTexture = texture;
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.uniform1i(activeShader.samplerUniform, 0);
    }
}

// Get the WebGL context from a canvas.
function getGLContext(canvas) {
    var gl = canvas.getContext("webgl");

    if (!gl)
        alert("Your browser does not support 3D WebGl rendering. Please" +
			  "upgrade to a newer version. We recommend the newest version" +
			  "of Firefox or Google Chrome");

    return gl;
}

// Resize the canvas to match the new window size
function onGLCanvasResized() {
    glCanvas.width = window.innerWidth;
    glCanvas.height = window.innerHeight;
}

// Compile a shader based on the passed in source code and type. (Fragment or Vertex)
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

// Create a shader program and attach fragment and vertex shaders compiled
// from the passed in source code.
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

// Binds a shader program to the GPU that rendering will pass through.
function useShader(shader) {
    gl.useProgram(shader);

    shader.vertexPositionAttribute = gl.getAttribLocation(shader, "aVertexPosition");
    gl.enableVertexAttribArray(shader.vertexPositionAttribute);

    shader.texturePositionAttribute = gl.getAttribLocation(shader, "aTexturePosition");
    gl.enableVertexAttribArray(shader.texturePositionAttribute);

    shader.normalPositionAttribute = gl.getAttribLocation(shader, "aNormalPosition");
    gl.enableVertexAttribArray(shader.normalPositionAttribute);

    shader.pMatrixUniform = gl.getUniformLocation(shader, "uPMatrix");
    shader.mvMatrixUniform = gl.getUniformLocation(shader, "uMVMatrix");
    shader.nMatrixUniform = gl.getUniformLocation(shader, "uNMatrix");

    activeShader = shader;
}

// Concatinate the source array on to the end of the dest array.
function arrayPush(dest, source) {
    for (var i = 0; i < source.length; ++i)
        dest.push(source[i]);
}

// Build the vertex data for a flat plane.
function buildQuad(width, height) {
	var w = width/2,
	    h = height / 2;
    // Positional data
	vertices = [-w, -h,  0,
				-w,  h,  0,
				 w, -h,  0,
				 w,  h,  0];
				
    // The facing direction of each vertex
	normals = [0, 0, 1,
			   0, 0, 1,
			   0, 0, 1,
			   0, 0, 1];
			   
    // Define the two trangles using the vertices
	indices = [0, 1, 2, 1, 2, 3];
	
    // Define the position of the texture on each vertex.
	textureCoords = [0, 1,
					 0, 0,
					 1, 1,
					 1, 0];
					 
	return new Model(vertices, indices, textureCoords, normals);
}

// Builds a sphere vertex buffer
function buildSphere() {
    vertices = [];
    normals = [];
    indices = [];
    textureCoords = [];

    // The resolution of the sphere. (In vertices)
    vSegs = 32;
    hSegs = 32;

    // For each vertex, calculate it's position using a cosine and sin wave.
    for (var i = 0; i <= vSegs; ++i)
        for (var t = 0; t <= hSegs; ++t) {
            arrayPush(vertices, [Math.cos((t / (hSegs)) * Math.PI * 2) * Math.sin((i / vSegs / 2) * 2 * Math.PI),
								 Math.cos((i / vSegs / 2) * Math.PI * 2),
								 Math.sin((t / hSegs) * Math.PI * 2) * Math.sin((i / vSegs / 2) * 2 * Math.PI)]);

            arrayPush(normals, [Math.cos((t / (hSegs)) * Math.PI * 2) * Math.sin((i / vSegs / 2) * 2 * Math.PI),
								 Math.cos((i / vSegs / 2) * Math.PI * 2),
								 Math.sin((t / hSegs) * Math.PI * 2) * Math.sin((i / vSegs / 2) * 2 * Math.PI)]);
        }

    // Generate a list indexes that make up all the triangles out of the position data.
    for (var i = 0; i <= vSegs - 1; ++i)
        for (var t = 0; t <= hSegs - 1; ++t) {
            arrayPush(indices, [(i * (hSegs + 1)) + t, (i * (hSegs + 1)) + t + 1, ((i + 1) * (hSegs + 1)) + t]);
            arrayPush(indices, [(i * (hSegs + 1)) + t + 1, ((i + 1) * (hSegs + 1)) + t, ((i + 1) * (hSegs + 1)) + (t + 1)]);
        }

    // Generate the texture coordinate of each of the vertices.
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

    return new Model(vertices, indices, textureCoords, normals);
}

// Clamp a number to a minimum and maximum vlaue
function clamp(number, min, max) {
    return number < min ? min : (number > max ? max : number);
}

var worldTransform = mat4.create();

// Draw the 3D map with the map texture applied.
function drawWorld(delta) {
    // Setup a viewport to draw into
    gl.viewport(0, 0, glCanvas.width, glCanvas.height);
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

    // Define the transformation from world to screen space.
    // The perspective is orthographic to make zooming look even.
    mat4.ortho(pMatrix,
               -2 * (glCanvas.width / glCanvas.height),
               2 * (glCanvas.width / glCanvas.height),
			   -2, 2, 0.001, 1000.0);

    // Draw the planet infront of the view point.
    mat4.identity(mvMatrix);
    mat4.translate(mvMatrix, mvMatrix, [0.0, 0.0, cameraZ]);

    if (planetReady) {
        mat4.multiply(mvMatrix, mvMatrix, worldTransform);

        // Upload the transform matrices to the GPU
        gl.uniformMatrix4fv(mapShader.pMatrixUniform, false, pMatrix);
        gl.uniformMatrix4fv(mapShader.mvMatrixUniform, false, mvMatrix);
        var nMatrix = mat3.create();
        // Calculate the normal matrix from the model view matrix
        mat3.normalFromMat4(nMatrix, mvMatrix);
        // Upload the normal matrix to the GPU
        gl.uniformMatrix3fv(mapShader.nMatrixUniform, false, nMatrix);

		// Choose the right model depending on 2D or 2D view.
		var model = sphere;
		if(renderType == RENDER_2D)	
			model = plane;
			
        // Bind the correct data for the chosen model for drawing.
        gl.bindBuffer(gl.ARRAY_BUFFER, model.positionBuffer);
        gl.vertexAttribPointer(mapShader.vertexPositionAttribute,
							   model.positionBuffer.itemSize,
							   gl.FLOAT, false, 0, 0);

        gl.bindBuffer(gl.ARRAY_BUFFER, model.texturePosBuffer);
        gl.vertexAttribPointer(mapShader.texturePositionAttribute,
							   model.texturePosBuffer.itemSize,
							   gl.FLOAT, false, 0, 0);

        gl.bindBuffer(gl.ARRAY_BUFFER, model.normalBuffer);
        gl.vertexAttribPointer(mapShader.normalPositionAttribute,
							   model.normalBuffer.itemSize,
							   gl.FLOAT, false, 0, 0);

        // Bind the map texture
        bindTexture(mapTexture);
			
        gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, model.indexBuffer);
        // Draw the model
        gl.drawElements(gl.TRIANGLES, model.indexBuffer.numItems, gl.UNSIGNED_SHORT, 0);
    }
}

// Initialize WebGL
function webGLStart() {
    // Get thecanvas
    glCanvas = document.getElementById("map");

    // Listen for any resizing of the window and adjust the canvas
    // accordingly.
    window.onresize = function () {
        onGLCanvasResized();
    }

    // Listen for keyboard events and store the key states
    window.addEventListener("keydown", function (e) {
        keyboard.keys[e.keyCode] = true;
    });

    window.addEventListener("keyup", function (e) {
        keyboard.keys[e.keyCode] = false;
    });
	
    // Disable right click context menus on the map.
	glCanvas.oncontextmenu = function(e) {
		return false;
	};
	
    // Listen for mouse events and store it's state
	glCanvas.addEventListener("mousedown", function(e) {
		mouse.buttons[e.button] = true;
	});
	
	glCanvas.addEventListener("mouseup", function(e) {
		mouse.buttons[e.button] = false;
	});
	
	glCanvas.addEventListener("mousemove", function(e) {
		if(mouse.x != -1)
		{
            // If the right mouse button is being pressed then zoom based on the mouse movement.
			if(mouse.buttons[mouse.RIGHT])
			{
				worldPosition.targetZoom -= (e.clientY - mouse.y) / 200 * worldPosition.zoom;
			}
            // If the left button is being pressed then rotate the world
			if(mouse.buttons[mouse.LEFT])
			{
				worldPosition.targetXAngle += (e.clientX - mouse.x) / 50 / worldPosition.zoom;
				worldPosition.targetYAngle += (e.clientY - mouse.y) / 50 / worldPosition.zoom;
			}
		}
		mouse.x = e.clientX;
		mouse.y = e.clientY;
	});

    // Get a GL context
    gl = getGLContext(glCanvas);

    // load the map shader
    mapShader = loadShader(document.getElementById("mapFrag").innerHTML,
					       document.getElementById("mapVert").innerHTML);
    useShader(mapShader);

    // Set a clear colour for the background
    gl.clearColor(2 / 255, 98 / 255, 138 / 255, 1);
    // Enable depth testing while drawing
    gl.enable(gl.DEPTH_TEST);

    onGLCanvasResized();
    drawLoop();
}

// Build the vertex buffer for the planet (Both a sphere and a plane for the
// two different representations.
function buildPlanet(rebuildVbo) {
	rebuildVbo = rebuildVbo == undefined ? true : rebuildVbo;
	
    mapTexture = gl.createTexture();
    textureFromImage(mapCanvas, mapTexture);
	if(rebuildVbo)
	{
		sphere = buildSphere()//buildQuad(2, 1);
		plane = buildQuad(4, 2);
	}
    planetReady = true;
}

// Draw the map to the screen and re-call it's self
// to run again on the next screen update
function drawLoop() {
    // Request that this function be called again on the
    // next frame
    window.requestAnimationFrame(drawLoop);

    // Get the time that has passed since the last frame
    var currentTime = Date.now();
    var delta = (currentTime - lastTime) / 1000;
    lastTime = currentTime;

    delta = Math.min(delta, MAX_DELTA);

    // Create a matrix for storing the transformations of the world
    worldTransform = mat4.create();

    var rotateSpeed = 4;

    // Rotate the world based on the keys being pressed
    if (keyboard.keys[keyboard.left])
        worldPosition.targetXAngle -= delta * rotateSpeed / worldPosition.zoom;

    if (keyboard.keys[keyboard.right])
        worldPosition.targetXAngle += delta * rotateSpeed / worldPosition.zoom;

    if (keyboard.keys[keyboard.up]) {
        if (keyboard.keys[keyboard.control]) // Zoom in or out if control is held
            worldPosition.targetZoom += delta * worldPosition.targetZoom;
        else
            worldPosition.targetYAngle -= delta * rotateSpeed / worldPosition.zoom;
    }
	
    // Switch renderers if the space button is pressed.
	if(keyboard.keys[keyboard.space] && !keyboard.keysLast[keyboard.space])
		renderType = renderType == RENDER_2D ? RENDER_3D : RENDER_2D;

    if (keyboard.keys[keyboard.down]) {
        if (keyboard.keys[keyboard.control]) // Zoom out if the control key is held
            worldPosition.targetZoom -= delta * worldPosition.targetZoom;
        else
            worldPosition.targetYAngle += delta * rotateSpeed / worldPosition.zoom;
    }
	
    // Clamp the zoom and the Y rotation values to resonable amounts.
	worldPosition.targetZoom = clamp(worldPosition.targetZoom, 1, 100);
	worldPosition.targetYAngle = clamp(worldPosition.targetYAngle, -1.1, 1.1);

    // Smoothly ease the world to it's new position
    worldPosition.zoom += (worldPosition.targetZoom - worldPosition.zoom) * delta * 4;
    worldPosition.xAngle += (worldPosition.targetXAngle - worldPosition.xAngle) * delta * 4;
    worldPosition.yAngle += (worldPosition.targetYAngle - worldPosition.yAngle) * delta * 4;

    // Scale the matrix based on the zoom value.
    mat4.scale(worldTransform, worldTransform,
               [worldPosition.zoom,
                worldPosition.zoom,
                worldPosition.zoom]);
	
    // Depending on the type of renderer translate or rotate the map.
	if(renderType == RENDER_3D) // Rotate 3D map
	{
		mat4.rotate(worldTransform, worldTransform, worldPosition.xAngle, [0, 1, 0]);
		mat4.rotate(worldTransform, worldTransform, worldPosition.yAngle,
					[Math.cos(worldPosition.xAngle), 0, Math.sin(worldPosition.xAngle)]);
	}
	else if(renderType == RENDER_2D) // Translate 2D map
	{
		mat4.translate(worldTransform, worldTransform, [worldPosition.x, -worldPosition.y, 0]);
	}

    // Draw the world
    drawWorld(delta);
	
    // Take not of the keys pressed on this frame for
    // using next frame.
	keyboard.keysLast = keyboard.keys.slice(0);
}



// Model ----------------------------------------------------------------------
// Contains all the information about a model including positions, texture coords,
// normals and indexes.
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

    this.normalBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, this.normalBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(normals), gl.STATIC_DRAW);
    this.normalBuffer.itemSize = 3;
    this.normalBuffer.numItems = normals.length / this.normalBuffer.itemSize;
}

Model.prototype =
{
    positionBuffer: null,
    indexBuffer: null,
    texturePosBuffer: null,
    normalBuffer: null,
}