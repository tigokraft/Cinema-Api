// Netflix-style header scroll effect
document.addEventListener('DOMContentLoaded', function() {
    const header = document.querySelector('.netflix-header');
    if (header) {
        window.addEventListener('scroll', function() {
            if (window.scrollY > 50) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        });
    }
});

// Image error handling for TMDB images
document.addEventListener('DOMContentLoaded', function() {
    const images = document.querySelectorAll('img[src*="image.tmdb.org"]');
    images.forEach(img => {
        img.addEventListener('error', function() {
            this.style.display = 'none';
            const placeholder = this.parentElement.querySelector('.movie-placeholder');
            if (!placeholder) {
                const div = document.createElement('div');
                div.className = 'movie-placeholder';
                div.innerHTML = '<span>📷</span>';
                this.parentElement.appendChild(div);
            }
        });
    });
});
