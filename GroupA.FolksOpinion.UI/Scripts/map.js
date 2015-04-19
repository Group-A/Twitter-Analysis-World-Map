/*
	File Name: map.js
	Author(s): Julian Reid, Jamie Aitken
	Created: 22/01/2015
	Modified: 11/02/2015
	Version: 0.7
	Description:
		Renders a map with tinted countries that represent opinion data from 
		twitter. Provides zooming and panning functionality. Also displays
		text based percentages and statistics when zoomed far enough in.
*/

// Global variables
var ctx, canvas;
var mapCanvas, mapCtx;

var countries = {};
var negativeColor, neutralColor, positiveColor, backgroundColor;

var mapWidth = 1020,
	mapHeight = 660;

var mapTextureWidth = 1024 * 4,
	mapTextureHeight = 512 * 4;

window.onload = function () {
    // The colours of the map
    negativeColor = new Color(227, 27, 50);
    neutralColor = new Color(255, 255, 255);
    positiveColor = new Color(56, 215, 89);
    backgroundColor = new Color(0, 0, 0, 0);

    // The canvas for rendering the map texture to
    mapCanvas = document.createElement("canvas");
    mapCanvas.width = mapTextureWidth;
    mapCanvas.height = mapTextureHeight;
    mapCtx = mapCanvas.getContext("2d");

    // Load the external svg file
    loadSvg("/Content/worldLow.svg", function (content) {
        // Once loaded parse the svg
        parseSvg(content);
        // Render the map from the svg
        setTimeout(renderMap, 10);
    });

    // Start the WebGL renderer
    webGLStart()
}

// Render the map to its texture and build the vertex buffer for the planet.
function renderMap(rebuildVbo) {
	rebuildVbo = rebuildVbo == undefined ? true : rebuildVbo;
    drawRawMap(mapCanvas, mapCtx);
    buildPlanet(rebuildVbo);
}

// Parse the passed in country JSON data
function parseJSONData(string) {

    // Zero out any previous results
    for (var i in countries)
        countries[i].attitude = 0;

    // parse the json
    var data = JSON.parse(string);
    if (data.CountryOpinions == "")
    {
        // Show an error if no opinions were received
        alert("Not enough data was gathered " + decodeURIComponent(data.Subject));
    }
    else
    {
        // Read opinions 
        for (var i in data.CountryOpinions) {
            var opinion = data.CountryOpinions[i];

            // Generate an attitude based on passed in data and
            // assign it to the correct data
            countries[opinion.Country].attitude =
                opinion.Opinion.PositiveBias
              - opinion.Opinion.NegativeBias;
        }
    }
    // Generate a screen reader friendly table from the data
    createScreenReader(data);
    
}

// Initiate a search with the passed in element's value
function searchCustomTerm(event) {
    if (event.keyCode == 13) {
        var topic = document.getElementById("customSearchTermField").value;

        if (topic.length > 0) {
            requestTopic(topic);
        }
    }
}

// Request country opinion data on the topic passed in.
function requestTopic(topic) {
    // Create a http request for the option data
    var searchField = document.getElementById("customSearchTermField");
    var url = "/Data/Opinion?q=" + encodeURIComponent(topic);
    var request = newRequest();
    // If the request is processing then show a processing gif.
    if (request.readyState < 4) {
        searchField.style.backgroundImage = "~/Content/Images/process.gif";
    }
    // Execute the request
    request.open("GET", url, true);
    request.setRequestHeader("Content-Type", "application/json");
    request.withCredentials = true;

    // Upon loading the new topic data
    request.onload = function (e) {
        // Clear the spinner
        searchField.style.backgroundImage = "~/Content/Images/customSearchTermFieldBackground.png";
        // Clear old screen reader
        clearScreenReader();
        // Read the new data and parse it
        parseJSONData(request.response);
        // Re-draw the map texture
        renderMap(false);
    }.bind(this);

    request.send();
}

