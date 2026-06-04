// ===== Mobile Menu =====
const hamburger = document.getElementById('hamburger');
const mobileMenu = document.getElementById('mobileMenu');

hamburger.addEventListener('click', () => {
    hamburger.classList.toggle('active');
    mobileMenu.classList.toggle('hidden');
});

document.querySelectorAll('.mobile-link').forEach(link => {
    link.addEventListener('click', () => {
        hamburger.classList.remove('active');
        mobileMenu.classList.add('hidden');
    });
});

// ===== Navbar Scroll Effect =====
const navbar = document.getElementById('navbar');
window.addEventListener('scroll', () => {
    if (window.scrollY > 50) {
        navbar.classList.add('border-b', 'border-border');
    } else {
        navbar.classList.remove('border-b', 'border-border');
    }
});

// ===== Scroll Reveal Animation =====
const revealElements = document.querySelectorAll('.reveal');
const revealObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('active');
        }
    });
}, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

revealElements.forEach(el => revealObserver.observe(el));

// ===== Animated Counters =====
const counters = document.querySelectorAll('.counter');
let countersAnimated = false;

const counterObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting && !countersAnimated) {
            countersAnimated = true;
            counters.forEach(counter => {
                const target = parseInt(counter.getAttribute('data-target'));
                const duration = 2000;
                const step = target / (duration / 16);
                let current = 0;

                const updateCounter = () => {
                    current += step;
                    if (current < target) {
                        counter.textContent = Math.floor(current).toLocaleString();
                        requestAnimationFrame(updateCounter);
                    } else {
                        counter.textContent = target.toLocaleString();
                    }
                };
                updateCounter();
            });
        }
    });
}, { threshold: 0.5 });

const counterSection = document.getElementById('counters');
if (counterSection) counterObserver.observe(counterSection);

// ===== Toast Notification =====
function showToast(message, type = 'success') {
    const toast = document.getElementById('toast');
    const toastMessage = document.getElementById('toastMessage');
    toast.className = `toast toast-${type} show`;
    toastMessage.textContent = message;
    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

// ===== Toggle Password Visibility =====
function togglePassword(inputId, btn) {
    const input = document.getElementById(inputId);
    const icon = btn.querySelector('i');
    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        input.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}

// ===== Validation Helpers =====
function validateEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function validateCNIC(cnic) {
    return /^\d{5}-\d{7}-\d$/.test(cnic);
}

function showError(elementId, message) {
    const el = document.getElementById(elementId);
    el.textContent = message;
    el.classList.remove('hidden');
}

function hideError(elementId) {
    const el = document.getElementById(elementId);
    el.textContent = '';
    el.classList.add('hidden');
}

// ===== Contact Form Validation =====
function handleContactSubmit(e) {
    e.preventDefault();
    let isValid = true;

    const name = document.getElementById('contactName').value.trim();
    const email = document.getElementById('contactEmail').value.trim();
    const subject = document.getElementById('contactSubject').value.trim();
    const message = document.getElementById('contactMessage').value.trim();

    ['contactNameError', 'contactEmailError', 'contactSubjectError', 'contactMessageError'].forEach(hideError);

    if (!name) {
        showError('contactNameError', 'Name is required');
        isValid = false;
    }

    if (!email) {
        showError('contactEmailError', 'Email is required');
        isValid = false;
    } else if (!validateEmail(email)) {
        showError('contactEmailError', 'Please enter a valid email');
        isValid = false;
    }

    if (!subject) {
        showError('contactSubjectError', 'Subject is required');
        isValid = false;
    }

    if (!message) {
        showError('contactMessageError', 'Message is required');
        isValid = false;
    } else if (message.length < 10) {
        showError('contactMessageError', 'Message must be at least 10 characters');
        isValid = false;
    }

    if (isValid) {
        showToast('Message sent successfully! We\'ll get back to you soon.', 'success');
        document.getElementById('contactForm').reset();
    }
}

// ===== Newsletter =====
// function handleNewsletter(e) {
//     e.preventDefault();
//     showToast('Subscribed successfully! Thank you.', 'success');
//     e.target.reset();
// }

// ===== Premium Calculator =====
const calcTerm = document.getElementById('calcTerm');
const calcTermValue = document.getElementById('calcTermValue');

calcTerm.addEventListener('input', () => {
    calcTermValue.textContent = calcTerm.value + ' years';
});

function calculatePremium() {
    const type = document.getElementById('calcType').value;
    const coverage = parseFloat(document.getElementById('calcCoverage').value);
    const age = parseInt(document.getElementById('calcAge').value);
    const term = parseInt(document.getElementById('calcTerm').value);

    if (!coverage || !age) {
        showToast('Please fill in coverage amount and age', 'error');
        return;
    }

    const rates = {
        life: 0.0005,
        medical: 0.0008,
        motor: 0.0012,
        home: 0.0003
    };

    const ageFactor = age > 50 ? 1.5 : age > 35 ? 1.2 : 1;
    const termFactor = term > 25 ? 0.8 : 1;
    const baseRate = rates[type] * ageFactor * termFactor;

    const monthlyPremium = (coverage * baseRate) / 12;
    const annualPremium = monthlyPremium * 12;
    const totalPremium = annualPremium * term;

    document.getElementById('calcPremium').textContent = '$' + monthlyPremium.toFixed(2);
    document.getElementById('calcAnnual').textContent = '$' + annualPremium.toFixed(2);
    document.getElementById('calcTotal').textContent = '$' + totalPremium.toFixed(2);

    showToast('Premium calculated successfully!', 'info');
}

// ===== Hero Premium Calculator =====
const heroCoverage = document.getElementById('heroCoverage');
const heroCoverageValue = document.getElementById('heroCoverageValue');

heroCoverage.addEventListener('input', () => {
    heroCoverageValue.textContent = parseInt(heroCoverage.value).toLocaleString();
});

function calculateHeroPremium() {
    const type = document.getElementById('heroInsType').value;
    const coverage = parseFloat(heroCoverage.value);
    const age = parseInt(document.getElementById('heroAge').value);

    if (!age || age < 18 || age > 100) {
        showToast('Please enter a valid age (18-100)', 'error');
        return;
    }

    const rates = {
        life: 0.0005,
        medical: 0.0008,
        motor: 0.0012,
        home: 0.0003
    };

    const ageFactor = age > 50 ? 1.5 : age > 35 ? 1.2 : 1;
    const monthlyPremium = (coverage * rates[type] * ageFactor) / 12;

    document.getElementById('heroPremiumResult').classList.remove('hidden');
    document.getElementById('heroPremiumAmount').textContent = '$' + monthlyPremium.toFixed(2) + '/mo';
}

// ===== Smooth Scroll for Anchor Links =====
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            const offset = 80;
            const targetPosition = target.getBoundingClientRect().top + window.pageYOffset - offset;
            window.scrollTo({
                top: targetPosition,
                behavior: 'smooth'
            });
        }
    });
});

