/* IT Helpdesk Platform */

// 1. Password Visibility Toggle
// Allows users to reveal or hide their password in input fields
function togglePassword(inputId, iconId) {
    const input = document.getElementById(inputId);
    const icon = document.getElementById(iconId);

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

// DOM Initialization - Runs when the HTML is fully loaded
document.addEventListener("DOMContentLoaded", function () {

    // 2. Bootstrap Toasts Initialization
    // Automatically finds and displays any success/error popups (Toasts)
    var toastElList = [].slice.call(document.querySelectorAll('.toast'));
    var toastList = toastElList.map(function (toastEl) {
        return new bootstrap.Toast(toastEl, { delay: 3000 });
    });
    toastList.forEach(toast => toast.show());

    // 3. Back to Top Button Logic
    // Shows a button when scrolling down; clicking it scrolls smoothly to the top
    const backToTopBtn = document.getElementById("btnBackToTop");

    if (backToTopBtn) {
        // Show/hide the button based on vertical scroll position
        window.addEventListener("scroll", function () {
            if (window.scrollY > 300) {
                backToTopBtn.classList.add("show");
            } else {
                backToTopBtn.classList.remove("show");
            }
        });

        // Custom smooth scrolling animation logic
        backToTopBtn.addEventListener("click", function () {
            const targetPosition = 0;
            const startPosition = window.scrollY;
            const distance = targetPosition - startPosition;
            const duration = 500; // Animation duration in milliseconds
            let start = null;

            // Easing function for a natural slow-start, slow-end scroll feel
            function easeInOutQuad(t, b, c, d) {
                t /= d / 2;
                if (t < 1) return c / 2 * t * t + b;
                t--;
                return -c / 2 * (t * (t - 2) - 1) + b;
            }

            // Animation loop
            function animation(currentTime) {
                if (start === null) start = currentTime;
                const timeElapsed = currentTime - start;
                const run = easeInOutQuad(timeElapsed, startPosition, distance, duration);

                window.scrollTo(0, run);

                if (timeElapsed < duration) {
                    requestAnimationFrame(animation);
                } else {
                    window.scrollTo(0, 0); // Ensure it stops exactly at the top
                }
            }

            requestAnimationFrame(animation);
        });
    }

    // 4. Dark Mode Theme Toggle
    // Handles switching between light and dark themes and saves user preference
    const toggleBtn = document.getElementById('darkModeToggle');
    const themeIcon = document.getElementById('themeIcon');

    // Check if the user has a saved preference; otherwise, default to 'dark'
    const currentTheme = localStorage.getItem('theme') || 'dark';

    // Apply the initial theme on page load
    document.documentElement.setAttribute('data-bs-theme', currentTheme);
    updateIcon(currentTheme);

    // Listen for clicks on the dark mode toggle button
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            const current = document.documentElement.getAttribute('data-bs-theme');
            const newTheme = current === 'dark' ? 'light' : 'dark';

            // Apply the new theme and save it to the browser's localStorage
            document.documentElement.setAttribute('data-bs-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateIcon(newTheme);
        });
    }

    // Helper function to swap the sun/moon icon based on the active theme
    function updateIcon(theme) {
        if (!themeIcon) return;

        if (theme === 'dark') {
            themeIcon.classList.remove('bi-moon-fill');
            themeIcon.classList.add('bi-sun-fill', 'text-warning'); // Yellow sun
        } else {
            themeIcon.classList.remove('bi-sun-fill', 'text-warning');
            themeIcon.classList.add('bi-moon-fill'); // Standard moon
        }
    }

});

/**
 * 5. Updates the hidden input field by combining the country code and the phone digits.
 * This runs every time the user types a digit or changes the dropdown.
 */
function updateHiddenPhone() {
    let prefix = document.getElementById('countryCode').value;
    let digits = document.getElementById('phoneDigits').value.trim();
    
    // Combine them (e.g., "+40 777123456")
    if (digits.length > 0) {
        document.getElementById('fullPhoneNumber').value = prefix + ' ' + digits;
    } else {
        document.getElementById('fullPhoneNumber').value = '';
    }
}

/**
 * On page load, split the database phone number (e.g., "+40 774877546") 
 * so the dropdown and the input field show the correct values.
 */
document.addEventListener("DOMContentLoaded", function() {
    let fullPhoneInput = document.getElementById('fullPhoneNumber');
    
    if (fullPhoneInput && fullPhoneInput.value) {
        let fullPhone = fullPhoneInput.value;
        let spaceIndex = fullPhone.indexOf(' '); // Find where the prefix ends
        
        if (spaceIndex !== -1) {
            // Split by the first space
            let prefix = fullPhone.substring(0, spaceIndex);
            let digits = fullPhone.substring(spaceIndex + 1);
            
            // Set the UI elements
            document.getElementById('countryCode').value = prefix;
            document.getElementById('phoneDigits').value = digits;
        } else {
            // Fallback if there is no space saved in the database
            document.getElementById('phoneDigits').value = fullPhone;
        }
    }
});

function verifyAccountSimulated(email, redirectUrl, $btn, $errorDiv) {
    // Show a "verifying" state
    $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Verifying...');

    $.post('/Account/VerifyAccountManual', { email: email }, function (res) {
        if (res.success) {
            // Update the UI to show the final success state
            $errorDiv.removeClass('alert-warning').addClass('alert-success')
                .html('<i class="bi bi-shield-check text-success" style="font-size: 3rem;"></i><br><br>Identity Verified!<br><small class="text-muted fs-6">Redirecting to your dashboard...</small>');

            $btn.fadeOut(500, function () {
                window.location.href = redirectUrl;
            });
        } else {
            $errorDiv.removeClass('alert-success').addClass('alert-danger').text('Verification failed. Please try again.');
            $btn.prop('disabled', false).text('Verify Account Now');
        }
    });
}