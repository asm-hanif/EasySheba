// Directions Button: Open Google Maps in new tab with from/to locations
window.showDirectionsModal = function(btn) {
    const hospitalLocation = btn.getAttribute('data-hospital-location');
    if (!hospitalLocation) {
        alert('Hospital address not available.');
        return;
    }
    // Try to use user's current location as origin
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function(position) {
            const userLat = position.coords.latitude;
            const userLng = position.coords.longitude;
            const mapsUrl = `https://www.google.com/maps/dir/?api=1&origin=${userLat},${userLng}&destination=${encodeURIComponent(hospitalLocation)}&travelmode=driving`;
            window.open(mapsUrl, '_blank');
        }, function() {
            // If permission denied or unavailable, use My+Location as origin (Google will prompt for permission)
            const mapsUrl = `https://www.google.com/maps/dir/?api=1&origin=My+Location&destination=${encodeURIComponent(hospitalLocation)}&travelmode=driving`;
            window.open(mapsUrl, '_blank');
        });
    } else {
        // Fallback: use My+Location as origin
        const mapsUrl = `https://www.google.com/maps/dir/?api=1&origin=My+Location&destination=${encodeURIComponent(hospitalLocation)}&travelmode=driving`;
        window.open(mapsUrl, '_blank');
    }
};