// ===== Initialize =====
document.addEventListener('DOMContentLoaded', () => {
    revealElements.forEach(el => revealObserver.observe(el));
});

// ========== TAB NAVIGATION JS ==========
function updateTabGlider(tabIndex) {
    const tabLinks = document.querySelectorAll('.tab-link');
    const glider = document.getElementById('tabGlider');
    const activeTab = tabLinks[tabIndex];

    if (activeTab && glider) {
        const container = activeTab.parentElement;
        const containerRect = container.getBoundingClientRect();
        const tabRect = activeTab.getBoundingClientRect();

        const leftOffset = tabRect.left - containerRect.left;
        const tabWidth = tabRect.width;

        glider.style.transform = `translateX(${leftOffset}px)`;
        glider.style.width = `${tabWidth}px`;

        tabLinks.forEach((link, idx) => {
            if (idx === tabIndex) {
                link.classList.add('active');
                link.setAttribute('aria-selected', 'true');
            } else {
                link.classList.remove('active');
                link.setAttribute('aria-selected', 'false');
            }
        });

        const radios = document.querySelectorAll('.tab-radio');
        radios.forEach((radio, idx) => {
            radio.checked = (idx === tabIndex);
        });
    }
}

function initTabNavigation() {
    const tabLinks = document.querySelectorAll('.tab-link');

    updateTabGlider(0);

    tabLinks.forEach((link, index) => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            updateTabGlider(index);

            const targetId = link.getAttribute('href');
            const target = document.querySelector(targetId);
            if (target) {
                const offset = 80;
                const targetPosition = target.getBoundingClientRect().top + window.pageYOffset - offset;
                window.scrollTo({
                    top: targetPosition,
                    behavior: 'smooth'
                });
            }

            history.pushState(null, null, targetId);
        });

        link.addEventListener('keydown', (e) => {
            if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
                e.preventDefault();
                const direction = e.key === 'ArrowRight' ? 1 : -1;
                let newIndex = index + direction;

                if (newIndex < 0) newIndex = tabLinks.length - 1;
                if (newIndex >= tabLinks.length) newIndex = 0;

                tabLinks[newIndex].focus();
                updateTabGlider(newIndex);
            }
        });
    });

    window.addEventListener('hashchange', () => {
        const hash = window.location.hash || '#home';
        const tabMap = {
            '#home': 0,
            '#services': 1,
            '#features': 2,
            '#testimonials': 3,
            '#contact': 4
        };
        if (tabMap[hash] !== undefined) {
            updateTabGlider(tabMap[hash]);
        }
    });

    let resizeTimeout;
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            const activeIndex = Array.from(tabLinks).findIndex(link => link.classList.contains('active'));
            if (activeIndex >= 0) {
                updateTabGlider(activeIndex);
            }
        }, 100);
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initTabNavigation);
} else {
    initTabNavigation();
}