// Create the screen reader elements
function createScreenReader(data)
{
    // Create a table to hold the data
    var table = document.getElementById("reader");
    // If no data was found then show error message.
    if (data.CountryOpinions == "")
    {
        table.innerHTML = "<tr><td>Not enough data gathered for "+decodeURIComponent(data.Subject)+", please select another subject</td></tr>";
    }
    else { // Otherwise display data in table
        table.innerHTML += "<tr><td>Subject</td><td>Country</td><td>Attitude</td><td>Positive</td><td>Negative</td>";
        for (var i in data.CountryOpinions) {
            var opinion = data.CountryOpinions[i];
            table.innerHTML += "<td>" + decodeURIComponent(data.Subject) + "</td><td>" + opinion.Country + "</td><td>" + Math.floor((opinion.Opinion.PositiveBias - opinion.Opinion.NegativeBias) * 100) / 100 + "</td><td>" + opinion.Opinion.PositiveBias + "</td><td>" + opinion.Opinion.NegativeBias + "</td>";
        }
        table.innerHTML += "</tr>";
    }
    
}

// Remove the old screen reader data
function clearScreenReader()
{
    var table = document.getElementById("reader");
    table.innerHTML = "";
}

// Create a cross-platform request object
function newRequest() {
    var request = null;

    if (window.XMLHttpRequest)
        request = new XMLHttpRequest();
    else
        request = new ActiveXObject("Microsoft.XMLHTTP");

    if (!request)
        console.log("ERROR: Request object could not be created");

    return request;
}

// Load an svg file and execute the callback when loading is done.
function loadSvg(url, callback) {
    var request = newRequest();

    request.open("GET", url, true);

    request.onload = function (e) {
        callback(request.response);
    }.bind(this);

    request.send();
}

// Parse the svg file for it's contents
function parseSvg(content, countryCode) {
    var parser = new DOMParser();
    svg = parser.parseFromString(content, "text/xml");

    var paths = svg.getElementsByTagName("path");

    // For each path in the svg file
    for (var p = 0; p < paths.length; ++p) {
        var code = paths[p].getAttribute("id");

        if (countryCode === undefined || code == countryCode) {
            // Get each of the polygons that make up the path
            var polygons = parsePath(paths[p].getAttribute("d"));
            // Create a country that is made up of the loaded polygons
            var country = new Country(polygons, 0);
            // Calculate a center for the country and get it's name.
            country.calculateCenter();
            country.setName(code, paths[p].getAttribute("title"));
            // Add the country to the list
            countries[code] = country;
        }
    }
}

// Parse a path in an SVG document. Takes the path as a string
function parsePath(dataString) {
    // Split the data appropriatly into individual instructions
    var data = dataString.split(/(?=[a-zA-Z])/);

    // The position that relative coordinates will calculate from.
    var position = null;

    // Create a list of polygons and a first one to write to.
    var polygons = [];
    polygons[0] = new Polygon();
    var polygonCount = 0;

    // For each instruction in the path
    for (var i = 0; i < data.length; ++i) {
        // Parse the instruction
        result = parseInstruction(data, i, position);

        // If the end of the path was reached then end the polygon.
        if (result.endOfPath == true) {
            position = null;
            if (i < data.length - 1) {
                result.newPolygon = true;
            }
            else // Polygon is already empty so keep it
                break;
        }
        else {
            // Don't create a new polygon if this is the first vertex.
            if (result.newPolygon && i != 0) {
                polygonCount++;
                polygons[polygonCount] = new Polygon();
            }

            // If the end of the path has not been reached then add the new point
            // to the polygon.
            if (!result.endOfPath) {
                position = result.position;
                polygons[polygonCount].addVertex(new Vertex(position.x, position.y));
            }
        }
    }

    return polygons;
}

// Parse an SVG path instruction and return the resulting point
function parseInstruction(data, index, position)
{
    // Get the instruction at the index passed
    var string = data[index];
    var instruction = string.charAt(0);
    // If the character is lower case then the instruction is
    // relative to the current position
    var relative = !isUpperCase(instruction);

    // Split the coordinates of the instruction
    var stringCoords = string.slice(1).split(",");

    // Build a result data structure.
    var result = {
        position: {
            x: 0,
            y: 0
        },
        endOfPath: false,
        newPolygon: false
    }

    // Handle the instruction
    switch (instruction.toLowerCase()) {
        case "h": // Horizontal Movement
            result.position.x = parseFloat(stringCoords[0]);
            break;

        case "v": // Vertical Movement
            result.position.y = parseFloat(stringCoords[0]);
            break;

        case "z": // End of path reached
            result.endOfPath = true;
            break;

        case "l": // Line to / Move to
            result.position.x = parseFloat(stringCoords[0]);
            result.position.y = parseFloat(stringCoords[1]);
            break;

        case "m":
            result.position.x = parseFloat(stringCoords[0]);
            result.position.y = parseFloat(stringCoords[1]);
            result.newPolygon = true;
            break;

        default: // An invalid instruction was reached
            Console.log("Invalid instruction: " + instruction);
            result.endOfPath = true;
            break;
    }

    // If relative then add to the old position
    if (relative) {
        if (position == null)
            console.log("Error: Map reading failed. Relative position requested from no previous position.");

        result.position.x += position.x;
        result.position.y += position.y;
    }

    return result;
}

