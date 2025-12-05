// Dynamic Theme Color Extraction from Featured Movie Backdrop
// Extracts dominant color and applies to site theme

/**
 * Extract dominant color from an image using Canvas API
 * @param {string} imageUrl - URL of the image
 * @param {function} callback - Callback function that receives {r, g, b}
 */
function extractDominantColor(imageUrl, callback) {
    const img = new Image();
    img.crossOrigin = 'Anonymous';
    
    img.onerror = function() {
        console.warn('Failed to load image for color extraction:', imageUrl);
    };
    
    img.onload = function() {
        try {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            
            // Sample from center area for better results
            const sampleSize = 100;
            canvas.width = sampleSize;
            canvas.height = sampleSize;
            
            ctx.drawImage(img, 0, 0, sampleSize, sampleSize);
            const imageData = ctx.getImageData(0, 0, sampleSize, sampleSize);
            const data = imageData.data;
            
            // Count color frequency
            const colorMap = {};
            for (let i = 0; i < data.length; i += 4) {
                const r = data[i];
                const g = data[i + 1];
                const b = data[i + 2];
                const a = data[i + 3];
                
                // Skip transparent and very dark/light pixels
                if (a < 125 || (r + g + b) < 50 || (r + g + b) > 700) continue;
                
                // Group similar colors (reduce to 10-value buckets)
                const key = `${Math.floor(r/10)*10},${Math.floor(g/10)*10},${Math.floor(b/10)*10}`;
                colorMap[key] = (colorMap[key] || 0) + 1;
            }
            
            // Find most frequent color
            let maxCount = 0;
            let dominantColor = null;
            for (const [color, count] of Object.entries(colorMap)) {
                if (count > maxCount) {
                    maxCount = count;
                    dominantColor = color;
                }
            }
            
            if (dominantColor) {
                const [r, g, b] = dominantColor.split(',').map(Number);
                callback({ r, g, b });
            } else {
                console.warn('No dominant color found');
            }
        } catch (error) {
            console.error('Error extracting color:', error);
        }
    };
    
    img.src = imageUrl;
}

/**
 * Convert RGB to HSL for CSS variables
 * @param {number} r - Red (0-255)
 * @param {number} g - Green (0-255)
 * @param {number} b - Blue (0-255)
 * @returns {object} - {h, s, l}
 */
function rgbToHsl(r, g, b) {
    r /= 255;
    g /= 255;
    b /= 255;
    
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    let h, s, l = (max + min) / 2;
    
    if (max === min) {
        h = s = 0; // achromatic
    } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        
        switch (max) {
            case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
            case g: h = ((b - r) / d + 2) / 6; break;
            case b: h = ((r - g) / d + 4) / 6; break;
        }
    }
    
    return {
        h: Math.round(h * 360),
        s: Math.round(s * 100),
        l: Math.round(l * 100)
    };
}

/**
 * Apply theme color extracted from backdrop
 * @param {string} backdropUrl - URL of the backdrop image
 */
function applyThemeColor(backdropUrl) {
    extractDominantColor(backdropUrl, (rgb) => {
        const hsl = rgbToHsl(rgb.r, rgb.g, rgb.b);
        
        // Adjust lightness for better visibility
        const primaryL = Math.min(hsl.l, 55);
        const accentL = Math.min(hsl.l + 10, 65);
        
        // Update CSS variables
        document.documentElement.style.setProperty('--primary', `${hsl.h} ${hsl.s}% ${primaryL}%`);
        document.documentElement.style.setProperty('--accent', `${hsl.h} ${Math.min(hsl.s + 15, 100)}% ${accentL}%`);
        
        // Also update muted colors with same hue
        document.documentElement.style.setProperty('--muted', `${hsl.h} ${Math.max(hsl.s - 40, 15)}% 25%`);
        
        // Store in localStorage for persistence across page loads
        localStorage.setItem('themeColor', JSON.stringify({ 
            h: hsl.h, 
            s: hsl.s, 
            l: hsl.l,
            rgb: rgb
        }));
        
        console.log('Theme color applied:', `hsl(${hsl.h}, ${hsl.s}%, ${hsl.l}%)`);
    });
}

/**
 * Load theme color from localStorage
 */
function loadSavedThemeColor() {
    const saved = localStorage.getItem('themeColor');
    if (saved) {
        try {
            const { h, s, l } = JSON.parse(saved);
            const primaryL = Math.min(l, 55);
            const accentL = Math.min(l + 10, 65);
            
            document.documentElement.style.setProperty('--primary', `${h} ${s}% ${primaryL}%`);
            document.documentElement.style.setProperty('--accent', `${h} ${Math.min(s + 15, 100)}% ${accentL}%`);
            document.documentElement.style.setProperty('--muted', `${h} ${Math.max(s - 40, 15)}% 25%`);
        } catch (error) {
            console.error('Error loading saved theme:', error);
        }
    }
}

/**
 * Initialize theme color on page load
 */
document.addEventListener('DOMContentLoaded', () => {
    // Load saved theme first (instant)
    loadSavedThemeColor();
    
    // Then check for featured hero and extract fresh color
    const featuredHero = document.querySelector('.featured-hero');
    if (featuredHero) {
        const backgroundImage = featuredHero.style.backgroundImage;
        const backdropUrl = backgroundImage.match(/url\(['"]?([^'"]+)['"]?\)/)?.[1];
        
        if (backdropUrl) {
            console.log('Extracting theme color from:', backdropUrl);
            applyThemeColor(backdropUrl);
        }
    }
    
    // Also extract from movie detail backdrop
    const movieBackdrop = document.querySelector('.movie-detail-backdrop');
    if (movieBackdrop) {
        const backgroundImage = movieBackdrop.style.backgroundImage;
        const backdropUrl = backgroundImage.match(/url\(['"]?([^'"]+)['"]?\)/)?.[1];
        
        if (backdropUrl) {
            console.log('Extracting theme color from movie backdrop:', backdropUrl);
            applyThemeColor(backdropUrl);
        }
    }
});
