(function (window) {
    'use strict';

    function getSwal() {
        return window.Swal;
    }

    async function alert(options) {
        const Swal = getSwal();
        if (!Swal) {
            window.alert(options?.text || options?.title || 'Notification');
            return;
        }

        await Swal.fire({
            icon: options?.icon || 'info',
            title: options?.title || 'Notification',
            text: options?.text || '',
            confirmButtonText: options?.confirmButtonText || 'OK',
            customClass: {
                confirmButton: 'btn btn-primary'
            },
            buttonsStyling: false
        });
    }

    async function confirm(options) {
        const texts = window.gropCommonTexts || {};
        const Swal = getSwal();
        if (!Swal) return window.confirm(options?.text || texts.deleteText || options?.title || texts.deleteTitle || 'Are you sure?');

        const result = await Swal.fire({
            icon: options?.icon || 'warning',
            title: options?.title || texts.deleteTitle || 'Are you sure?',
            text: options?.text || texts.deleteText || 'Are you sure you want to perform this action?',
            showCancelButton: true,
            confirmButtonText: options?.confirmButtonText || texts.deleteButtonText || 'Yes',
            cancelButtonText: options?.cancelButtonText || texts.cancelButtonText || 'No, cancel',
            reverseButtons: true,
            customClass: {
                confirmButton: options?.confirmButtonClass || 'btn btn-danger me-2',
                cancelButton: options?.cancelButtonClass || 'btn btn-outline-secondary'
            },
            buttonsStyling: false
        });

        return !!result.isConfirmed;
    }

    async function notify(options) {
        const Swal = getSwal();
        if (!Swal) return;

        await Swal.fire({
            toast: true,
            position: options?.position || 'top-end',
            icon: options?.icon || 'success',
            title: options?.title || '',
            timer: options?.timer || 2500,
            showConfirmButton: false,
            timerProgressBar: true
        });
    }

    function bindConfirmForms(selector) {
        document.querySelectorAll(selector).forEach(function (form) {
            if (form.dataset.gropConfirmBound === 'true') return;
            form.dataset.gropConfirmBound = 'true';

            form.addEventListener('submit', async function (event) {
                event.preventDefault();

                const texts = window.gropCommonTexts || {};
                const ok = await confirm({
                    icon: form.dataset.confirmIcon || 'warning',
                    title: form.dataset.confirmTitle || texts.deleteTitle || 'Are you sure?',
                    text: form.dataset.confirmText || texts.deleteText || 'Are you sure you want to perform this action?',
                    confirmButtonText: form.dataset.confirmButtonText || texts.deleteButtonText || 'Yes',
                    cancelButtonText: form.dataset.cancelButtonText || texts.cancelButtonText || 'No, cancel'
                });

                if (ok) form.submit();
            });
        });
    }

    window.GropSwal = {
        alert: alert,
        confirm: confirm,
        notify: notify,
        bindConfirmForms: bindConfirmForms
    };

    document.addEventListener('DOMContentLoaded', function () {
        bindConfirmForms("form[data-grop-confirm='true'], form[data-grop-delete='true']");
    });
})(window);