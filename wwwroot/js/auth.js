/**
 * auth.js — Shared JavaScript for all authentication pages.
 * Loaded globally via _AuthLayout.cshtml.
 */

// ---------------------------------------------------------------------------
// Password visibility toggle
// Used by: Register, ResetPassword
// ---------------------------------------------------------------------------
function togglePassword(inputId, iconId) {
    const input = document.getElementById(inputId);
    const icon = document.getElementById(iconId);

    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.replace('fa-eye', 'fa-eye-slash');
    } else {
        input.type = 'password';
        icon.classList.replace('fa-eye-slash', 'fa-eye');
    }
}

// ---------------------------------------------------------------------------
// Password strength checker
// Used by: Register, ResetPassword
// Expects: #passwordInput, #passwordStrength, [data-requirement] elements
// ---------------------------------------------------------------------------
function initPasswordStrength() {
    const passwordInput = document.getElementById('passwordInput');
    if (!passwordInput) return;

    passwordInput.addEventListener('input', function () {
        const password = this.value;
        const strengthBar = document.getElementById('passwordStrength');
        if (!strengthBar) return;

        const checks = {
            length:    password.length >= 6,
            uppercase: /[A-Z]/.test(password),
            lowercase: /[a-z]/.test(password),
            number:    /\d/.test(password)
        };

        let strength = 0;
        Object.keys(checks).forEach(key => {
            const item = document.querySelector(`[data-requirement="${key}"]`);
            if (!item) return;

            if (checks[key]) {
                item.classList.add('requirement-met');
                strength++;
            } else {
                item.classList.remove('requirement-met');
            }
        });

        strengthBar.className = 'password-strength';
        if      (strength === 0) strengthBar.classList.add('bg-secondary');
        else if (strength <= 1)  strengthBar.classList.add('bg-danger');
        else if (strength <= 2)  strengthBar.classList.add('bg-warning');
        else if (strength <= 3)  strengthBar.classList.add('bg-info');
        else                     strengthBar.classList.add('bg-success');
    });
}

// ---------------------------------------------------------------------------
// OTP / numeric code input
// Strips non-digits and auto-submits when maxlength is reached.
// Used by: PreRegisterConfirm, LoginWithSmsResponse, LoginWith2faSms, LoginWith2fa
// ---------------------------------------------------------------------------
function initOtpInput(inputId) {
    const input = inputId
        ? document.getElementById(inputId)
        : document.querySelector('.auth-code-input');

    if (!input) return;

    input.addEventListener('input', function () {
        this.value = this.value.replace(/[^0-9]/g, '');

        const maxLen = parseInt(this.getAttribute('maxlength') || '6', 10);
        if (this.value.length === maxLen) {
            this.form.classList.add('auth-loading');
            setTimeout(() => this.form.submit(), 300);
        }
    });
}

// ---------------------------------------------------------------------------
// Recovery code input
// Strips non-alphanumeric characters and uppercases the value.
// Used by: LoginWithRecoveryCode
// ---------------------------------------------------------------------------
function initRecoveryCodeInput() {
    const input = document.getElementById('RecoveryCode');
    if (!input) return;

    input.addEventListener('input', function () {
        this.value = this.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    });
}

// ---------------------------------------------------------------------------
// Form loading state on valid submit
// Adds .auth-loading to the form so the button shows a spinner.
// Pass a CSS selector or leave blank to target the first <form>.
// Used by: all auth pages
// ---------------------------------------------------------------------------
function initFormLoadingState(formSelector) {
    const form = formSelector
        ? document.querySelector(formSelector)
        : document.querySelector('form');

    if (!form) return;

    form.addEventListener('submit', function () {
        if ($(this).valid()) {
            this.classList.add('auth-loading');
        }
    });
}

// ---------------------------------------------------------------------------
// Boot — called once the DOM is ready
// ---------------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', function () {
    initPasswordStrength();
    initOtpInput();
    initRecoveryCodeInput();
    initFormLoadingState();
});
