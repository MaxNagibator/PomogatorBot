let subscriptionMeta = {};

async function loadSubscriptionMeta() {
    try {
        const response = await fetch('/subscriptions/meta');
        let data = await response.json();
        subscriptionMeta = data.subscriptionMetas;
        renderSubscriptions();
    } catch (error) {
        console.error('Ошибка загрузки подписок:', error);
        showNotification('❌ Не удалось загрузить типы подписок', '#ff6699');
    }
}

function renderSubscriptions() {
    const container = document.getElementById('subscription-container');
    container.innerHTML = '';

    subscriptionMeta.forEach(meta => {
        const value = meta.subscription;
        const label = meta.icon + meta.displayName || meta.description;
        const color = meta.color || '#00bcd4';

        const labelElement = `
            <label class="subscription-item" style="--sub-color: ${color}">
                <input type="checkbox" name="subscribes" value="${value}">
                <span class="checkmark"></span>
                <span class="subscription-label">${label}</span>
            </label>
        `;
        container.insertAdjacentHTML('beforeend', labelElement);
    });

    const checkboxes = container.querySelectorAll('input[type="checkbox"]');
    const savedSubs = JSON.parse(localStorage.getItem('selectedSubscriptions')) || [];

    checkboxes.forEach(checkbox => {
        checkbox.checked = savedSubs.includes(checkbox.value);
        checkbox.addEventListener('change', handleCheckboxChange);
    });
}

function handleCheckboxChange() {
    const selectedSubs = Array.from(
        document.querySelectorAll('#subscription-container input[type="checkbox"]:checked')
    ).map(checkbox => checkbox.value);
    localStorage.setItem('selectedSubscriptions', JSON.stringify(selectedSubs));
}
