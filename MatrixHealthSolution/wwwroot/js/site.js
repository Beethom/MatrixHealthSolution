// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Modal Functions
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'block';
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
    }
}

// Close modal when clicking outside of it
window.onclick = function(event) {
    if (event.target.classList.contains('modal')) {
        event.target.style.display = 'none';
    }
}

// Handle Login Form Submission
function handleLogin(event) {
    event.preventDefault();
    
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    
    // TODO: Implement actual login logic with backend
    console.log('Login attempt:', { email, password });
    
    // Temporary: Just close modal and show success
    alert('Login functionality will be implemented with backend!');
    closeModal('loginModal');
    
    // Reset form
    event.target.reset();
}

// Handle Signup Form Submission
function handleSignup(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const data = Object.fromEntries(formData.entries());
    
    // TODO: Implement actual signup logic with backend
    console.log('Signup attempt:', data);
    
    // Temporary: Just close modal and show success
    alert('Signup functionality will be implemented with backend!');
    closeModal('signupModal');
    
    // Reset form
    event.target.reset();
}

// Handle Booking Form Submission
function handleBooking(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const data = Object.fromEntries(formData.entries());
    
    // TODO: Implement actual booking logic with backend
    console.log('Booking attempt:', data);
    
    // Temporary: Just close modal and show success
    alert('Booking request received! We will contact you shortly to confirm.');
    closeModal('bookingModal');
    
    // Reset form
    event.target.reset();
}

// Purchase Membership
function purchaseMembership(planType) {
    // TODO: Implement actual membership purchase logic
    console.log('Purchasing membership:', planType);
    
    const plans = {
        basic: { name: 'Basic Wellness', price: 79 },
        premium: { name: 'Premium Serenity', price: 149 },
        ultimate: { name: 'Ultimate Luxury', price: 249 }
    };
    
    const selectedPlan = plans[planType];
    if (selectedPlan) {
        alert(`You selected the ${selectedPlan.name} plan ($${selectedPlan.price}/month). Payment integration coming soon!`);
    }
}

// Purchase Gift Card
function purchaseGiftCard(amount) {
    // TODO: Implement actual gift card purchase logic
    console.log('Purchasing gift card:', amount);
    
    alert(`You selected a $${amount} gift card. Payment integration coming soon!`);
}

// Handle Custom Gift Card Modal
function handleCustomGiftCard(event) {
    event.preventDefault();
    
    const amount = document.getElementById('customAmount').value;
    const recipientName = document.getElementById('recipientName').value;
    const recipientEmail = document.getElementById('recipientEmail').value;
    const message = document.getElementById('giftMessage').value;
    
    // TODO: Implement actual custom gift card logic
    console.log('Custom gift card:', { amount, recipientName, recipientEmail, message });
    
    alert(`Custom gift card for $${amount} will be sent to ${recipientEmail}. Payment integration coming soon!`);
    closeModal('customGiftCardModal');
    
    // Reset form
    event.target.reset();
}

// Smooth Scrolling for Anchor Links
document.addEventListener('DOMContentLoaded', function() {
    const links = document.querySelectorAll('a[href^="#"]');
    
    links.forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            
            // Don't prevent default for modal triggers
            if (href === '#' || this.hasAttribute('onclick')) {
                return;
            }
            
            e.preventDefault();
            
            const targetId = href.substring(1);
            const targetElement = document.getElementById(targetId);
            
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});

// Add loading animation for forms (optional enhancement)
function showLoading(buttonElement) {
    const originalText = buttonElement.innerHTML;
    buttonElement.innerHTML = '<span>Processing...</span>';
    buttonElement.disabled = true;
    
    // Simulate processing (remove this in production)
    setTimeout(() => {
        buttonElement.innerHTML = originalText;
        buttonElement.disabled = false;
    }, 2000);
}

// Console welcome message
console.log('%c🌿 Welcome to Serenity Wellness', 'color: #4CAF50; font-size: 20px; font-weight: bold;');
console.log('%cThis site is under development. Backend functionality coming soon!', 'color: #666; font-size: 12px;');