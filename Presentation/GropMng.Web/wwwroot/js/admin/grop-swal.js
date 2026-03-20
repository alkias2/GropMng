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
        const Swal = getSwal();
        if (!Swal) return window.confirm(options?.text || options?.title || 'Are you sure?');

        const result = await Swal.fire({
            icon: options?.icon || 'warning',
            title: options?.title || 'Are you sure?',
            text: options?.text || '',
            showCancelButton: true,
            confirmButtonText: options?.confirmButtonText || 'Yes',
            cancelButtonText: options?.cancelButtonText || 'Cancel',
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

                const ok = await confirm({
                    icon: form.dataset.confirmIcon || 'warning',
                    title: form.dataset.confirmTitle || 'Are you sure?',
                    text: form.dataset.confirmText || '',
                    confirmButtonText: form.dataset.confirmButtonText || 'Yes',
                    cancelButtonText: form.dataset.cancelButtonText || 'Cancel'
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