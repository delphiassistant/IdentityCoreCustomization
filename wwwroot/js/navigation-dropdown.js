// Navigation Dropdown Enhancements
document.addEventListener('DOMContentLoaded', function() {
    console.log('Navigation dropdown enhancements loaded');
    
    // Initialize dropdown with enhanced functionality
    const userDropdown = document.getElementById('userAccountDropdown');
    
    if (userDropdown) {
        console.log('User account dropdown found');
        
        // Add smooth animation when dropdown opens/closes
        const dropdownMenu = userDropdown.nextElementSibling;
        
        if (dropdownMenu) {
            // Enhanced dropdown show/hide with animations
            userDropdown.addEventListener('click', function(e) {
                const isExpanded = this.getAttribute('aria-expanded') === 'true';
                
                if (!isExpanded) {
                    // About to show - add entering animation
                    dropdownMenu.style.opacity = '0';
                    dropdownMenu.style.transform = 'translateY(-10px)';
                    
                    setTimeout(() => {
                        dropdownMenu.style.opacity = '1';
                        dropdownMenu.style.transform = 'translateY(0)';
                    }, 10);
                }
            });
            
            // Close dropdown when clicking outside
            document.addEventListener('click', function(e) {
                if (!userDropdown.contains(e.target) && !dropdownMenu.contains(e.target)) {
                    if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
                        const dropdownInstance = bootstrap.Dropdown.getInstance(userDropdown);
                        if (dropdownInstance) {
                            dropdownInstance.hide();
                        }
                    }
                }
            });
            
            // Add hover effects to dropdown items
            const dropdownItems = dropdownMenu.querySelectorAll('.dropdown-item');
            dropdownItems.forEach(item => {
                item.addEventListener('mouseenter', function() {
                    if (this.style.transform !== undefined) {
                        this.style.transform = 'translateX(5px)';
                    }
                });
                
                item.addEventListener('mouseleave', function() {
                    if (this.style.transform !== undefined) {
                        this.style.transform = 'translateX(0)';
                    }
                });
            });
        }
        
        // Add user status indicator
        console.log('User account dropdown initialized successfully');
    }
    
    // Initialize Bootstrap tooltips for dropdown items
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('.dropdown-item[title]'));
        const tooltipList = tooltipTriggerList.map(function(tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl, {
                placement: 'left',
                delay: { show: 500, hide: 100 }
            });
        });
        console.log('Tooltips initialized:', tooltipList.length);
    }
    
    // Add keyboard navigation support
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            const openDropdowns = document.querySelectorAll('.dropdown-menu.show');
            openDropdowns.forEach(dropdown => {
                const dropdownToggle = dropdown.previousElementSibling;
                if (dropdownToggle && typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
                    const dropdownInstance = bootstrap.Dropdown.getInstance(dropdownToggle);
                    if (dropdownInstance) {
                        dropdownInstance.hide();
                    }
                }
            });
        }
    });
    
    // Add loading state to logout button
    const logoutForm = document.querySelector('form[action*="Logout"]');
    if (logoutForm) {
        const logoutButton = logoutForm.querySelector('button[type="submit"]');
        if (logoutButton) {
            logoutButton.addEventListener('click', function() {
                this.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> در حال خروج...';
                this.disabled = true;
                this.classList.add('loading');
            });
        }
    }
    
    // Add click tracking for admin menu items (for analytics)
    const adminItems = document.querySelectorAll('[visible-to-roles="Admin"] .dropdown-item');
    adminItems.forEach(item => {
        item.addEventListener('click', function() {
            const action = this.textContent.trim();
            console.log('Admin action clicked:', action);
        });
    });
    
    console.log('Navigation dropdown initialization completed');
});

// Utility function to check if user has admin role (for debugging)
window.checkUserRole = function() {
    const adminElements = document.querySelectorAll('[visible-to-roles*="Admin"]');
    if (adminElements.length > 0) {
        console.log('User has Admin role - Admin menu items visible');
        return true;
    } else {
        console.log('User does not have Admin role - Admin menu items hidden');
        return false;
    }
};

// Function to toggle dropdown programmatically (for debugging)
window.toggleUserDropdown = function() {
    const userDropdown = document.getElementById('userAccountDropdown');
    if (userDropdown && typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
        const dropdownInstance = bootstrap.Dropdown.getOrCreateInstance(userDropdown);
        dropdownInstance.toggle();
        console.log('User dropdown toggled');
    } else {
        console.log('Bootstrap Dropdown not available or element not found');
    }
};

// Function to simulate admin role check (for testing)
window.simulateAdminRole = function(hasAdmin = true) {
    const adminElements = document.querySelectorAll('[visible-to-roles="Admin"]');
    adminElements.forEach(element => {
        element.style.display = hasAdmin ? 'block' : 'none';
    });
    console.log('Admin elements', hasAdmin ? 'shown' : 'hidden');
};