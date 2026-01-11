// EmailJS Configuration and Utility Functions
// Service ID: service_qz6sbsp
// Public Key: 3JJbf4JB2-x-unn4F

(function() {
    // Initialize EmailJS
    if (typeof emailjs !== 'undefined') {
        emailjs.init({ publicKey: "3JJbf4JB2-x-unn4F" });
    }
})();

// Send ticket confirmation email
function sendTicketConfirmationEmail(emailData) {
    if (typeof emailjs === 'undefined') {
        console.warn('EmailJS not loaded');
        return Promise.reject('EmailJS not loaded');
    }
    
    return emailjs.send('service_qz6sbsp', 'template_ticket', emailData);
}

// Send ticket cancellation email
function sendTicketCancellationEmail(emailData) {
    if (typeof emailjs === 'undefined') {
        console.warn('EmailJS not loaded');
        return Promise.reject('EmailJS not loaded');
    }
    
    return emailjs.send('service_qz6sbsp', 'template_cancel', emailData);
}