// Draw a country to a 2d context
function drawCountry(ctx, country)
{
    // Get the countries polygons and attitude
    var polygons = country.polygons;
    var attitude = country.attitude;

    // Generate a colour form the attitude of the country.
    if (attitude > 0)
        ctx.fillStyle = neutralColor.blend(positiveColor, attitude).getHex();
    else
        ctx.fillStyle = neutralColor.blend(negativeColor, Math.abs(attitude)).getHex();

    // Draw each of the polygons in the country
    for (var i = 0; i < polygons.length; ++i) {
        var polygon = polygons[i];

        ctx.beginPath();
        ctx.moveTo(polygon.vertices[0].x, polygon.vertices[0].y);

        // "The Canada hack" Fixes Canada
        if (country.code == "CA") {
            for (var t = 1; t < polygon.vertices.length; ++t) {
                var x = polygon.vertices[t].x;
                if (x < 20)
                    x += 135;
                ctx.lineTo(x, polygon.vertices[t].y);
            }
        }
        else {
            for (var t = 1; t < polygon.vertices.length; ++t)
                ctx.lineTo(polygon.vertices[t].x, polygon.vertices[t].y);
        }

        // Fill all the polygons
        ctx.fill();

        ctx.save();
        // draw the lines between countries as transparency
        ctx.globalCompositeOperation = "destination-out";
        ctx.strokeStyle = backgroundColor.getRGBAStringFlippedA();
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.restore();
    }
}

// Draws the map to a canvas
function drawRawMap(canvas, ctx) {
    // clear the old texture.
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.save();

    // Scalte the map to fill the texture.
    var hScale = canvas.width / mapWidth,
		vScale = canvas.height / mapHeight

    var scale = Math.min(hScale, vScale);

    ctx.scale(scale, scale);

    // Keep aspect ratio
    if (hScale > vScale)
        ctx.translate(((canvas.width - (mapWidth * scale)) * 0.5) / scale, 0);
    else
        ctx.translate(0, ((canvas.height - (mapHeight * scale)) * 0.5) / scale);

    // Draw all the countries
    for (var c in countries)
        drawCountry(ctx, countries[c]);

    ctx.restore();
}

// Check if a string is upper case
function isUpperCase(string) {
    return string.toUpperCase() == string;
}



// Class: Country -------------------------------------------------------------
// Used to store a group of polygons that make up a country and the attitude that
// the country has towards the topic.

function Country(polygons, attitude) {
    this.polygons = polygons;
    this.attitude = attitude === undefined ? 0 : attitude;
    this.center = new Vector2();
    this.name = "";
    this.code = "";
}

Country.prototype = {
    polygons: null,
    attitude: 0,
    center: null,

    // Sets the name of the country
    setName: function (code, name) {
        this.code = code;
        this.name = name;
    },

    // Calculates the center of the country based on the average
    // position of all it's vertices.
    calculateCenter: function () {
        var largestPolygon = null;
        var largestPolygonArea = 0;
        for (var i = 0; i < this.polygons.length; ++i) {
            var area = this.polygons[i].estimateAreaValue();
            if (area > largestPolygonArea) {
                largestPolygon = this.polygons[i];
                largestPolygonArea = area;
            }
        }
        this.center = largestPolygon.center;
    }
}



// Class: Polygon -------------------------------------------------------------

function Polygon(data) {
    this.vertices = [];
    this.indices = [];

    if (data != undefined) {
        for (var i = 0; i < data.length; ++i) {
            this.vertices.push(new Vertex(data[i], data[++i]));
            this.indices.push(i);
        }
    }
}

