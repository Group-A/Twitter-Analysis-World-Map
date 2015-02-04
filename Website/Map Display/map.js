// TODO: Fix Canada (?)
// TODO: Try and figure out why some maps are completely corrupt. 
//       (Might be a coordinate system thing or some stupid bug.)

var ctx, canvas;
var countries = {};
var negativeColor, neutralColor, positiveColor, backgroundColor;

var mapWidth = 1020;
var mapHeight = 660;

window.onload = function()
{
	negativeColor = new Color(227, 27, 50);
	neutralColor = new Color(255, 255, 255);
	positiveColor = new Color(56, 215, 89);
	backgroundColor = new Color(0, 0, 0, 0);

	canvas = document.getElementById("map");
	ctx = canvas.getContext("2d");
	
	window.onresize = function()
	{
		makeElementFillWindow(canvas);
	}.bind(this);
	
	loadSvg("worldLow.svg", function(content){
		parseSvg(content);
		makeElementFillWindow(canvas);
	});
}

function makeElementFillWindow(element)
{
	element.width = window.innerWidth;
	element.height = window.innerHeight;
	drawMap();
}

function loadSvg(url, callback)
{
	request = null;
	
	if(window.XMLHttpRequest)
		request = new XMLHttpRequest();
	else
		request = new ActiveXObject("Microsoft.XMLHTTP");
	
	if(!request)
		alert("Error: SVG map file could not be loaded.");
		
	request.open("GET", url, true);
	
	request.onload = function(e)
	{
		callback(request.response);
	}.bind(this);
	
	request.send();
}

function parseSvg(content, country)
{
	var parser = new DOMParser();
	svg = parser.parseFromString(content, "text/xml");
	
	var paths = svg.getElementsByTagName("path");
	
	for(var p = 0; p < paths.length; ++p)
	{
		var name = paths[p].getAttribute("id");
		
		if(country === undefined || name == country)
		{
			var polygons = parsePath(paths[p].getAttribute("d"));
			countries[name] = polygons;
		}
	}
}

function parsePath(dataString)
{
	var data = dataString.split(/(?=[a-zA-Z])/);
	
	// The position that relative coordinates will calculate from.
	var position = null;
	
	var polygons = [];
	polygons[0] = new Polygon();
	var polygonCount = 0;
	
	for(var i = 0; i < data.length; ++i)
	{
		result = parseInstruction(data, i, position);
		
		if(result.endOfPath == true)
		{
			position = null;
			if(i < data.length-1)
			{
				result.newPolygon = true;
			}
			else
				break;
		}
		else
		{
			// Don't create a new polygon if this is the first vertex.
			if(result.newPolygon && i != 0) 
			{
				polygonCount++;
				polygons[polygonCount] = new Polygon();
			}
			
			if(!result.endOfPath)
			{
				position = result.position;
				polygons[polygonCount].addVertex(new Vertex(position.x, position.y));
			}
		}
	}
	
	return polygons;
}

