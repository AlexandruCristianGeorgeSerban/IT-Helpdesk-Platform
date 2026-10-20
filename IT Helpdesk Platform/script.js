/* IT Helpdesk Platform */

// 1. Function to show/hide password (Used in login.html)
function togglePassword(inputId, iconId) {
    const input = document.getElementById(inputId);
    const icon = document.getElementById(iconId);
    
    // Check if the elements exist on the page to avoid errors
    if (input && icon) {
        if (input.type === "password") {
            input.type = "text";
            icon.classList.remove("bi-eye");
            icon.classList.add("bi-eye-slash");
        } else {
            input.type = "password";
            icon.classList.remove("bi-eye-slash");
            icon.classList.add("bi-eye");
        }
    }
}

// 2. Function for the create ticket form (Used in create-ticket.html)
// Use DOMContentLoaded to ensure the page is fully loaded before looking for the form
document.addEventListener("DOMContentLoaded", function() {
    
    const ticketForm = document.getElementById('newTicketForm');
    
    // If the form is found on the current page, activate the logic
    if (ticketForm) {
        ticketForm.addEventListener('submit', function(event) {
            event.preventDefault();
            
            // Show the success Toast
            const toastElement = document.getElementById('successToast');
            if (toastElement) {
                const toast = new bootstrap.Toast(toastElement);
                toast.show();
            }

            // Redirect the user to the ticket list after 2 seconds
            setTimeout(() => {
                window.location.href = 'ticket-list.html';
            }, 2000);
        });
    }
});