// Image URL debugging and fixing
document.addEventListener('DOMContentLoaded', function() {
    const images = document.querySelectorAll('img[src]');
    
    images.forEach(img => {
        const originalSrc = img.src;
        
        // Log all image URLs for debugging
        console.log('Image source:', originalSrc);
        
        // Fix common URL issues
        if (originalSrc && originalSrc.includes('image.tmdb.org')) {
            // Check if URL is malformed
            if (!originalSrc.startsWith('https://')) {
                console.warn('Invalid TMDB URL format:', originalSrc);
            }
            
            // Log successful loads
            img.addEventListener('load', function() {
                console.log('✓ Image loaded:', originalSrc);
            });
            
            // Log errors with details
            img.addEventListener('error', function() {
                console.error('✗ Image failed to load:', originalSrc);
                console.error('  - Image element:', this);
                console.error('  - Current src:', this.src);
            });
        }
    });
    
    // Also check for movie posters specifically
    const moviePosters = document.querySelectorAll('.movie-poster img, .screening-poster img');
    moviePosters.forEach(img => {
        console.log('Movie poster URL:', img.src || 'NO SRC ATTRIBUTE');
    });
});

