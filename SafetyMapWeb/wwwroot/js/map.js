var map = L.map('map').setView([42.7339, 25.4858], 8);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '© OpenStreetMap contributors'
}).addTo(map);

var populationData = {};
var crimeDataMap = {};
var maxCrimeRate = 0; // Changed to track density instead of absolute count
var categoryColors = {};

var geoJsonLayer;
var geoJsonData;
var legendControl;

var currentView = 'population';
var currentMode = 'view';
var selectedCities = [];

var info = L.control();

info.onAdd = function (map) {
    this._div = L.DomUtil.create('div', 'info');
    info.update();
    return this._div;
};

info.update = function (props) {
    if (!props) {
        this._div.innerHTML = '<h4>Bulgaria Data</h4>Hover over a region';
        return;
    }

    var name = props.name;
    var html = `<h4>Bulgaria Data</h4><b>${name}</b><br />`;

    if (currentView === 'population') {
        html += `${getPopulation(name).toLocaleString()} people`;
    } else {
        var rate = getCrimeRate(name);
        html += `${rate.toFixed(1)} crimes per 1000 people<br/><small class="text-muted">${getCrimeCount(name)} total crimes</small>`;
    }

    this._div.innerHTML = html;
};

info.addTo(map);


Promise.all([
    fetch('/Map/GetPopulationData').then(r => r.json()),
    fetch('/Map/GetCrimeCategories').then(r => r.json()),
    fetch('/bg.json').then(r => r.json())
]).then(([popData, categories, geoJson]) => {

    popData.forEach(item => {
        populationData[item.name] = item.population;
        if (item.name === "Sofia") {
            populationData["Grad Sofiya"] = item.population;
            populationData["Sofia-Grad"] = item.population;
        }
        populationData["Oblast " + item.name] = item.population;
    });

    const categorySelect = document.getElementById('crimeCategorySelect');
    categories.forEach(c => {
        categoryColors[c.id] = c.colorCode || '#b30000';
        let opt = document.createElement('option');
        opt.value = c.id;
        opt.textContent = c.name;
        categorySelect.appendChild(opt);
    });

    geoJsonData = geoJson;

    loadCrimeData('');

    geoJsonLayer = L.geoJson(geoJsonData, {
        style: style,
        onEachFeature: onEachFeature
    }).addTo(map);

    updateLegend();

}).catch(error => console.error('Error loading map data:', error));


function loadCrimeData(categoryId) {
    let url = '/Map/GetCrimeData';
    let params = [];
    if (categoryId) params.push('categoryId=' + categoryId);

    let yearEnabled = document.getElementById('enableYearFilter').checked;
    if (yearEnabled) {
        let year = document.getElementById('yearSlider').value;
        params.push('year=' + year);
    }

    if (params.length > 0) {
        url += '?' + params.join('&');
    }

    fetch(url)
        .then(response => response.json())
        .then(data => {
            crimeDataMap = {};
            data.forEach(item => {
                crimeDataMap[item.cityName] = item;
                if (item.cityName === "Sofia") {
                    crimeDataMap["Grad Sofiya"] = item;
                    crimeDataMap["Sofia-Grad"] = item;
                }
                crimeDataMap["Oblast " + item.cityName] = item;
            });

            // Recalculate the absolute maximum per capita rate for the current dataset
            maxCrimeRate = 0;
            for (let cityName in crimeDataMap) {
                let rate = getCrimeRate(cityName);
                if (rate > maxCrimeRate) {
                    maxCrimeRate = rate;
                }
            }

            if (currentView === 'crime' && geoJsonLayer) {
                geoJsonLayer.setStyle(style);
                updateLegend();
                updateComparisonContainer();
            }
        });
}

