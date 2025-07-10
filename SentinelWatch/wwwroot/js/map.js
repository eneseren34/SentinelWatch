document.addEventListener('DOMContentLoaded', function () {
    var mapElement = document.getElementById('map');

    if (mapElement) {
        var map = L.map('map').setView([41.0082, 28.9784], 10);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);

        var markers = L.markerClusterGroup(); // Initialize cluster group

        console.log("Map initialized. Now fetching reports...");

        function capitalizeWords(str) {
            if (!str) return str;
            return str.split(' ')
                .map(word => word.charAt(0).toUpperCase() + word.slice(1))
                .join(' ');
        }

        async function fetchAndDisplayReports() {
            try {
                const response = await fetch('/api/Reports');
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const reports = await response.json();
                console.log("Fetched reports:", reports);

                reports.forEach(report => {
                    if (report.latitude != null && report.longitude != null) {
                        const marker = L.marker([report.latitude, report.longitude]);

                        let popupContent = `<b>${report.category || 'Report'}</b>`;
                        if (report.severity) {
                            popupContent += `<br>Severity: ${report.severity}`;
                        }
                        popupContent += `<br>Time: ${new Date(report.timestamp).toLocaleString()}`;
                        popupContent += `<hr><div id="weather-info-${report.id}" style="font-size: 0.9em; margin-top: 5px;"><i>Loading weather...</i></div>`;

                        marker.bindPopup(popupContent);

                        marker.on('popupopen', async (e) => {
                            const popup = e.popup;
                            const lat = popup.getLatLng().lat;
                            const lon = popup.getLatLng().lng;
                            const weatherDiv = popup.getElement().querySelector(`#weather-info-${report.id}`);

                            if (weatherDiv) {
                                try {
                                    // *** CORRECTED FETCH URL ***
                                    const weatherResponse = await fetch(`/api/Weather?lat=${lat}&lon=${lon}`);
                                    if (!weatherResponse.ok) throw new Error('Weather fetch failed');
                                    const weatherData = await weatherResponse.json();
                                    const weatherHtml = `
                                        <b>Current Weather:</b><br>
                                        ${capitalizeWords(weatherData.weather[0].description) || 'N/A'}<br>
                                        Temp: ${weatherData.main.temp}°C<br>
                                        Humidity: ${weatherData.main.humidity}%`;
                                    weatherDiv.innerHTML = weatherHtml;
                                } catch (err) {
                                    weatherDiv.innerHTML = '<i>Could not load weather.</i>';
                                    console.error("Weather fetch failed for marker:", err);
                                }
                            }
                        });
                        markers.addLayer(marker); // Add marker to the cluster group
                    } else {
                        console.warn("Report skipped due to missing coordinates:", report.id);
                    }
                });
                map.addLayer(markers); // Add the whole cluster group to the map
            } catch (error) {
                console.error("Could not fetch or display reports:", error);
            }
        }

        fetchAndDisplayReports(); // Call the function to load reports

        async function onMapClick(e) {
            var lat = e.latlng.lat.toFixed(6);
            var lng = e.latlng.lng.toFixed(6);
            let weatherHtml = '<i>Loading weather...</i>';

            try {
                // *** CORRECTED FETCH URL ***
                const response = await fetch(`/api/Weather?lat=${lat}&lon=${lng}`);
                if (!response.ok) throw new Error('Weather fetch failed');
                const weather = await response.json();
                weatherHtml = `<b>Current Weather:</b><br>
                               ${capitalizeWords(weather.weather[0].description)}, ${weather.main.temp}°C`;
            } catch (err) {
                weatherHtml = '<i>Could not load weather.</i>';
                console.error("Weather fetch failed for new report:", err);
            }

            // *** CORRECTED HTML FORM ***
            var formHtml = `
                <h4>Create New Report</h4>
                <p>At: ${lat}, ${lng}</p>
                <div style="margin-bottom: 10px; font-style: italic; font-size: 0.9em;">${weatherHtml}</div>
                <hr>
                <form id="popupForm">
                    <input type="hidden" name="latitude" value="${lat}" />
                    <input type="hidden" name="longitude" value="${lng}" />
                    <div style="margin-bottom: 10px;">
                        <label for="category">Category:</label><br>
                        <input type="text" id="category" name="category" required />
                    </div>
                    <div style="margin-bottom: 10px;">
                        <label for="severity">Severity:</label><br>
                        <input type="text" id="severity" name="severity" />
                    </div>
                    <button type="submit">Submit Report</button>
                </form>
                <div id="popupMessage" style="color: red; margin-top: 10px;"></div>
            `;

            var popup = L.popup()
                .setLatLng(e.latlng)
                .setContent(formHtml)
                .openOn(map);

            setTimeout(() => {
                const popupForm = document.getElementById('popupForm');
                if (popupForm) {
                    popupForm.addEventListener('submit', handlePopupSubmit);
                }
            }, 100);
        }

        async function handlePopupSubmit(event) {
            event.preventDefault();
            const form = event.target;
            const formData = new FormData(form);
            const data = Object.fromEntries(formData.entries());
            const popupMessageDiv = document.getElementById('popupMessage');
            console.log("Submitting:", data);

            try {
                const response = await fetch('/api/Reports', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });

                if (!response.ok) {
                    const errorText = await response.text();
                    throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
                }

                const newReport = await response.json();
                console.log("Report created:", newReport);

                map.closePopup();
                const newMarker = L.marker([newReport.latitude, newReport.longitude])
                    .bindPopup(`<b>${newReport.category}</b><br>Just Added!`);
                markers.addLayer(newMarker); // Add to cluster group
                newMarker.openPopup();       // Open the popup for the new marker

            } catch (error) {
                console.error("Failed to submit report:", error);
                if (popupMessageDiv) {
                    popupMessageDiv.textContent = 'Failed to submit. Try again.';
                }
            }
        }

        map.on('click', onMapClick); // Attach the click listener to the map

    } else {
        console.log("Map container #map not found on this page.");
    }
});