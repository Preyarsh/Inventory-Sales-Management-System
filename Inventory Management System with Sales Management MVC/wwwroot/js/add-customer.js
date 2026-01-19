document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('statusToggle');
    var isActiveInput = document.getElementById('isActiveInput');
    var createForm = document.getElementById('createCustomerForm');

    // Initialize toggle
    toggle.classList.add('toggle-switch');
    isActiveInput.value = 'true';

    toggle.addEventListener('click', function () {
        this.classList.toggle('off');
        var isActive = !this.classList.contains('off');
        isActiveInput.value = isActive;
    });

    // Form submission
    if (createForm) {
        createForm.addEventListener('submit', function (e) {
            var customerNameEl = document.getElementById('CustomerName');
            var emailEl = document.getElementById('Email');
            var mobileEl = document.getElementById('CustomerMobile');
            var billingEl = document.getElementById('BillingAddress');
            var shippingEl = document.getElementById('ShippingAddress');

            var customerName = customerNameEl.value.trim();
            var email = emailEl.value.trim();
            var mobile = mobileEl.value.trim();
            var billingAddress = billingEl.value.trim();
            var shippingAddress = shippingEl.value.trim();

            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                e.preventDefault();
                Swal.fire({
                    title: 'Invalid Email!',
                    text: 'Please enter a valid email address.',
                    icon: 'error',
                    confirmButtonColor: '#D6336C'
                });
                return false;
            }

            var mobileRegex = /^[0-9]{10}$/;
            var cleanMobile = mobile.replace(/[-\s]/g, '');
            if (!mobileRegex.test(cleanMobile)) {
                e.preventDefault();
                Swal.fire({
                    title: 'Invalid Mobile!',
                    text: 'Please enter a valid 10-digit mobile number.',
                    icon: 'error',
                    confirmButtonColor: '#D6336C'
                });
                return false;
            }

            if (!billingAddress || billingAddress.length < 10) {
                e.preventDefault();
                Swal.fire({
                    title: 'Invalid Address!',
                    text: 'Billing address must be at least 10 characters long.',
                    icon: 'error',
                    confirmButtonColor: '#D6336C'
                });
                return false;
            }

            if (!shippingAddress || shippingAddress.length < 10) {
                e.preventDefault();
                Swal.fire({
                    title: 'Invalid Address!',
                    text: 'Shipping address must be at least 10 characters long.',
                    icon: 'error',
                    confirmButtonColor: '#D6336C'
                });
                return false;
            }

            var submitBtn = document.getElementById('submitBtn');
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creating Customer...';
            submitBtn.disabled = true;
        });
    }

    // Check for success message
    var successAlert = document.querySelector('.alert-success');
    if (successAlert) {
        var alertText = successAlert.querySelector('span').textContent;
        Swal.fire({
            title: 'Success!',
            text: alertText,
            icon: 'success',
            confirmButtonColor: '#D6336C',
            timer: 2000
        }).then(function () {
            window.location.href = '/Customer/Index';
        });
    }
});