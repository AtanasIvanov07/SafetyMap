var map = L.map('map').setView([42.7339, 25.4858], 8);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap contributors'
}).addTo(map);

var populationData = {};
var geoJsonLayer;


fetch('/Map/GetPopulationData')
    .then(response => response.json())
    .then(data => {

        data.forEach(item => {
            populationData[item.name] = item.population;

            if (item.name === "Sofia") {
                populationData["Grad Sofiya"] = item.population;
                populationData["Sofia-Grad"] = item.population;
            }

            populationData["Oblast " + item.name] = item.population;
        });

        return fetch('/bg.json');
    })
    .then(response => response.json())
    .then(geoJson => {
        geoJsonLayer = L.geoJson(geoJson, {
            style: style,
            onEachFeature: onEachFeature
        }).addTo(map);
    })
    .catch(error => console.error('Error loading map data:', error));


function getColor(d) {
    return d > 1000000 ? '#08306b' :
        d > 300000 ? '#08519c' :
            d > 100000 ? '#2171b5' :
                d > 50000 ? '#4292c6' :
                    d > 20000 ? '#6baed6' :
                        d > 10000 ? '#9ecae1' :
                            d > 5000 ? '#c6dbef' :
                                '#deebf7';
}

function style(feature) {
    var name = feature.properties.name;
    var population = getPopulation(name);

    return {
        fillColor: getColor(population),
        weight: 2,
        opacity: 1,
        color: 'white',
        dashArray: '3',
        fillOpacity: 0.7
    };
}

function getPopulation(regionName) {

    if (populationData[regionName]) return populationData[regionName];


    return 0;
}


function highlightFeature(e) {
    var layer = e.target;

    layer.setStyle({
        weight: 5,
        color: '#666',
        dashArray: '',
        fillOpacity: 0.7
    });

    layer.bringToFront();
    info.update(layer.feature.properties);
}

function resetHighlight(e) {
    geoJsonLayer.resetStyle(e.target);
    info.update();
}

function zoomToFeature(e) {
    map.fitBounds(e.target.getBounds());
}

function onEachFeature(feature, layer) {
    layer.on({
        mouseover: highlightFeature,
        mouseout: resetHighlight,
        click: zoomToFeature
    });
}


var info = L.control();

info.onAdd = function (map) {
    this._div = L.DomUtil.create('div', 'info');
    this.update();
    return this._div;
};

info.update = function (props) {
    var pop = props ? getPopulation(props.name) : 0;
    this._div.innerHTML = '<h4>Bulgaria Population</h4>' + (props ?
        '<b>' + props.name + '</b><br />' + pop + ' people'
        : 'Hover over a region');
};

info.addTo(map);


var legend = L.control({ position: 'bottomright' });

legend.onAdd = function (map) {

    var div = L.DomUtil.create('div', 'info legend'),
        grades = [0, 5000, 10000, 20000, 50000, 100000, 300000, 1000000],
        labels = [];

    for (var i = 0; i < grades.length; i++) {
        div.innerHTML +=
            '<i style="background:' + getColor(grades[i] + 1) + '"></i> ' +
            grades[i] + (grades[i + 1] ? '&ndash;' + grades[i + 1] + '<br>' : '+');
    }

    return div;
};

legend.addTo(map);
