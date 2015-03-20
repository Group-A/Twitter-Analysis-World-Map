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

var ctx, canvas;
var mapCanvas, mapCtx;

var countries = {};
var negativeColor, neutralColor, positiveColor, backgroundColor;

// TODO: move this definition somewhere better. (Maybe svg?)
var mapWidth = 1020,
	mapHeight = 660;

var mapTextureWidth = 1024 * 4,
	mapTextureHeight = 512 * 4;

window.onload = function () {
    negativeColor = new Color(227, 27, 50);
    neutralColor = new Color(255, 255, 255);
    positiveColor = new Color(56, 215, 89);
    backgroundColor = new Color(0, 0, 0, 0);

    mapCanvas = document.createElement("canvas");
    mapCanvas.width = mapTextureWidth;
    mapCanvas.height = mapTextureHeight;
    mapCtx = mapCanvas.getContext("2d");

    loadSvg("/Content/worldLow.svg", function (content) {
        parseSvg(content);
        setTimeout(renderMap, 10); // Allow time to avoid collisions
    });

    webGLStart()
}

function renderMap() {
    drawRawMap(mapCanvas, mapCtx);
    buildPlanet(); //TODO: why is this here?
}

function parseJSONData(string) {
    for (var i in countries)
        countries[i].attitude = 0;

    var data = JSON.parse(string);
    if (data.CountryOpinions == "")
    {
        alert("Not enough data was gathered "+decodeURIComponent(data.Subject));
    }
    else
    {
        for (var i in data.CountryOpinions) {
            var opinion = data.CountryOpinions[i];

            countries[opinion.Country].attitude =
                opinion.Opinion.PositiveBias
              - opinion.Opinion.NegativeBias;
        }
        createScreenReader(data);
    }
    
}

function searchCustomTerm(event) {
    if (event.keyCode == 13) {
        var topic = document.getElementById("customSearchTermField").value;

        if (topic.length > 0) {
            requestTopic(topic);
        }
    }
}

function requestTopic(topic) {
    var searchField = document.getElementById("customSearchTermField");
    var url = "/Data/Opinion?q=" + encodeURIComponent(topic);
    var request = newRequest();
    if (request.readyState < 4) {
        searchField.style.backgroundImage = "~/Content/Images/process.gif";
    }
    request.open("GET", url, true);
    request.setRequestHeader("Content-Type", "application/json");
    request.withCredentials = true;

    request.onload = function (e) {
        searchField.style.backgroundImage = "~/Content/Images/customSearchTermFieldBackground.png";
        clearScreenReader();
        parseJSONData(request.response);
        renderMap();
    }.bind(this);

    request.send();
}

function createScreenReader(data)
{
    var table = document.getElementById("reader");
    table.innerHTML += "<tr><td>Subject</td><td>Country</td><td>Attitude</td><td>Positive</td><td>Negative</td>";
    for(var i in data.CountryOpinions)
    {
        var opinion = data.CountryOpinions[i];
        table.innerHTML += "<td>"+decodeURIComponent(data.Subject)+"</td><td>" + opinion.Country + "</td><td>" + Math.floor((opinion.Opinion.PositiveBias - opinion.Opinion.NegativeBias)*100)/100 + "</td><td>" + opinion.Opinion.PositiveBias + "</td><td>" + opinion.Opinion.NegativeBias + "</td>";
    }
    table.innerHTML += "</tr>";
}

function clearScreenReader()
{
    var table = document.getElementById("reader");
    table.innerHTML = "";
}

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

function loadSvg(url, callback) {
    var request = newRequest();

    request.open("GET", url, true);

    request.onload = function (e) {
        callback(request.response);
    }.bind(this);

    request.send();
}

function parseSvg(content, countryCode) {
    var parser = new DOMParser();
    svg = parser.parseFromString(content, "text/xml");

    var paths = svg.getElementsByTagName("path");

    for (var p = 0; p < paths.length; ++p) {
        var code = paths[p].getAttribute("id");

        if (countryCode === undefined || code == countryCode) {
            var polygons = parsePath(paths[p].getAttribute("d"));
            var country = new Country(polygons, 0);
            country.calculateCenter();
            country.setName(code, paths[p].getAttribute("title"));
            countries[code] = country;
        }
    }
}