function hexToRgb(hex) {
    hex = hex.replace(/^#/, '');
    if (hex.length === 3) {
        hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
    }
    var num = parseInt(hex, 16);
    return { r: (num >> 16) & 255, g: (num >> 8) & 255, b: num & 255 };
}

function mixRgb(rgb, mix) {
    var r = Math.round(255 + (rgb.r - 255) * mix);
    var g = Math.round(255 + (rgb.g - 255) * mix);
    var b = Math.round(255 + (rgb.b - 255) * mix);
    return `rgb(${r},${g},${b})`;
}

function getCrimeColor(d, maxVal) {
    if (maxVal === 0 || d === 0) return '#f4f4f4'; // Light gray for empty cities to distinct from map tiles

    var categoryId = document.getElementById('crimeCategorySelect').value;
    var baseHex = categoryId && categoryColors[categoryId] ? categoryColors[categoryId] : '#b30000';
    var rgb = hexToRgb(baseHex);
    var ratio = d / maxVal;

    if (ratio > 0.9) return mixRgb(rgb, 1.0);
    if (ratio > 0.8) return mixRgb(rgb, 0.9);
    if (ratio > 0.7) return mixRgb(rgb, 0.8);
    if (ratio > 0.6) return mixRgb(rgb, 0.7);
    if (ratio > 0.5) return mixRgb(rgb, 0.6);
    if (ratio > 0.4) return mixRgb(rgb, 0.5);
    if (ratio > 0.3) return mixRgb(rgb, 0.4);
    if (ratio > 0.2) return mixRgb(rgb, 0.3);
    if (ratio > 0.1) return mixRgb(rgb, 0.2);
    return mixRgb(rgb, 0.1);
}

function getColor(d) {
    if (currentView === 'population') {
        return d > 1000000 ? '#08306b' :
            d > 300000 ? '#08519c' :
                d > 100000 ? '#2171b5' :
                    d > 50000 ? '#4292c6' :
                        d > 20000 ? '#6baed6' :
                            d > 10000 ? '#9ecae1' :
                                d > 5000 ? '#c6dbef' :
                                    '#deebf7';
    } else {
        return getCrimeColor(d, maxCrimeRate);
    }
}

function style(feature) {
    var name = feature.properties.name;
    var val = currentView === 'population' ? getPopulation(name) : getCrimeRate(name);

    var weight = 2;
    var color = 'white';
    var dashArray = '3';

    if (currentView === 'crime' && val === 0) {
        color = '#d3d3d3'; // Added visible border for empty cities
    }

    if ((currentMode === 'compareAll' || currentMode === 'compareTwo') && selectedCities.includes(name)) {
        weight = 4;
        color = '#ffcc00';
        dashArray = '';
    }

    return {
        fillColor: getColor(val),
        weight: weight,
        opacity: 1,
        color: color,
        dashArray: dashArray,
        fillOpacity: 0.7
    };
}

function getPopulation(regionName) {
    return populationData[regionName] || 0;
}

function getCrimeCount(regionName) {
    return crimeDataMap[regionName] ? crimeDataMap[regionName].totalCrimes : 0;
}

function getCrimeRate(regionName) {
    var total = getCrimeCount(regionName);
    var pop = getPopulation(regionName);
    if (total === 0 || pop === 0) return 0;
    return (total / pop) * 1000;
}

function highlightFeature(e) {
    var layer = e.target;
    var name = layer.feature.properties.name;

    if (!((currentMode === 'compareAll' || currentMode === 'compareTwo') && selectedCities.includes(name))) {
        layer.setStyle({
            weight: 5,
            color: '#666',
            dashArray: '',
            fillOpacity: 0.7
        });
    } else {
        layer.setStyle({
            weight: 6,
            color: '#ff9900',
            dashArray: ''
        });
    }

    if (!L.Browser.ie && !L.Browser.opera && !L.Browser.edge) {
        try {
            layer.bringToFront();
        } catch (err) { }
    }
    info.update(layer.feature.properties);
}

function resetHighlight(e) {
    geoJsonLayer.resetStyle(e.target);
    info.update();
}

function clickFeature(e) {
    var layer = e.target;
    var name = layer.feature.properties.name;

    if (currentMode === 'view') {
        map.fitBounds(layer.getBounds());
    } else if (currentMode === 'compareAll' || currentMode === 'compareTwo') {
        toggleCompareCity(name);
    }
}

function onEachFeature(feature, layer) {
    layer.on({
        mouseover: highlightFeature,
        mouseout: resetHighlight,
        click: clickFeature
    });
}

function updateLegend() {
    if (legendControl) {
        map.removeControl(legendControl);
    }

    legendControl = L.control({ position: 'bottomright' });

    legendControl.onAdd = function (map) {
        var div = L.DomUtil.create('div', 'info legend');

        if (currentView === 'population') {
            var grades = [0, 5000, 10000, 20000, 50000, 100000, 300000, 1000000];
            for (var i = 0; i < grades.length; i++) {
                div.innerHTML +=
                    '<i style="background:' + getColor(grades[i] + 1) + '"></i> ' +
                    grades[i] + (grades[i + 1] ? '&ndash;' + grades[i + 1] + '<br>' : '+');
            }
        } else {
            if (maxCrimeRate === 0) {
                div.innerHTML += '<i style="background:#f4f4f4"></i> 0 crimes';
                return div;
            }

            var grades = [
                0,
                maxCrimeRate * 0.1,
                maxCrimeRate * 0.2,
                maxCrimeRate * 0.3,
                maxCrimeRate * 0.4,
                maxCrimeRate * 0.5,
                maxCrimeRate * 0.6,
                maxCrimeRate * 0.7,
                maxCrimeRate * 0.8,
                maxCrimeRate * 0.9
            ];

            for (var i = 0; i < grades.length; i++) {
                div.innerHTML +=
                    '<i style="background:' + getColor(grades[i] + 0.01) + '"></i> ' +
                    grades[i].toFixed(1) + (grades[i + 1] ? '&ndash;' + grades[i + 1].toFixed(1) + '<br>' : '+');
            }
        }
        return div;
    };

    legendControl.addTo(map);
}

document.getElementById('viewPopulation').addEventListener('change', function (e) {
    if (this.checked) {
        currentView = 'population';
        document.getElementById('crimeFilterContainer').style.display = 'none';
        document.getElementById('mapTitle').innerText = 'Bulgaria Population Density';
        if (geoJsonLayer) geoJsonLayer.setStyle(style);
        updateLegend();
    }
});

document.getElementById('viewCrime').addEventListener('change', function (e) {
    if (this.checked) {
        currentView = 'crime';
        document.getElementById('crimeFilterContainer').style.display = 'block';
        document.getElementById('mapTitle').innerText = 'Bulgaria Crime Statistics';
        if (geoJsonLayer) geoJsonLayer.setStyle(style);
        updateLegend();
    }
});

document.getElementById('crimeCategorySelect').addEventListener('change', function (e) {
    loadCrimeData(this.value);
});

document.getElementById('enableYearFilter').addEventListener('change', function (e) {
    var sliderContainer = document.getElementById('yearSliderContainer');
    sliderContainer.style.display = this.checked ? 'block' : 'none';

    // Update map title based on state
    if (currentView === 'crime') {
        let title = this.checked ? `Bulgaria Crime Statistics (${document.getElementById('yearSlider').value})` : 'Bulgaria Crime Statistics (All Time)';
        document.getElementById('mapTitle').innerText = title;

        var categoryId = document.getElementById('crimeCategorySelect').value;
        loadCrimeData(categoryId);
    }
});

document.getElementById('yearSlider').addEventListener('input', function (e) {
    document.getElementById('yearLabel').innerText = 'Year: ' + this.value;
    if (currentView === 'crime' && document.getElementById('enableYearFilter').checked) {
        document.getElementById('mapTitle').innerText = `Bulgaria Crime Statistics (${this.value})`;
    }
});

document.getElementById('yearSlider').addEventListener('change', function (e) {
    if (currentView === 'crime' && document.getElementById('enableYearFilter').checked) {
        var categoryId = document.getElementById('crimeCategorySelect').value;
        loadCrimeData(categoryId);
    }
});

document.querySelectorAll('input[name="mapMode"]').forEach(radio => {
    radio.addEventListener('change', function (e) {
        currentMode = this.value;
        if (currentMode === 'view') {
            selectedCities = [];
        } else if (currentMode === 'compareTwo' && selectedCities.length > 2) {
            selectedCities = selectedCities.slice(0, 2);
        }
        updateComparisonContainer();
        if (geoJsonLayer) geoJsonLayer.setStyle(style);
    });
});

document.getElementById('clearComparisonBtn').addEventListener('click', function (e) {
    selectedCities = [];
    updateComparisonContainer();
    if (geoJsonLayer) geoJsonLayer.setStyle(style);
});

function toggleCompareCity(name) {
    var idx = selectedCities.indexOf(name);
    if (idx > -1) {
        selectedCities.splice(idx, 1);
    } else {
        if (currentMode === 'compareTwo' && selectedCities.length >= 2) {
            var removed = selectedCities.shift(); // Remove oldest to keep max 2
        }
        selectedCities.push(name);
    }
    updateComparisonContainer();
    if (geoJsonLayer) geoJsonLayer.setStyle(style);
}

window.toggleCompareCity = toggleCompareCity;

function generateCityCardHTML(cityName) {
    var pop = getPopulation(cityName);
    var crimeRate = getCrimeRate(cityName).toFixed(1);
    var crimeInfo = "";
    var crimeData = crimeDataMap[cityName];
    if (crimeData && crimeData.totalCrimes > 0) {
        crimeInfo = `<p class="mb-1"><strong>Total Crimes:</strong> ${crimeData.totalCrimes}</p>`;
        crimeInfo += `<p class="mb-1"><strong>Crimes per 1,000:</strong> ${crimeRate}</p>`;
        crimeInfo += `<ul class="list-unstyled mb-0" style="font-size: 0.85rem;">`;
        for (let [cat, count] of Object.entries(crimeData.crimesByCategory)) {
            crimeInfo += `<li>${cat}: ${count}</li>`;
        }
        crimeInfo += `</ul>`;
    } else {
        crimeInfo = `<p class="text-muted mb-0">No crime data</p>`;
    }

    return `
        <div class="d-flex justify-content-between align-items-start border-bottom mb-2 pb-1">
            <h6 class="mb-0 text-primary">${cityName}</h6>
            <button type="button" class="btn-close btn-sm" aria-label="Close" onclick="toggleCompareCity('${cityName}'); geoJsonLayer.setStyle(style);"></button>
        </div>
        <p class="mb-1"><strong>Population:</strong> ${pop.toLocaleString()}</p>
        ${crimeInfo}
    `;
}

function updateComparisonContainer() {
    var container = document.getElementById('comparison-container');
    var cardsDiv = document.getElementById('comparison-cards');

    if ((currentMode !== 'compareAll' && currentMode !== 'compareTwo') || selectedCities.length === 0) {
        container.style.display = 'none';
        cardsDiv.innerHTML = '';
        return;
    }

    container.style.display = 'block';
    cardsDiv.innerHTML = '';

    if (currentMode === 'compareTwo' && selectedCities.length === 2) {
        let city1 = selectedCities[0];
        let city2 = selectedCities[1];
        let pop1 = getPopulation(city1);
        let pop2 = getPopulation(city2);
        let crime1 = getCrimeCount(city1);
        let crime2 = getCrimeCount(city2);

        // Build City 1 Card
        let card1 = document.createElement('div');
        card1.className = 'comparison-card flex-fill';
        card1.innerHTML = generateCityCardHTML(city1);
        cardsDiv.appendChild(card1);

        // Build VS Card
        let vsCard = document.createElement('div');
        vsCard.className = 'vs-card';
        let vsHtml = `<div class="vs-text">VS</div>`;

        // Population Diff
        if (pop1 !== pop2) {
            let heroPop = pop1 > pop2 ? city1 : city2;
            let popDiff = Math.abs(pop1 - pop2);
            let popPerc = pop1 && pop2 ? ((popDiff / Math.min(pop1, pop2)) * 100).toFixed(1) + '%' : 'N/A';
            vsHtml += `<div class="diff-badge bg-population">${heroPop} has <b>${popPerc}</b> more population (+${popDiff.toLocaleString()})</div>`;
        } else {
            vsHtml += `<div class="diff-badge bg-population">Equal Population</div>`;
        }

        // Crime Diff (Total)
        if (crime1 !== crime2 && (crime1 > 0 || crime2 > 0)) {
            let heroCrime = crime1 > crime2 ? city1 : city2;
            let crimeDiff = Math.abs(crime1 - crime2);
            let crimePerc = crime1 && crime2 && Math.min(crime1, crime2) > 0 ? ((crimeDiff / Math.min(crime1, crime2)) * 100).toFixed(1) + '%' : (Math.min(crime1, crime2) === 0 ? '100% (No baseline data)' : 'N/A');
            vsHtml += `<div class="diff-badge bg-crime">${heroCrime} has <b>${crimePerc}</b> more total crimes (+${crimeDiff.toLocaleString()})</div>`;
        } else if (crime1 === crime2 && crime1 > 0) {
            vsHtml += `<div class="diff-badge bg-crime">Equal Total Crimes</div>`;
        }

        // Crime Diff (Rate)
        let rate1 = getCrimeRate(city1);
        let rate2 = getCrimeRate(city2);
        if (rate1 !== rate2 && (rate1 > 0 || rate2 > 0)) {
            let heroRate = rate1 > rate2 ? city1 : city2;
            let rateDiff = Math.abs(rate1 - rate2);
            let ratePerc = rate1 && rate2 && Math.min(rate1, rate2) > 0 ? ((rateDiff / Math.min(rate1, rate2)) * 100).toFixed(1) + '%' : (Math.min(rate1, rate2) === 0 ? '100% (No baseline data)' : 'N/A');
            vsHtml += `<div class="diff-badge bg-crime" style="margin-top: 5px; opacity: 0.9;">${heroRate} has a <b>${ratePerc}</b> higher crime rate (+${rateDiff.toFixed(1)} per 1,000)</div>`;
        } else if (rate1 === rate2 && rate1 > 0) {
            vsHtml += `<div class="diff-badge bg-crime" style="margin-top: 5px; opacity: 0.9;">Equal Crime Rate per 1,000</div>`;
        }

        vsCard.innerHTML = vsHtml;
        cardsDiv.appendChild(vsCard);

        // Build City 2 Card
        let card2 = document.createElement('div');
        card2.className = 'comparison-card flex-fill';
        card2.innerHTML = generateCityCardHTML(city2);
        cardsDiv.appendChild(card2);

    } else {
        selectedCities.forEach(cityName => {
            let card = document.createElement('div');
            card.className = 'comparison-card';
            card.innerHTML = generateCityCardHTML(cityName);
            cardsDiv.appendChild(card);
        });
    }
}