Polygon.prototype =
{
    vertices: null,
    indices: null,
    center: null,

    // Add a vertex to the polygon
    addVertex: function (vertex) {
        this.vertices.push(vertex);
    },

    // Add an index to the polygon
    addIndice: function (indice) {
        this.indices.push(indice);
    },

    // Draw the polygon to a 2D context
    draw: function (ctx) {
        var vertex = vertices[0];

        ctx.beginPath();
        ctx.moveTo(vertex.x, vertex.y);
        for (var i = 1; i < this.indices.length; ++i) {
            vertex = vertices[indices[i]];
            vertex.lineTo(vertex.x, vertex.y);
        }
        ctx.stroke();
    },

    // Calculate the center of this polygon based on all it's vertices.
    calculateCenter: function () {
        var center = new Vector2();
        for (var i = 0; i < this.vertices.length; ++i) {
            center.x += this.vertices[i].x;
            center.y += this.vertices[i].y;
        }

        center.x /= this.vertices.length;
        center.y /= this.vertices.length;

        this.center = center;
    },

    // Estimate the area of the polygon relative to others.
    estimateAreaValue: function () {
        // This function is only useful for comparing areas between
        // multiple polygons. Probably should implement actual area
        // algorithm.

        if (this.center == null)
            this.calculateCenter();
        var totalDist = 0;
        for (var i = 0; i < this.vertices.length; ++i)
            totalDist += this.center.distance(this.vertices[i]);
        return totalDist / this.vertices.length;
    }
}



// Class: Vertex --------------------------------------------------------------
// For storing the position of a vertex.

function Vertex(x, y) {
    this.x = x == undefined ? 0 : x;
    this.y = y == undefined ? 0 : y;
}

Vertex.prototype =
{
    x: 0,
    y: 0,
}



// Class: Vector2 -------------------------------------------------------------
// For storing a 2D vector

function Vector2(x, y) {
    this.x = x == undefined ? 0 : x;
    this.y = y == undefined ? 0 : y;
}

Vector2.prototype =
{
    x: 0,
    y: 0,

    // The distance between this vector and another. If no other
    // is passed then the distance is from 0, 0
    distance: function (other) {
        if (other === undefined)
            other = new Vector2();

        var dx = other.x - this.x;
        var dy = other.y - this.y;

        return Math.sqrt((dx * dx) + (dy * dy));
    }
}



// Class: Color ---------------------------------------------------------------
// Holds a 32bit colour

function Color(r, g, b, a) {
    this.set(r, g, b, a);
}

Color.prototype =
{
    r: 0,
    g: 0,
    b: 0,
    a: 255,

    // Sets the value of the color. (The alpha is optional)
    set: function (r, g, b, a) {
        a = a === undefined ? 255 : a;

        this.r = Math.round(r);
        this.g = Math.round(g);
        this.b = Math.round(b);
        this.a = Math.round(a);
    },

    // Get the hexdecimal value of the colour.
    getHex: function () {
        var values = [];

        values.push(this.r.toString(16));
        values.push(this.g.toString(16));
        values.push(this.b.toString(16));

        var output = "#";

        for (var i = 0; i < 3; ++i) {
            if (values[i].length < 2)
                values[i] = "0" + values[i];
            output += values[i];
        }

        return output;
    },

    // Get the RGBA string representation of this color
    getRGBAString: function () {
        return "rgba("
		     + (this.r / 1) + ", "
			 + (this.g / 1) + ", "
			 + (this.b / 1) + ", "
			 + (this.a / 255) + ")";
    },

    // Get the RGBA with an inverse a from this color
    getRGBAStringFlippedA: function () {
        return "rgba("
		     + (this.r / 1) + ", "
			 + (this.g / 1) + ", "
			 + (this.b / 1) + ", "
			 + (1 - (this.a / 255)) + ")";
    },

    // Blend this and another color together with a weight value
    // from 0 to 1.
    blend: function (other, weight) {
        var dr = other.r - this.r,
			dg = other.g - this.g,
			db = other.b - this.b,
			da = other.a - this.a;

        return new Color(this.r + (dr * weight),
						 this.g + (dg * weight),
						 this.b + (db * weight),
						 this.a + (dr * weight));
    }
}



// Class: MouseState ----------------------------------------------------------
// Stores the state of the mouse.

function MouseState() {
    this.buttons = [];
}

MouseState.prototype = {
    x: 0,
    y: 0,
    wheelPosition: 0,
    buttons: null,

    // Return a new copy of this state.
    clone: function () {
        clone = new MouseState();
        clone.x = this.x;
        clone.y = this.y;
        clone.wheelPosition = this.wheelPosition;
        clone.buttons = this.buttons.slice(0);
    }
}