function parseInstruction(data, index, position)
{
	var string = data[index];
	var instruction = string.charAt(0);
	var relative = !isUpperCase(instruction);
	
	var stringCoords = string.slice(1).split(",");
	
	var result = {
		position : {
			x : 0,
			y : 0
		},
		endOfPath : false,
		newPolygon : false
	}
	
	
	switch(instruction.toLowerCase())
	{
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
	
	if(relative)
	{
		if(position == null)
			console.log("Error: Map reading failed. Relative position requested from no previous position.");
		
		result.position.x += position.x;
		result.position.y += position.y;
	}
	
	return result;
}

function drawCountry(polygons, attitude)
{
	if(attitude > 0)
		ctx.fillStyle = neutralColor.blend(positiveColor, attitude).getHex();
	else
		ctx.fillStyle = neutralColor.blend(negativeColor, Math.abs(attitude)).getHex();
			
	for(var i = 0; i < polygons.length; ++i)
	{
		var polygon = polygons[i];
		
		ctx.beginPath();
		ctx.moveTo(polygon.vertices[0].x, polygon.vertices[0].y);
		
		for(var t = 1; t < polygon.vertices.length; ++t)
			ctx.lineTo(polygon.vertices[t].x, polygon.vertices[t].y);
			
		ctx.fill();
		
		ctx.save();
		ctx.globalCompositeOperation = "destination-out";
		ctx.strokeStyle = backgroundColor.getRGBAStringFlippedA();
		ctx.lineWidth = 1;
		ctx.stroke();
		ctx.restore();
	}
}

function drawMap()
{
	ctx.fillStyle = backgroundColor.getRGBAString();
	ctx.fillRect(0, 0, canvas.width, canvas.height);

	ctx.save();
	
	var hScale = canvas.width/mapWidth,
		vScale = canvas.height/mapHeight
		
	var scale = Math.min(hScale, vScale);

	ctx.scale(scale, scale);
	
	if(hScale > vScale)
		ctx.translate(((canvas.width - (mapWidth * scale)) * 0.5) / scale, 0);
	else
		ctx.translate(0, ((canvas.height - (mapHeight * scale)) * 0.5) / scale);
	
	for(var c in countries)
		drawCountry(countries[c], (Math.random()*2) - 1);
		
	ctx.restore();
}

function isUpperCase(string)
{
	return string.toUpperCase() == string;
}



// Class: Polygon -------------------------------------------------

function Polygon(data)
{
	this.vertices = [];
	this.indices = [];

	if(data != undefined)
	{
		for(var i = 0; i < data.length; ++i)
		{
			this.vertices.push(new Vertex(data[i], data[++i]));
			this.indices.push(i);
		}
	}
}

Polygon.prototype =
{
	vertices : null,
	indices : null,
	
	addVertex : function(vertex)
	{
		this.vertices.push(vertex);
	},
	
	addIndice : function(indice)
	{
		this.indices.push(indice);
	},
	
	draw : function(ctx)
	{
		var vertex = vertices[0];
		
		ctx.beginPath();
		ctx.moveTo(vertex.x, vertex.y);
		for(var i = 1; i < this.indices.length; ++i)
		{
			vertex = vertices[indices[i]];
			vertex.lineTo(vertex.x, vertex.y);
		}
		ctx.stroke();
	}
}



// Class: Vertex --------------------------------------------------

function Vertex(x, y)
{
	this.x = x == undefined ? 0 : x;
	this.y = y == undefined ? 0 : y;
}

Vertex.prototype = 
{
	x : 0,
	y : 0,
}



// Class: Color ---------------------------------------------------

function Color(r, g, b, a)
{
	this.set(r, g, b, a);
}

Color.prototype = 
{
	r : 0,
	g : 0,
	b : 0,
	a : 255,
	
	set : function(r, g, b, a)
	{
		a = a === undefined ? 255 : a;
	
		this.r = Math.round(r);
		this.g = Math.round(g);
		this.b = Math.round(b);
		this.a = Math.round(a);
	},
	
	getHex : function()
	{
		var values = [];
		
		values.push(this.r.toString(16));
		values.push(this.g.toString(16));
		values.push(this.b.toString(16));
		
		var output = "#";
		
		for(var i = 0; i < 3; ++i)
		{
			if(values[i].length < 2)
				values[i] = "0" + values[i];
			output += values[i];
		}
	
		return output;
	},
	
	getRGBAString : function()
	{
		return "rgba(" 
		     + (this.r/1) + ", "
			 + (this.g/1) + ", "
			 + (this.b/1) + ", "
			 + (this.a/255) + ")";
	},
	
	getRGBAStringFlippedA : function()
	{
		return "rgba(" 
		     + (this.r/1) + ", "
			 + (this.g/1) + ", "
			 + (this.b/1) + ", "
			 + (1 - (this.a/255)) + ")";
	},
	
	blend : function(other, weight)
	{
		var dr = other.r - this.r,
			dg = other.g - this.g,
			db = other.b - this.b,
			da = other.a - this.a;
		
		return new Color(this.r + (dr*weight),
						 this.g + (dg*weight),
						 this.b + (db*weight),
						 this.a + (dr*weight));
	}
}