function parsePath(dataString) {
    var data = dataString.split(/(?=[a-zA-Z])/);

    // The position that relative coordinates will calculate from.
    var position = null;

    var polygons = [];
    polygons[0] = new Polygon();
    var polygonCount = 0;

    for (var i = 0; i < data.length; ++i) {
        result = parseInstruction(data, i, position);

        if (result.endOfPath == true) {
            position = null;
            if (i < data.length - 1) {
                result.newPolygon = true;
            }
            else
                break;
        }
        else {
            // Don't create a new polygon if this is the first vertex.
            if (result.newPolygon && i != 0) {
                polygonCount++;
                polygons[polygonCount] = new Polygon();
            }

            if (!result.endOfPath) {
                position = result.position;
                polygons[polygonCount].addVertex(new Vertex(position.x, position.y));
            }
        }
    }

    return polygons;
}

function parseInstruction(data, index, position) {
    var string = data[index];
    var instruction = string.charAt(0);
    var relative = !isUpperCase(instruction);

    var stringCoords = string.slice(1).split(",");

    var result = {
        position: {
            x: 0,
            y: 0
        },
        endOfPath: false,
        newPolygon: false
    }


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

    if (relative) {
        if (position == null)
            console.log("Error: Map reading failed. Relative position requested from no previous position.");

        result.position.x += position.x;
        result.position.y += position.y;
    }

    return result;
}

function drawCountry(ctx, country) {
    var polygons = country.polygons;
    var attitude = country.attitude;

    // TODO: Sometimes the colours are not assigned properly
    if (attitude > 0)
        ctx.fillStyle = neutralColor.blend(positiveColor, attitude).getHex();
    else
        ctx.fillStyle = neutralColor.blend(negativeColor, Math.abs(attitude)).getHex();

    for (var i = 0; i < polygons.length; ++i) {
        var polygon = polygons[i];

        ctx.beginPath();
        ctx.moveTo(polygon.vertices[0].x, polygon.vertices[0].y);

        // TODO: Remove this horrible hack and fix properly.
        if (country.code == "CA") {
            // The ultimate hack.
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

        ctx.fill();

        ctx.save();
        ctx.globalCompositeOperation = "destination-out";
        ctx.strokeStyle = backgroundColor.getRGBAStringFlippedA();
        ctx.lineWidth = 1;
        ctx.stroke();
        ctx.restore();
    }
}

function drawRawMap(canvas, ctx) {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.save();

    var hScale = canvas.width / mapWidth,
		vScale = canvas.height / mapHeight

    var scale = Math.min(hScale, vScale);

    ctx.scale(scale, scale);

    if (hScale > vScale)
        ctx.translate(((canvas.width - (mapWidth * scale)) * 0.5) / scale, 0);
    else
        ctx.translate(0, ((canvas.height - (mapHeight * scale)) * 0.5) / scale);

    for (var c in countries)
        drawCountry(ctx, countries[c]);

    ctx.restore();
}

function isUpperCase(string) {
    return string.toUpperCase() == string;
}



// Class: Country -------------------------------------------------------------

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

    setName: function (code, name) {
        this.code = code;
        this.name = name;
    },

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

    addVertex: function (vertex) {
        this.vertices.push(vertex);
    },

    addIndice: function (indice) {
        this.indices.push(indice);
    },

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

function Vector2(x, y) {
    this.x = x == undefined ? 0 : x;
    this.y = y == undefined ? 0 : y;
}

Vector2.prototype =
{
    x: 0,
    y: 0,

    distance: function (other) {
        if (other === undefined)
            other = new Vector2();

        var dx = other.x - this.x;
        var dy = other.y - this.y;

        return Math.sqrt((dx * dx) + (dy * dy));
    }
}



// Class: Color ---------------------------------------------------------------

function Color(r, g, b, a) {
    this.set(r, g, b, a);
}

Color.prototype =
{
    r: 0,
    g: 0,
    b: 0,
    a: 255,

    set: function (r, g, b, a) {
        a = a === undefined ? 255 : a;

        this.r = Math.round(r);
        this.g = Math.round(g);
        this.b = Math.round(b);
        this.a = Math.round(a);
    },

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

    getRGBAString: function () {
        return "rgba("
		     + (this.r / 1) + ", "
			 + (this.g / 1) + ", "
			 + (this.b / 1) + ", "
			 + (this.a / 255) + ")";
    },

    getRGBAStringFlippedA: function () {
        return "rgba("
		     + (this.r / 1) + ", "
			 + (this.g / 1) + ", "
			 + (this.b / 1) + ", "
			 + (1 - (this.a / 255)) + ")";
    },

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

function MouseState() {
    this.buttons = [];
}

MouseState.prototype = {
    x: 0,
    y: 0,
    wheelPosition: 0,
    buttons: null,

    clone: function () {
        clone = new MouseState();
        clone.x = this.x;
        clone.y = this.y;
        clone.wheelPosition = this.wheelPosition;
        clone.buttons = this.buttons.slice(0);
    }
}